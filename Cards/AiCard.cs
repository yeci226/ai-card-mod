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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Reflection;
using System.Threading;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：神諭：靈感
/// 描述：根據前綴、中綴、後綴設定生成衍生卡牌。可於模組設定切換為AI模式。
/// </summary>
public class AiCard : PortraitCardModel
{
    private sealed record TemplatePart(string Name, string Effect, int BaseValue);

    private static readonly TemplatePart[] PrefixOptions =
    [
        new("守護", "BLOCK", 8),
        new("聖炎", "DAMAGE", 8),
        new("靈思", "DRAW", 1),
        new("恩典", "HEAL", 5),
        new("懲戒", "VULNERABLE", 1)
    ];

    private static readonly TemplatePart[] MiddleOptions =
    [
        new("祈言", "ENERGY", 1),
        new("審判", "WEAK", 1),
        new("庇護", "BLOCK", 6),
        new("裁決", "DAMAGE", 6),
        new("啟示", "DRAW", 1)
    ];

    private static readonly TemplatePart[] SuffixOptions =
    [
        new("之光", "DAMAGE", 5),
        new("之幕", "BLOCK", 5),
        new("之息", "HEAL", 4),
        new("之誓", "STRENGTH", 1),
        new("之痕", "FRAIL", 1)
    ];

    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;
    private static readonly string BaseCardTitle = new LocString("cards", "AICARDMOD-AI_CARD.title").GetFormattedText();
    private static int _generatedCardSerial = 0;

    public AiCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    private bool _hasTransformed = false;
    private List<(string effect, int value)> _effectsList = [];
    private string _cardNameSuffix = "";
    private bool _isCardExhaust = false;
    private bool _isCardEthereal = false;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        _isCardExhaust ? [CardKeyword.Exhaust] : [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_hasTransformed)
        {
            GD.Print($"[AICard] Executing stored effects: {string.Join(", ", _effectsList.Select(e => $"{e.effect} {e.value}"))}");
            await ApplyEffects(_effectsList, choiceContext);
            return;
        }

        if (!AiCardModConfig.UseAiMode)
        {
            await GenerateFromTemplate(choiceContext);
            return;
        }

        if (!IsAiConfigReady())
        {
            GD.PrintErr("[AICard] AI mode is enabled but API URL/model are not configured. Returning card to hand.");
            await CardPileCmd.AddGeneratedCardToCombat(
                this,
                PileType.Hand,
                addedByPlayer: true,
                CardPilePosition.Top);
            return;
        }

        // First play: ask player for request
        var playerRequest = await AiInputDialog.ShowAsync();
        if (string.IsNullOrWhiteSpace(playerRequest))
        {
            GD.Print("[AICard] Player cancelled, returning card to hand.");
            await CardPileCmd.AddGeneratedCardToCombat(
                this,
                PileType.Hand,
                addedByPlayer: true,
                CardPilePosition.Top);
            return;
        }

        int piety = (int)(Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault()?.Amount ?? 0m);
        string pietyTier = GetPietyTier(piety);
        GD.Print($"[AICard] Piety: {piety} ({pietyTier})");

        GD.Print($"[AICard] Player request: \"{playerRequest}\"");
        GD.Print("[AICard] Calling AI...");

        var aiResponse = await OllamaClient.AskAsync(BuildPrompt(playerRequest, piety, pietyTier));
        GD.Print($"[AICard] AI response:\n{aiResponse}");

        ParseAiResponse(aiResponse, out var effectsList, out var cardName, out var isExhaust, out var isEthereal, out var cost);

        GD.Print($"[AICard] Effects: {string.Join(", ", effectsList.Select(e => $"{e.Item1} {e.Item2}"))}");
        GD.Print($"[AICard] Name: {cardName}, Cost: {cost}, Exhaust: {isExhaust}, Ethereal: {isEthereal}");

        // Fallback if parsing still produced nothing useful.
        if (effectsList.Count == 0 && !isExhaust && !isEthereal)
        {
            GD.Print("[AICard] No effects parsed, using neutral fallback.");
            effectsList.Add(("BLOCK", 8));
            cardName = "庇護";
            cost = 1;
        }

        if (piety > 0)
        {
            GD.Print($"[AICard] Consuming {piety} faith.");
            await PowerCmd.Apply<FaithPower>(Owner.Creature, -piety, Owner.Creature, null);
        }

        // Transform this card in-place and return it to hand.
        _effectsList = new List<(string, int)>(effectsList);
        _hasTransformed = true;
        _cardNameSuffix = cardName;
        _isCardExhaust = false;
        _isCardEthereal = isEthereal;

        var description = GenerateDescription(_effectsList, _isCardEthereal);
        UpdateCardAppearance(description, _cardNameSuffix, cost);

        await CardPileCmd.AddGeneratedCardToCombat(
            this,
            PileType.Hand,
            addedByPlayer: true,
            CardPilePosition.Top);

        GD.Print($"[AICard] Card transformed in-place to '神諭 - {cardName}' and returned to hand.");
    }

    private async Task GenerateFromTemplate(PlayerChoiceContext choiceContext)
    {
        int availablePiety = (int)(Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault()?.Amount ?? 0m);
        int defaultPietySpend = Math.Clamp((int)Math.Round(AiCardModConfig.DefaultPietySpend), 0, Math.Max(0, availablePiety));

        var selection = await TemplateCardDialog.ShowAsync(
            PrefixOptions.Select(x => x.Name).ToArray(),
            MiddleOptions.Select(x => x.Name).ToArray(),
            SuffixOptions.Select(x => x.Name).ToArray(),
            (int)Math.Round(AiCardModConfig.PrefixIndex),
            (int)Math.Round(AiCardModConfig.MiddleIndex),
            (int)Math.Round(AiCardModConfig.SuffixIndex),
            defaultPietySpend,
            Math.Max(0, availablePiety),
            AiCardModConfig.GeneratedCardEthereal,
            BuildTemplatePreview);

        if (selection == null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(
                this,
                PileType.Hand,
                addedByPlayer: true,
                CardPilePosition.Top);
            GD.Print("[AICard] Template selection cancelled, card returned to hand.");
            return;
        }

        int pietySpend = Math.Clamp(selection.PietySpend, 0, Math.Max(0, availablePiety));
        var prefix = GetTemplatePart(PrefixOptions, selection.PrefixIndex);
        var middle = GetTemplatePart(MiddleOptions, selection.MiddleIndex);
        var suffix = GetTemplatePart(SuffixOptions, selection.SuffixIndex);

        double scale = Math.Clamp(AiCardModConfig.EffectScale, 0.5, 2.5);
        double pietyPoints = CalculatePietyPoints(pietySpend);
        var effects = new List<(string effect, int value)>
        {
            (prefix.Effect, ScaleEffectValue(prefix.Effect, prefix.BaseValue, scale, pietyPoints)),
            (middle.Effect, ScaleEffectValue(middle.Effect, middle.BaseValue, scale, pietyPoints)),
            (suffix.Effect, ScaleEffectValue(suffix.Effect, suffix.BaseValue, scale, pietyPoints))
        };

        MergeEffects(effects);

        _effectsList = effects;
        _hasTransformed = true;
        _cardNameSuffix = $"{prefix.Name}{middle.Name}{suffix.Name}";
        _isCardExhaust = false;
        _isCardEthereal = selection.Ethereal;

        AiCardModConfig.PrefixIndex = selection.PrefixIndex;
        AiCardModConfig.MiddleIndex = selection.MiddleIndex;
        AiCardModConfig.SuffixIndex = selection.SuffixIndex;
        AiCardModConfig.DefaultPietySpend = pietySpend;
        AiCardModConfig.GeneratedCardEthereal = selection.Ethereal;

        var description = GenerateDescription(_effectsList, _isCardEthereal);
        UpdateCardAppearance(description, _cardNameSuffix, 0);

        if (pietySpend > 0)
        {
            await PowerCmd.Apply<FaithPower>(Owner.Creature, -pietySpend, Owner.Creature, null);
        }

        await CardPileCmd.AddGeneratedCardToCombat(
            this,
            PileType.Hand,
            addedByPlayer: true,
            CardPilePosition.Top);

        GD.Print($"[AICard] Template card '{_cardNameSuffix}' transformed in-place and returned to hand. Piety spent: {pietySpend}");
    }

    private static bool IsAiConfigReady() =>
        !string.IsNullOrWhiteSpace(AiCardModConfig.AiApiUrl) &&
        !string.IsNullOrWhiteSpace(AiCardModConfig.AiModel);

    private static TemplatePart GetTemplatePart(TemplatePart[] options, double rawIndex)
    {
        int index = Math.Clamp((int)Math.Round(rawIndex), 0, options.Length - 1);
        return options[index];
    }

    private static (string title, string description, string affixText, string metaText) BuildTemplatePreview(
        int prefixIndex,
        int middleIndex,
        int suffixIndex,
        int pietySpend,
        bool isEthereal)
    {
        var prefix = GetTemplatePart(PrefixOptions, prefixIndex);
        var middle = GetTemplatePart(MiddleOptions, middleIndex);
        var suffix = GetTemplatePart(SuffixOptions, suffixIndex);

        double scale = Math.Clamp(AiCardModConfig.EffectScale, 0.5, 2.5);
        double pietyPoints = CalculatePietyPoints(Math.Max(0, pietySpend));

        var effects = new List<(string effect, int value)>
        {
            (prefix.Effect, ScaleEffectValue(prefix.Effect, prefix.BaseValue, scale, pietyPoints)),
            (middle.Effect, ScaleEffectValue(middle.Effect, middle.BaseValue, scale, pietyPoints)),
            (suffix.Effect, ScaleEffectValue(suffix.Effect, suffix.BaseValue, scale, pietyPoints))
        };

        MergeEffects(effects);

        string suffixName = $"{prefix.Name}{middle.Name}{suffix.Name}";
        string title = $"{BaseCardTitle} - {suffixName}";
        string description = GenerateDescription(effects, isEthereal);
        string affixText = $"前綴：{prefix.Name}    中綴：{middle.Name}    後綴：{suffix.Name}";
        string keywords = isEthereal ? "虛無" : "無";
        string metaText = $"費用：0    關鍵字：{keywords}    虔誠投入：{Math.Max(0, pietySpend)}    虔誠轉點：{pietyPoints:0.##}";
        return (title, description, affixText, metaText);
    }

    private static double CalculatePietyPoints(int pietySpend)
    {
        if (pietySpend <= 0) return 0;
        if (pietySpend <= 4) return pietySpend * 0.5;
        if (pietySpend <= 8) return 2 + (pietySpend - 4) * 0.35;
        return 3.4 + (pietySpend - 8) * 0.2;
    }

    private static int ScaleEffectValue(string effect, int baseValue, double scale, double pietyPoints)
    {
        int scaled = (int)Math.Round(baseValue * scale, MidpointRounding.AwayFromZero);
        int withBonus = scaled + (int)Math.Round(pietyPoints, MidpointRounding.AwayFromZero);

        return effect.ToUpperInvariant() switch
        {
            "DRAW" => Math.Clamp(withBonus, 1, 5),
            "ENERGY" => Math.Clamp(withBonus, 1, 3),
            "VULNERABLE" or "WEAK" or "FRAIL" => Math.Clamp(withBonus, 1, 6),
            "STRENGTH" or "DEXTERITY" or "FOCUS" => Math.Clamp(withBonus, 1, 5),
            _ => Math.Clamp(withBonus, 1, 99)
        };
    }

    private static void MergeEffects(List<(string effect, int value)> effects)
    {
        var merged = effects
            .GroupBy(x => x.effect, StringComparer.OrdinalIgnoreCase)
            .Select(g => (effect: g.Key.ToUpperInvariant(), value: g.Sum(x => x.value)))
            .ToList();

        effects.Clear();
        effects.AddRange(merged);
    }

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

    private static string GetPietyTier(int piety) => piety switch
    {
        < 0 => "ANGERED",
        0 => "ANGERED",
        <= 2 => "DISPLEASED",
        <= 5 => "NEUTRAL",
        <= 9 => "BLESSED",
        _ => "DIVINE"
    };

    // TODO: Unicode comment repaired.

    private static void ParseAiResponse(
        string response,
        out List<(string, int)> effects,
        out string cardName,
        out bool isExhaust,
        out bool isEthereal,
        out int cost)
    {
        effects = [];
        cardName = "變�?";
        isExhaust = false;
        isEthereal = false;
        cost = 1;

        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim().TrimStart('-', '*', ' '))
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

    // TODO: Unicode comment repaired.

    private static string GenerateDescription(List<(string effect, int value)> effects, bool isEthereal)
    {
        var lines = new List<string>();
        var keywordLines = new List<string>();

        if (isEthereal)
            keywordLines.Add("[gold]虛無[/gold]");

        foreach (var (effectName, value) in effects)
        {
            string line = effectName.ToUpperInvariant() switch
            {
                "DAMAGE" => $"造成 {value} 點傷害。",
                "BLOCK" => $"獲得 {value} 點格擋。",
                "DRAW" => $"抽取 {value} 張卡牌。",
                "HEAL" => $"恢復 {value} 點生命值。",
                "ENERGY" => $"獲得 {value} 點能量。",
                "STRENGTH" => $"獲得 {value} 層力量。",
                "DEXTERITY" => $"獲得 {value} 層敏捷。",
                "FOCUS" => $"獲得 {value} 層專注。",
                "GOLD" => $"獲得 {value} 枚金幣。",
                "VULNERABLE" => $"對敵人施加 {value} 層易傷。",
                "WEAK" => $"對敵人施加 {value} 層虛弱。",
                "FRAIL" => $"對敵人施加 {value} 層脆弱。",
                "PLAYER_WEAK" => $"[red]你獲得 {value} 層虛弱。[/red]",
                "PLAYER_VULNERABLE" => $"[red]你獲得 {value} 層易傷。[/red]",
                _ => $"{effectName} {value}"
            };
            lines.Add(line);
        }

        return string.Join("\n", keywordLines.Concat(lines));
    }

    // TODO: Unicode comment repaired.

    private void UpdateCardAppearance(string description, string nameSuffix, int cost)
    {
        try
        {
            var cardKey = Id.Entry;
            var fullTitle = $"{BaseCardTitle} - {nameSuffix}";

            var table = LocManager.Instance.GetTable("cards");
            table.MergeWith(new Dictionary<string, string>
            {
                [$"{cardKey}.title"] = fullTitle,
                [$"{cardKey}.description"] = description,
                [$"{cardKey}.upgraded_description"] = description
            });

            EnergyCost.SetThisCombat(cost);
            GD.Print($"[AICard] Appearance updated -> {fullTitle} (cost {cost})");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AICard] UpdateCardAppearance failed: {ex.Message}");
        }
    }

    // TODO: Unicode comment repaired.

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
                await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(value, ValueProp.Move), null);
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

            // TODO: Unicode comment repaired.
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

    private static string BuildPrompt(string playerRequest, int piety, string pietyTier) =>
        $"You are designing a card effect for Slay the Spire 2.\n\n" +
        $"Player request: \"{playerRequest}\"\n\n" +
        $"RULES:\n" +
        $"1. Grant EXACTLY what the player asked for.\n" +
        $"2. Output ONLY effects (one per line), then a short Chinese card name (2-4 characters). No explanation.\n\n" +
        $"Available effects:\n" +
        $"  BLOCK X            - gain X block\n" +
        $"  DAMAGE X           - deal X damage to random enemy\n" +
        $"  DRAW X             - draw X cards\n" +
        $"  HEAL X             - heal X HP\n" +
        $"  ENERGY X           - gain X energy this turn\n" +
        $"  STRENGTH X         - gain X Strength\n" +
        $"  DEXTERITY X        - gain X Dexterity\n" +
        $"  GOLD X             - gain X gold\n" +
        $"  VULNERABLE X       - enemy gets X Vulnerable\n" +
        $"  WEAK X             - enemy gets X Weak\n" +
        $"  FRAIL X            - enemy gets X Frail\n" +
        $"  PLAYER_WEAK X      - player gets X Weak (negative)\n" +
        $"  PLAYER_VULNERABLE X- player gets X Vulnerable (negative)\n\n" +
        $"Example - player asked \"80 block\":\n" +
        $"BLOCK 80\n" +
        $"堡壘\n\n" +
        $"Now output for the player's request. No extra text.";

    protected override void OnUpgrade()
    {
        EnergyCost.SetCustomBaseCost(0);
        _hasTransformed = false;
        _cardNameSuffix = "";
        _effectsList = [];
        _isCardExhaust = false;
        _isCardEthereal = false;
    }

    private static void SetGeneratedCardId(AiCard card)
    {
        int serial = Interlocked.Increment(ref _generatedCardSerial);
        var generatedId = new ModelId(card.Id.Category, $"AICARDMOD-AI_CARD_GEN_{serial:D4}");
        var idField = typeof(AbstractModel).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate AbstractModel.Id backing field.");
        idField.SetValue(card, generatedId);
    }
}
