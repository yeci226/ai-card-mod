using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：療癒之雨
/// 描述：所有友方回復{Heal:diff()}點生命。
/// </summary>
public class HealingRainCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Heal", 3)];

    public HealingRainCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heal = DynamicVars["Heal"].IntValue;
        foreach (var ally in Owner.Creature.CombatState?.GetCreaturesOnSide(Owner.Creature.Side).Where(creature => creature.IsAlive) ?? [])
            await CreatureCmd.Heal(ally, heal);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(1);
    }
}