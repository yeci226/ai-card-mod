using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class HolyCleaveCard : CustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _damage = 6;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(_damage, ValueProp.Move)];

    public HolyCleaveCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = Owner.Creature.CombatState?.Enemies.Where(e => e.IsAlive).ToList() ?? [];
        foreach (var enemy in enemies)
            await DamageCmd.Attack(_damage).FromCard(this).Targeting(enemy).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        _damage = 9;
        DynamicVars.Damage.BaseValue = _damage;
    }
}
