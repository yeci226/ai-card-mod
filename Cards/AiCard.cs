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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class AiCard : PortraitCardModel
{
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

        int piety = (int)(Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault()?.Amount ?? 0m);

        if (piety >= 3 && !_hasTransformed)
        {
            await MegaCrit.Sts2.Core.Commands.PowerCmd.Apply<FaithPower>(Owner.Creature, -3, Owner.Creature, null);
            GD.Print("[AICard] Consuming 3 faith.");
            await TransformNonAiMode(choiceContext, piety);
        }
        else if (_hasTransformed)
        {
            // Placeholder for flow logic
        }
        else
        {
            if (piety > 0)
            {
                GD.Print($"[AICard] Consuming {piety} faith.");
                await PowerCmd.Apply<FaithPower>(Owner.Creature, -piety, Owner.Creature, null);
            }

            bool isAiMode = !string.IsNullOrWhiteSpace(AiCardModConfig.AiApiUrl);

            if (isAiMode && IsAiConfigReady())
            {
                await TransformAiMode(choiceContext, piety);
            }
            else
            {
                await TransformNonAiMode(choiceContext, piety);
            }
        }
    }

    private static bool IsAiConfigReady() =>
        !string.IsNullOrWhiteSpace(AiCardModConfig.AiApiUrl) &&
        !string.IsNullOrWhiteSpace(AiCardModConfig.AiModel);

    // Non AI mode
    private async Task TransformNonAiMode(PlayerChoiceContext choiceContext, int piety)
    {
        _effectsList.Clear();

        if (piety <= 2)
        {
            _cardNameSuffix = "微小啟發";
            _effectsList.Add(("BLOCK", 8));
            _effectsList.Add(("DRAW", 1));
        }
        else if (piety <= 5)
        {
            _cardNameSuffix = "盲目試煉";
            _effectsList.Add(("MISSTEP_ALL", 2));
            _effectsList.Add(("REVELATION", 4));
        }
        else if (piety <= 8)
        {
            _cardNameSuffix = "神聖洗禮";
            _effectsList.Add(("BLOCK", 15));
            _effectsList.Add(("HEAL", 5));
            _effectsList.Add(("HOLY_MIGHT", 1));
        }
        else if (piety <= 11)
        {
            _cardNameSuffix = "狂熱異端";
            _effectsList.Add(("DAMAGE_ALL", 10));
            _effectsList.Add(("STRENGTH", 3));
        }
        else
        {
            _cardNameSuffix = "末日神諭";
            _effectsList.Add(("ENERGY", 2));
            _effectsList.Add(("DRAW", 3));
            _effectsList.Add(("REVELATION", 12));
        }

        await FinalizeTransformation(choiceContext);
    }

    // AI mode
    private async Task TransformAiMode(PlayerChoiceContext choiceContext, int piety)
    {
        var playerRequest = await AiInputDialog.ShowAsync();
        if (string.IsNullOrWhiteSpace(playerRequest))
        {
            await TransformNonAiMode(choiceContext, piety);
            return;
        }

        var aiResponse = await OllamaClient.AskAsync(BuildPrompt(playerRequest));
        ParseAiResponse(aiResponse, out var selectedEffects, out var cardName);

        if (selectedEffects.Count == 0)
        {
            selectedEffects.Add("BLOCK");
            cardName = "恩典";
        }

        _cardNameSuffix = cardName;
        _effectsList.Clear();

        // Budget calculation
        int totalBudget = 10 + (piety * 4);
        int budgetPerEffect = totalBudget / selectedEffects.Count;

        foreach (var eff in selectedEffects)
        {
            int value = CalculateValueFromBudget(eff, budgetPerEffect);
            _effectsList.Add((eff, value));
        }

        await FinalizeTransformation(choiceContext);
    }

    private static int CalculateValueFromBudget(string effect, int budget)
    {
        return effect switch
        {
            "DRAW" => Math.Max(1, budget / 8),
            "ENERGY" => Math.Max(1, budget / 12),
            "HEAL" => Math.Max(1, budget / 3),
            "STRENGTH" or "DEXTERITY" or "FOCUS" => Math.Max(1, budget / 5),
            "VULNERABLE" or "WEAK" or "FRAIL" => Math.Max(1, budget / 4),
            "MISSTEP_ALL" => Math.Max(1, budget / 6),
            "REVELATION" => Math.Max(1, budget / 2),
            "HOLY_MIGHT" => Math.Max(1, budget / 6),
            "DAMAGE_ALL" => Math.Max(1, budget / 2),
            _ => Math.Max(1, budget) // DAMAGE, BLOCK
        };
    }

    private async Task FinalizeTransformation(PlayerChoiceContext choiceContext)
    {
        // 建立真正的新卡牌來進行變化，使用 CombatState.CreateCard 以確保卡牌擁有 CombatState
        var newCard = this.CombatState.CreateCard<AiCard>(this.Owner);
        SetGeneratedCardId(newCard);

        var titleField = typeof(MegaCrit.Sts2.Core.Models.CardModel).GetField("_titleLocString", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (titleField != null)
        {
            titleField.SetValue(newCard, new MegaCrit.Sts2.Core.Localization.LocString("cards", $"{newCard.Id.Entry}.title"));
        }

        newCard._hasTransformed = true;
        newCard._effectsList = new List<(string effect, int value)>(_effectsList);
        newCard._cardNameSuffix = _cardNameSuffix;
        newCard._isCardExhaust = _isCardExhaust;
        newCard._isCardEthereal = _isCardEthereal;

        int energyCost = 1;
        var description = GenerateDescription(newCard, newCard._effectsList, newCard._isCardEthereal, newCard._isCardExhaust);

        newCard.UpdateCardAppearance(description, newCard._cardNameSuffix, energyCost);

        // 為了避免打出區留下幽靈卡牌，我們在變化前先找到並隱藏原本的 NCard
        var ghostNCard = MegaCrit.Sts2.Core.Nodes.Cards.NCard.FindOnTable(this);
        if (ghostNCard != null)
        {
            ghostNCard.Visible = false;
        }

        // 觸發真正的變化，並展示橫向的彈出式特效 (此時會在畫面中央展示)
        await MegaCrit.Sts2.Core.Commands.CardCmd.Transform(this, newCard, MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle.HorizontalLayout);

        // 稍微等待較長的時間 (2.2秒)，讓變化動畫完整播放完畢，避免在變化途中就飛回手牌
        await MegaCrit.Sts2.Core.Commands.Cmd.CustomScaledWait(2.2f, 2.2f);

        // 變化完成後，將全新的卡牌飛回手牌中
        await MegaCrit.Sts2.Core.Commands.CardPileCmd.Add(newCard, MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand);

        GD.Print($"[AICard] Instantly transformed and returned to hand: '神諭 - {_cardNameSuffix}'");
    }

    private static void ParseAiResponse(
        string response,
        out List<string> effects,
        out string cardName)
    {
        effects = [];
        cardName = "變化";

        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim().TrimStart('-', '*', ' '))
            .Where(l => l.Length > 0)
            .ToList();

        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (!KnownEffects.Contains(lines[i].ToUpperInvariant()))
            {
                cardName = lines[i];
                lines.RemoveAt(i);
                break;
            }
        }

        foreach (var line in lines)
        {
            var candidate = line.ToUpperInvariant();
            if (KnownEffects.Contains(candidate))
            {
                effects.Add(candidate);
            }
        }
    }

    private static readonly HashSet<string> KnownEffects = new(StringComparer.OrdinalIgnoreCase)
    {
        "DAMAGE", "BLOCK", "DRAW", "HEAL", "ENERGY", "STRENGTH", "DEXTERITY", "FOCUS", "GOLD",
        "VULNERABLE", "WEAK", "FRAIL", "MISSTEP_ALL", "REVELATION", "HOLY_MIGHT", "DAMAGE_ALL"
    };

    private static string BuildPrompt(string playerRequest) =>
        $"You are an AI generating card effects for Slay the Spire 2.\n\n" +
        $"Player request: \"{playerRequest}\"\n\n" +
        $"RULES:\n" +
        $"1. Output ONLY 1 to 3 effect keywords (one per line) from the list below, and finally a short Chinese card name (2-4 characters).\n" +
        $"2. DO NOT output any numbers or explanations. The game will automatically calculate the balance.\n\n" +
        $"Available effects:\n" +
        $"  BLOCK\n" +
        $"  DAMAGE\n" +
        $"  DAMAGE_ALL\n" +
        $"  DRAW\n" +
        $"  HEAL\n" +
        $"  ENERGY\n" +
        $"  STRENGTH\n" +
        $"  DEXTERITY\n" +
        $"  VULNERABLE\n" +
        $"  WEAK\n" +
        $"  FRAIL\n" +
        $"  MISSTEP_ALL\n" +
        $"  REVELATION\n" +
        $"  HOLY_MIGHT\n\n" +
        $"Example output:\n" +
        $"BLOCK\n" +
        $"DRAW\n" +
        $"庇護\n\n" +
        $"Now output for the player's request. No extra text.";

    private static void AddOrUpdateDynamicVar(MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVarSet varSet, MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar dynamicVar)
    {
        var dictField = typeof(MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVarSet).GetField("_vars", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (dictField?.GetValue(varSet) is System.Collections.Generic.Dictionary<string, MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar> dict)
        {
            dict[dynamicVar.Name] = dynamicVar;
        }
    }

    private static readonly System.Collections.Generic.Dictionary<string, string> FallbackStrings = new()
    {
        { "AICARDMOD-AI_CARD.effect_ENERGY", "獲得{Energy:diff()}點[gold]能量[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_STRENGTH", "獲得{Strength:diff()}點[gold]力量[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_DAMAGE_ALL", "對所有敵人造成{Damage:diff()}點傷害。" },
        { "AICARDMOD-AI_CARD.effect_HOLY_MIGHT", "獲得{HolyMight:diff()}點[gold]神聖之力[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_REVELATION", "獲得{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_EXHAUST", "[gold]消耗。[/gold]" },
        { "AICARDMOD-AI_CARD.effect_ETHEREAL", "[gold]虛無。[/gold]" },
        { "AICARDMOD-AI_CARD.effect_DEXTERITY", "獲得{Dexterity:diff()}點[gold]敏捷[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_FOCUS", "獲得{Focus:diff()}點[gold]專注[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_GOLD", "獲得{Gold:diff()}枚金幣。" },
        { "AICARDMOD-AI_CARD.effect_VULNERABLE", "給予敵人{Vulnerable:diff()}層[gold]易傷[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_WEAK", "給予敵人{Weak:diff()}層[gold]虛弱[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_FRAIL", "給予敵人{Frail:diff()}層[gold]脆弱[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_MISSTEP_ALL", "給予所有敵人{AICARDMOD-MisstepGain:diff()}層[gold]失誤[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_DAMAGE", "造成{Damage:diff()}點傷害。" },
        { "AICARDMOD-AI_CARD.effect_HEAL", "回復{Heal:diff()}點生命。" },
        { "AICARDMOD-AI_CARD.effect_BLOCK", "獲得{Block:diff()}點[gold]格擋[/gold]。" },
        { "AICARDMOD-AI_CARD.effect_DRAW", "抽{Cards:diff()}張牌。" }
    };

    private static string GenerateDescription(AiCard newCard, List<(string effect, int value)> effects, bool isEthereal, bool isExhaust)
    {
        var lines = new List<string>();

        if (isEthereal) lines.Add(FallbackStrings["AICARDMOD-AI_CARD.effect_ETHEREAL"]);
        if (isExhaust) lines.Add(FallbackStrings["AICARDMOD-AI_CARD.effect_EXHAUST"]);

        foreach (var (effectName, value) in effects)
        {
            string locKey = $"AICARDMOD-AI_CARD.effect_{effectName.ToUpperInvariant()}";
            string line = new MegaCrit.Sts2.Core.Localization.LocString("cards", locKey).GetFormattedText();
            
            // If the localization manager fails to find the key, it returns a string containing the key itself (or empty).
            if (string.IsNullOrWhiteSpace(line) || line.Contains(locKey) || line.Contains("effect_"))
            {
                line = FallbackStrings.TryGetValue(locKey, out string fallback) ? fallback : $"Missing:{locKey}";
            }

            // Populate DynamicVars so the engine can format {VarName:diff()}
            switch (effectName.ToUpperInvariant())
            {
                case "DAMAGE":
                case "DAMAGE_ALL":
                    newCard.DynamicVars.Damage.BaseValue = value;
                    break;
                case "BLOCK":
                    newCard.DynamicVars.Block.BaseValue = value;
                    break;
                case "DRAW":
                    newCard.DynamicVars.Cards.BaseValue = value;
                    break;
                case "HEAL":
                    newCard.DynamicVars.Heal.BaseValue = value;
                    break;
                case "ENERGY":
                    newCard.DynamicVars.Energy.BaseValue = value;
                    break;
                case "MISSTEP_ALL":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new AICardMod.Scripts.MisstepGainVar(value));
                    break;
                case "REVELATION":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new AICardMod.Scripts.RevelationGainVar(value));
                    break;
                case "HOLY_MIGHT":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("HolyMight", value));
                    break;
                case "STRENGTH":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Strength", value));
                    break;
                case "DEXTERITY":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Dexterity", value));
                    break;
                case "FOCUS":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Focus", value));
                    break;
                case "GOLD":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Gold", value));
                    break;
                case "VULNERABLE":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Vulnerable", value));
                    break;
                case "WEAK":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Weak", value));
                    break;
                case "FRAIL":
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar("Frail", value));
                    break;
                default:
                    AddOrUpdateDynamicVar(newCard.DynamicVars, new MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar(effectName, value));
                    break;
            }

            lines.Add(line);
        }

        return string.Join("\n", lines);
    }

    private void UpdateCardAppearance(string description, string nameSuffix, int cost)
    {
        try
        {
            var cardKey = Id.Entry;
            var fullTitle = $"{BaseCardTitle} - {nameSuffix}";

            var table = MegaCrit.Sts2.Core.Localization.LocManager.Instance.GetTable("cards");
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
            case "DAMAGE_ALL":
                var enemies = Owner.Creature.CombatState?.HittableEnemies.Where(e => e.IsAlive).ToList() ?? [];
                foreach (var enemy in enemies)
                {
                    await DamageCmd.Attack(value).FromCard(this).Targeting(enemy).Execute(choiceContext);
                }
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
            case "MISSTEP_ALL":
                var misstepEnemies = Owner.Creature.CombatState?.HittableEnemies.Where(e => e.IsAlive).ToList() ?? [];
                foreach (var enemy in misstepEnemies)
                {
                    await PowerCmd.Apply<MisstepPower>(enemy, value, Owner.Creature, this);
                }
                break;
            case "REVELATION":
                await PowerCmd.Apply<RevelationPower>(Owner.Creature, value, Owner.Creature, this);
                break;
            case "HOLY_MIGHT":
                await PowerCmd.Apply<HolyMightPower>(Owner.Creature, value, Owner.Creature, this);
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
