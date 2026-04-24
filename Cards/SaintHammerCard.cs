using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：聖徒之錘
/// 描述：造成{Damage:diff()}點傷害。若[gold]啟示[/gold]大於{AICARDMOD-RevelationThreshold:diff()}點，獲得{AICARDMOD-EnergyGain:diff()}點能量。
/// </summary>
public class SaintHammerCard : CustomCardModel
{
    private const string RevelationThresholdKey = "AICARDMOD-RevelationThreshold";
    private const string EnergyGainKey = "AICARDMOD-EnergyGain";
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RevelationPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new DynamicVar(RevelationThresholdKey, 10),
        new DynamicVar(EnergyGainKey, 1)
    ];

    public SaintHammerCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        int revelation = (int)(Owner.Creature.GetPower<RevelationPower>()?.Amount ?? 0m);

        await CommonActions.CardAttack(this, cardPlay, 1).Execute(choiceContext);
        if (revelation > DynamicVars[RevelationThresholdKey].IntValue)
            await PlayerCmd.GainEnergy(DynamicVars[EnergyGainKey].IntValue, Owner);
    }

    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(3); }
}
