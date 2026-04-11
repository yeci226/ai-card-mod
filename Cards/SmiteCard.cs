using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class SmiteCard : CustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    private int _damage = 7;
    private int _weak = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(_damage, ValueProp.Move), new DynamicVar("WeakAmount", _weak)];

    public SmiteCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(_damage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, _weak, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _damage = 10;
        _weak = 3;
        DynamicVars.Damage.BaseValue = _damage;
        DynamicVars["WeakAmount"].BaseValue = _weak;
    }
}
