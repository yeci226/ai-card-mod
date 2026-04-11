using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class HolyStrikeCard : CustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    private int _damage = 6;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(_damage, ValueProp.Move)];

    public HolyStrikeCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(_damage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        _damage = 8;
        DynamicVars.Damage.BaseValue = _damage;
    }
}
