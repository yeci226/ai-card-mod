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
/// 名稱：點燃靈感
/// 描述：造成{Damage:diff()}點傷害。每有{AICARDMOD-RevelationStep:diff()}層[gold]啟示[/gold]就額外造成{BonusPerThree:diff()}點傷害。
/// </summary>
public class IgniteInspirationCard : CustomCardModel
{
    private const string RevelationStepKey = "AICARDMOD-RevelationStep";
    private const string BonusPerThreeKey = "BonusPerThree";
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RevelationPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Move),
        new DynamicVar(RevelationStepKey, 3),
        new DynamicVar(BonusPerThreeKey, 2)
    ];

    public IgniteInspirationCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        int revelation = (int)(Owner.Creature.GetPower<RevelationPower>()?.Amount ?? 0m);
        int revelationStep = Math.Max(1, DynamicVars[RevelationStepKey].IntValue);
        var totalDamage = DynamicVars.Damage.BaseValue + (revelation / revelationStep) * DynamicVars[BonusPerThreeKey].IntValue;

        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() { DynamicVars[BonusPerThreeKey].UpgradeValueBy(1); }
}
