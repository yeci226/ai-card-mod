using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class DivineStrikeCard : CustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    private const int Hits = 3;
    private int _damagePerHit = 5;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(_damagePerHit, ValueProp.Move), new DynamicVar("Hits", Hits)];

    public DivineStrikeCard() : base(2, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        for (int i = 0; i < Hits; i++)
        {
            await DamageCmd.Attack(_damagePerHit).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
            await PowerCmd.Apply<PietyPower>(Owner.Creature, 1, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        _damagePerHit = 7;
        DynamicVars.Damage.BaseValue = _damagePerHit;
    }
}
