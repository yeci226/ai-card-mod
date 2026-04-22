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
/// 名稱：連鎖啟示
/// 描述：造成{Damage:diff()}點傷害{Repeat:diff()}次。每一下獲得{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]。
/// </summary>
public class ChainRevelationCard : CustomCardModel
{
    private const string RevelationGainKey = RevelationGainVar.Key;
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(2, ValueProp.Move),
        new RepeatVar(3),
        new DynamicVar(RevelationGainKey, 1).WithTooltip(RevelationGainVar.LocKey)
    ];

    public ChainRevelationCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        for (int index = 0; index < DynamicVars["Repeat"].IntValue; index++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.IntValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, DynamicVars[RevelationGainKey].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Repeat"].UpgradeValueBy(1);
    }
}