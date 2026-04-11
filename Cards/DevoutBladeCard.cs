using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class DevoutBladeCard : CustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    private int _damage = 6;
    private int _bonusDamage = 4;
    private const int PietyThreshold = 5;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(_damage, ValueProp.Move), new DamageVar("BonusDamage", _bonusDamage, ValueProp.Move)];

    public DevoutBladeCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int total = _damage + (piety >= PietyThreshold ? _bonusDamage : 0);
        await DamageCmd.Attack(total).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<PietyPower>(Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _damage = 9;
        _bonusDamage = 6;
        DynamicVars.Damage.BaseValue = _damage;
        DynamicVars["BonusDamage"].BaseValue = _bonusDamage;
    }
}
