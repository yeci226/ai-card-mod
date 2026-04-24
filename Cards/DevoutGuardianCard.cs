using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：虔誠衛士
/// 描述：獲得{Block:diff()}點[gold]格擋[/gold]。若有[gold]虔誠[/gold]，則額外獲得{AICARDMOD-FaithBonusBlock:diff()}點[gold]格擋[/gold]。
/// </summary>
public class DevoutGuardianCard : CustomCardModel
{
    private const string FaithBonusBlockKey = "AICARDMOD-FaithBonusBlock";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FaithPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move),
        new DynamicVar(FaithBonusBlockKey, 5)
    ];

    public DevoutGuardianCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int totalBlock = DynamicVars.Block.IntValue;
        int faith = (int)(Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault()?.Amount ?? 0m);
        if (faith > 0)
            totalBlock += DynamicVars[FaithBonusBlockKey].IntValue;
        await CreatureCmd.GainBlock(Owner.Creature, totalBlock, ValueProp.Move, null);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[FaithBonusBlockKey].UpgradeValueBy(1);
    }
}
