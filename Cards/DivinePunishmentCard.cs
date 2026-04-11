using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class DivinePunishmentCard : CustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    private int _damage = 12;
    private int _bonusDamage = 8;
    private const int PietyCost = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(_damage, ValueProp.Move), new DamageVar("BonusDamage", _bonusDamage, ValueProp.Move)];

    public DivinePunishmentCard() : base(2, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int total = _damage;
        if (piety >= PietyCost)
        {
            total += _bonusDamage;
            await PowerCmd.Apply<PietyPower>(Owner.Creature, -PietyCost, Owner.Creature, null);
        }
        await DamageCmd.Attack(total).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        _damage = 16;
        _bonusDamage = 12;
        DynamicVars.Damage.BaseValue = _damage;
        DynamicVars["BonusDamage"].BaseValue = _bonusDamage;
    }
}
