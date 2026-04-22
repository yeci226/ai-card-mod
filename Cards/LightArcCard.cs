using BaseLib.Abstracts;
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
/// 名稱：光弧
/// 描述：造成{Damage:diff()}點傷害。如果你擁有大於等於{AICARDMOD-LightArcFaithSpend:diff()}點[gold]虔誠[/gold]，則消耗{AICARDMOD-LightArcFaithSpend:diff()}點[gold]虔誠[/gold]，造成{BonusDamage:diff()}點額外傷害。
/// </summary>
public class LightArcCard : CustomCardModel
{
    private const string FaithSpendKey = "AICARDMOD-LightArcFaithSpend";
    private const string BonusDamageKey = "BonusDamage";
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar(FaithSpendKey, 1),
        new DynamicVar(BonusDamageKey, 5)
    ];

    public LightArcCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var totalDamage = DynamicVars.Damage.BaseValue;
        int faithSpend = Math.Max(1, DynamicVars[FaithSpendKey].IntValue);

        var faith = Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault();
        if (faith != null && faith.Amount >= faithSpend)
        {
            await PowerCmd.Apply<FaithPower>(Owner.Creature, -faithSpend, Owner.Creature, this);
            totalDamage += DynamicVars[BonusDamageKey].IntValue;
        }

        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars[BonusDamageKey].UpgradeValueBy(2);
    }
}
