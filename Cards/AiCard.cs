using AICardMod.AI;
using AICardMod.UI;
using BaseLib.Abstracts;
using BaseLib.Cards.Variables;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class AiCard : CustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public AiCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    private bool _hasTransformed = false;
    private List<(string effect, int value)> _effectsList = [];
    private string _cardNameSuffix = "";
    private bool _isCardExhaust = true;   // original card is always exhaust
    private bool _isCardEthereal = false;

    // ExhaustiveVar makes BaseLib treat this card as Exhaust (keyword shown on card, goes to exhaust pile)
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        _isCardExhaust ? [new ExhaustiveVar(1)] : [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_hasTransformed)
        {
            GD.Print($"[AICard] Executing stored effects: {string.Join(", ", _effectsList.Select(e => $"{e.effect} {e.value}"))}");
            await ApplyEffects(_effectsList, choiceContext);
            return;
        }

        // First play: ask player for request
        var playerRequest = await AiInputDialog.ShowAsync();
        if (string.IsNullOrWhiteSpace(playerRequest))
        {
            GD.Print("[AICard] Player cancelled — returning card to hand.");
            await CardPileCmd.AddGeneratedCardToCombat(
                this,
                PileType.Hand,
                addedByPlayer: true,
                CardPilePosition.Top);
            return;
        }

        // ── Read Piety ───────────────────────────────────────────────────────
        // Use LINQ to avoid generic constraint on GetPower<T>
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        string pietyTier = GetPietyTier(piety);
        GD.Print($"[AICard] Piety: {piety} ({pietyTier})");

        GD.Print($"[AICard] Player request: \"{playerRequest}\"");
        GD.Print("[AICard] Calling AI...");

        var aiResponse = await OllamaClient.AskAsync(BuildPrompt(playerRequest, piety, pietyTier));
        GD.Print($"[AICard] AI response:\n{aiResponse}");

        ParseAiResponse(aiResponse, out var effectsList, out var cardName, out var isExhaust, out var isEthereal, out var cost);
        GD.Print($"[AICard] Effects: {string.Join(", ", effectsList.Select(e => $"{e.Item1} {e.Item2}"))}");
        GD.Print($"[AICard] Name: {cardName}, Cost: {cost}, Exhaust: {isExhaust}, Ethereal: {isEthereal}");

        // Fallback if AI returned nothing useful
        if (effectsList.Count == 0)
        {
            GD.Print("[AICard] No effects parsed, using fallback.");
            if (piety == 0)
            {
                effectsList.Add(("PLAYER_WEAK", 2));
                cardName = "神罰";
                cost = 0;
            }
            else
            {
                effectsList.Add(("BLOCK", 8));
                cardName = "防盾";
                cost = 1;
            }
        }

        // ── Consume all Piety ────────────────────────────────────────────────
        if (piety > 0)
        {
            GD.Print($"[AICard] Consuming {piety} piety.");
            await PowerCmd.Apply<PietyPower>(Owner.Creature, -piety, Owner.Creature, null);
        }

        // Create a transformed clone and add to hand
        var clone = (AiCard)CreateClone();
        clone._effectsList = new List<(string, int)>(effectsList);
        clone._hasTransformed = true;
        clone._cardNameSuffix = cardName;
        clone._isCardExhaust = isExhaust;
        clone._isCardEthereal = isEthereal;

        var description = GenerateDescription(effectsList, isExhaust, isEthereal);
        clone.UpdateCardAppearance(description, cardName, cost);

        await CardPileCmd.AddGeneratedCardToCombat(
            clone,
            PileType.Hand,
            addedByPlayer: true,
            CardPilePosition.Top);

        GD.Print($"[AICard] Generated card '神諭 - {cardName}' added to hand.");
    }

    // ── Piety tier helpers ────────────────────────────────────────────────────

    private static string GetPietyTier(int piety) => piety switch
    {
        0 => "ANGERED",
        <= 2 => "DISPLEASED",
        <= 5 => "NEUTRAL",
        <= 9 => "BLESSED",
        _ => "DIVINE"
    };

    // ── Effect keyword sets ───────────────────────────────────────────────────

    private static readonly HashSet<string> KnownEffects = new(StringComparer.OrdinalIgnoreCase)
    {
        "DAMAGE", "BLOCK", "DRAW", "HEAL", "ENERGY", "STRENGTH", "DEXTERITY", "FOCUS", "GOLD",
        "VULNERABLE", "WEAK", "FRAIL",
        "PLAYER_WEAK", "PLAYER_VULNERABLE",
        "COST", "CARD_EXHAUST", "CARD_ETHEREAL"
    };

    // These keywords carry no numeric value
    private static readonly HashSet<string> NoValueEffects = new(StringComparer.OrdinalIgnoreCase)
    {
        "CARD_EXHAUST", "CARD_ETHEREAL"
    };

    // These are card-property keywords, not played effects
    private static readonly HashSet<string> CardPropertyEffects = new(StringComparer.OrdinalIgnoreCase)
    {
        "COST", "CARD_EXHAUST", "CARD_ETHEREAL"
    };

    // ── Parse AI response ────────────────────────────────────────────────────

    private static void ParseAiResponse(
        string response,
        out List<(string, int)> effects,
        out string cardName,
        out bool isExhaust,
        out bool isEthereal,
        out int cost)
    {
        effects = [];
        cardName = "變化";
        isExhaust = false;
        isEthereal = false;
        cost = 1;

        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim().TrimStart('-', '*', '•', ' '))
            .Where(l => l.Length > 0)
            .ToList();

        // The card name is the last non-effect line
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (!TryParseEffectLine(lines[i], out _, out _))
            {
                cardName = lines[i];
                break;
            }
        }

        // Parse all effect lines
        foreach (var line in lines)
        {
            if (!TryParseEffectLine(line, out var effectName, out var value)) continue;

            switch (effectName)
            {
                case "CARD_EXHAUST":
                    isExhaust = true;
                    break;
                case "CARD_ETHEREAL":
                    isEthereal = true;
                    break;
                case "COST":
                    cost = Math.Clamp(value, 0, 5);
                    break;
                default:
                    effects.Add((effectName, value));
                    break;
            }
        }
    }

    private static bool TryParseEffectLine(string line, out string effectName, out int value)
    {
        effectName = "";
        value = 1;

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;

        var candidate = parts[0].ToUpperInvariant();
        if (!KnownEffects.Contains(candidate)) return false;

        effectName = candidate;

        if (NoValueEffects.Contains(candidate))
            return true;

        if (parts.Length < 2) return false;
        var numPart = new string(parts[1].TakeWhile(c => char.IsDigit(c) || c == '-').ToArray());
        return int.TryParse(numPart, out value);
    }

    // ── Generate STS-style description ───────────────────────────────────────

    private static string GenerateDescription(List<(string effect, int value)> effects, bool isExhaust, bool isEthereal)
    {
        var lines = new List<string>();

        foreach (var (effectName, value) in effects)
        {
            string line = effectName.ToUpperInvariant() switch
            {
                "DAMAGE"           => $"造成 {value} 點傷害。",
                "BLOCK"            => $"獲得 {value} 格擋。",
                "DRAW"             => $"抽 {value} 張牌。",
                "HEAL"             => $"回復 {value} 點生命值。",
                "ENERGY"           => $"獲得 {value} 點能量。",
                "STRENGTH"         => $"獲得 {value} 層力量。",
                "DEXTERITY"        => $"獲得 {value} 層敏捷。",
                "FOCUS"            => $"獲得 {value} 層聚焦。",
                "GOLD"             => $"獲得 {value} 金幣。",
                "VULNERABLE"       => $"對敵人施加 {value} 層易傷。",
                "WEAK"             => $"對敵人施加 {value} 層虛弱。",
                "FRAIL"            => $"對敵人施加 {value} 層脆弱。",
                "PLAYER_WEAK"      => $"[red]你獲得 {value} 層虛弱。[/red]",
                "PLAYER_VULNERABLE"=> $"[red]你獲得 {value} 層易傷。[/red]",
                _                  => $"{effectName} {value}"
            };
            lines.Add(line);
        }

        return string.Join("\n", lines);
    }

    // ── Update clone appearance ───────────────────────────────────────────────

    private void UpdateCardAppearance(string description, string nameSuffix, int cost)
    {
        try
        {
            var cardKey = Id.Entry;
            var baseTitle = new LocString("cards", "AICARDMOD-AI_CARD.title").GetFormattedText();
            var fullTitle = $"{baseTitle} - {nameSuffix}";

            var table = LocManager.Instance.GetTable("cards");
            table.MergeWith(new Dictionary<string, string>
            {
                [$"{cardKey}.title"] = fullTitle,
                [$"{cardKey}.description"] = description
            });

            EnergyCost.SetThisCombat(cost);
            GD.Print($"[AICard] Appearance updated → {fullTitle} (cost {cost})");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AICard] UpdateCardAppearance failed: {ex.Message}");
        }
    }

    // ── Apply effects when the transformed card is played ────────────────────

    private async Task ApplyEffects(List<(string effect, int value)> effects, PlayerChoiceContext choiceContext)
    {
        foreach (var (effectName, value) in effects)
        {
            try { await ApplySingleEffect(effectName, value, choiceContext); }
            catch (Exception ex) { GD.PrintErr($"[AICard] Failed to apply {effectName} {value}: {ex.Message}"); }
        }
    }

    private async Task ApplySingleEffect(string effectName, int value, PlayerChoiceContext choiceContext)
    {
        effectName = effectName.ToUpperInvariant();
        GD.Print($"[AICard] Applying {effectName} {value}");

        switch (effectName)
        {
            case "DAMAGE":
                var dmgTarget = GetRandomEnemy();
                if (dmgTarget != null)
                    await DamageCmd.Attack(value).FromCard(this).Targeting(dmgTarget).Execute(choiceContext);
                break;

            case "BLOCK":
                await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(value, 0), null);
                break;

            case "DRAW":
                await CardPileCmd.Draw(choiceContext, value, Owner);
                break;

            case "HEAL":
                await CreatureCmd.Heal(Owner.Creature, value);
                break;

            case "ENERGY":
                await PlayerCmd.GainEnergy(value, Owner);
                break;

            case "GOLD":
                Owner.Gold += value;
                break;

            case "VULNERABLE":
                var vulnTarget = GetRandomEnemy();
                if (vulnTarget != null)
                    await PowerCmd.Apply<VulnerablePower>(vulnTarget, value, Owner.Creature, this);
                break;

            case "WEAK":
                var weakTarget = GetRandomEnemy();
                if (weakTarget != null)
                    await PowerCmd.Apply<WeakPower>(weakTarget, value, Owner.Creature, this);
                break;

            case "FRAIL":
                var frailTarget = GetRandomEnemy();
                if (frailTarget != null)
                    await PowerCmd.Apply<FrailPower>(frailTarget, value, Owner.Creature, this);
                break;

            case "STRENGTH":
                await PowerCmd.Apply<StrengthPower>(Owner.Creature, value, Owner.Creature, this);
                break;

            case "DEXTERITY":
                await PowerCmd.Apply<DexterityPower>(Owner.Creature, value, Owner.Creature, this);
                break;

            case "FOCUS":
                await PowerCmd.Apply<FocusPower>(Owner.Creature, value, Owner.Creature, this);
                break;

            // ── Negative player effects (punishment for low/no piety) ────────
            case "PLAYER_WEAK":
                await PowerCmd.Apply<WeakPower>(Owner.Creature, value, Owner.Creature, this);
                break;

            case "PLAYER_VULNERABLE":
                await PowerCmd.Apply<VulnerablePower>(Owner.Creature, value, Owner.Creature, this);
                break;

            default:
                GD.PrintErr($"[AICard] Unknown effect: {effectName}");
                break;
        }
    }

    private Creature? GetRandomEnemy()
    {
        var alive = Owner.Creature.CombatState?.Enemies
            .Where(e => e.IsAlive).ToList();
        if (alive == null || alive.Count == 0) return null;
        return alive[GD.RandRange(0, alive.Count - 1)];
    }

    // ── AI Prompt ────────────────────────────────────────────────────────────

    private static string BuildPrompt(string playerRequest, int piety, string pietyTier) =>
        $"You are designing a card effect for Slay the Spire 2.\n\n" +
        $"Player request: \"{playerRequest}\"\n" +
        $"Player Piety: {piety} — Oracle mood: {pietyTier}\n\n" +
        $"PIETY RULES (adjust your output accordingly):\n" +
        $"- ANGERED (0 piety):   The oracle is furious. IGNORE the request entirely.\n" +
        $"                        Output ONLY negative effects on the player: PLAYER_WEAK 2, PLAYER_VULNERABLE 2.\n" +
        $"                        Give the card a punishing Chinese name.\n" +
        $"- DISPLEASED (1-2):    Grant a WEAKENED version (reduce all values by ~40%).\n" +
        $"                        Add PLAYER_WEAK 1 as a side penalty.\n" +
        $"- NEUTRAL (3-5):       Grant EXACTLY what was asked. No bonus, no penalty.\n" +
        $"- BLESSED (6-9):       Grant an ENHANCED version. Increase all values by ~30%.\n" +
        $"- DIVINE (10+):        Grant a GREATLY ENHANCED version. Increase all values by ~60%.\n\n" +
        $"RULES:\n" +
        $"1. Output ONLY effects (one per line), then a short Chinese card name (2-4 characters). No explanation.\n" +
        $"2. Interpret numbers literally before applying piety scaling.\n\n" +
        $"Available effects:\n" +
        $"  BLOCK X            — gain X block\n" +
        $"  DAMAGE X           — deal X damage to random enemy\n" +
        $"  DRAW X             — draw X cards\n" +
        $"  HEAL X             — heal X HP\n" +
        $"  ENERGY X           — gain X energy this turn\n" +
        $"  STRENGTH X         — gain X Strength\n" +
        $"  DEXTERITY X        — gain X Dexterity\n" +
        $"  GOLD X             — gain X gold\n" +
        $"  PLAYER_WEAK X      — player gets X Weak (negative)\n" +
        $"  PLAYER_VULNERABLE X— player gets X Vulnerable (negative)\n\n" +
        $"Example — NEUTRAL piety, player asked \"80 block\":\n" +
        $"BLOCK 80\n" +
        $"鐵壁\n\n" +
        $"Example — ANGERED, player asked \"draw 3 cards\":\n" +
        $"PLAYER_WEAK 2\n" +
        $"PLAYER_VULNERABLE 2\n" +
        $"神罰\n\n" +
        $"Now output for the player's request. No extra text.";

    protected override void OnUpgrade()
    {
        _hasTransformed = false;
        _cardNameSuffix = "";
        _effectsList = [];
        _isCardExhaust = true;
        _isCardEthereal = false;
    }
}
