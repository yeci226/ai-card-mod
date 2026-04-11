using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class SacredBarrageCard : CustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    private int _damagePerHit = 4;
    private const int MaxHits = 5;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(_damagePerHit, ValueProp.Move), new DynamicVar("MaxHits", MaxHits)];

    public SacredBarrageCard() : base(2, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int hits = Math.Clamp(piety + 1, 1, MaxHits);
        for (int i = 0; i < hits; i++)
            await DamageCmd.Attack(_damagePerHit).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        _damagePerHit = 6;
        DynamicVars.Damage.BaseValue = _damagePerHit;
    }
}
