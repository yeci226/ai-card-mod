using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：苦行
/// 描述：失去{AICARDMOD-HpLoss:diff()}點生命值。獲得{AICARDMOD-FaithGain:diff()}點[gold]虔誠[/gold]。
/// </summary>
public class PenanceCard : CustomCardModel
{
    private const string HpLossKey = HpLossVar.Key;
    private const string FaithGainKey = FaithGainVar.Key;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(HpLossKey, 3),
        new DynamicVar(FaithGainKey, 2).WithTooltip(FaithGainVar.LocKey)
    ];

    public PenanceCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars[HpLossKey].IntValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            this);

        await FaithManager.ApplyFaith(Owner.Creature, DynamicVars[FaithGainKey].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[FaithGainKey].UpgradeValueBy(1);
    }
}