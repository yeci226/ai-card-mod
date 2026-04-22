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
/// 名稱：天界之門
/// 描述：造成{Damage:diff()}點傷害{Repeat:diff()}次。獲得{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]{Repeat:diff()}次。
/// </summary>
public class CelestialGateCard : CustomCardModel
{
    private const string RevelationGainKey = RevelationGainVar.Key;
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new RepeatVar(1),
        new DynamicVar(RevelationGainKey, 3).WithTooltip(RevelationGainVar.LocKey)
    ];

    public CelestialGateCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        int repeatCount = Math.Max(0, ResolveEnergyXValue());
        if (repeatCount <= 0)
            return;

        for (int index = 0; index < repeatCount; index++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.IntValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, DynamicVars[RevelationGainKey].IntValue, Owner.Creature, this);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}