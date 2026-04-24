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
/// 名稱：迴響之刃
/// 描述：造成{Damage:diff()}點傷害。每次[gold]啟示[/gold]箭矢觸發，此牌傷害永久+1。
/// </summary>
public class EchoBladeCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    public EchoBladeCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
            return;

        // Ensure the tracking power is active
        if (!Owner.Creature.HasPower<EchoBladePower>())
            await PowerCmd.Apply<EchoBladePower>(Owner.Creature, 0, Owner.Creature, this);

        var echoBlade = Owner.Creature.Powers!.OfType<EchoBladePower>().First();
        echoBlade.BonusPerTrigger = IsUpgraded ? 2 : 1;

        int totalDamage = (int)DynamicVars.Damage.BaseValue + echoBlade.BonusDamage;

        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
