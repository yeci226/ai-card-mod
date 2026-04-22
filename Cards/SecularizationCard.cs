using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：世俗化
/// 描述：失去至多{AICARDMOD-SecularizationCap:diff()}點[gold]虔誠[/gold]，每失去一點便獲得{AICARDMOD-StrengthPerFaith:diff()}點[gold]力量[/gold]。
/// </summary>
public class SecularizationCard : CustomCardModel
{
    private const string ConvertCapKey = "AICARDMOD-SecularizationCap";
    private const string StrengthPerFaithKey = "AICARDMOD-StrengthPerFaith";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FaithPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(ConvertCapKey, 3),
        new DynamicVar(StrengthPerFaithKey, 1)
    ];

    public SecularizationCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int faith = (int)(Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault()?.Amount ?? 0m);
        int convert = Math.Min(faith, DynamicVars[ConvertCapKey].IntValue);
        if (convert <= 0)
            return;

        int strengthPerFaith = DynamicVars[StrengthPerFaithKey].IntValue;
        await PowerCmd.Apply<FaithPower>(Owner.Creature, -convert, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, convert * strengthPerFaith, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[ConvertCapKey].UpgradeValueBy(2);
    }
}