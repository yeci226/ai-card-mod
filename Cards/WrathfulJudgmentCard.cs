using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class WrathfulJudgmentCard : CustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    private int _multiplier = 5;
    private int _minDamage = 10;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Multiplier", _multiplier), new DynamicVar("MinDamage", _minDamage)];

    public WrathfulJudgmentCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int damage = Math.Max(piety * _multiplier, _minDamage);

        if (piety > 0)
            await PowerCmd.Apply<PietyPower>(Owner.Creature, -piety, Owner.Creature, null);

        await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        _multiplier = 7;
        DynamicVars["Multiplier"].BaseValue = _multiplier;
    }
}
