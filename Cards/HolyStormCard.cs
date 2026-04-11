using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class HolyStormCard : CustomCardModel
{
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _damage = 12;
    private int _pietyGain = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(_damage, ValueProp.Move), new DynamicVar("PietyGain", _pietyGain)];

    public HolyStormCard() : base(3, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = Owner.Creature.CombatState?.Enemies.Where(e => e.IsAlive).ToList() ?? [];
        foreach (var enemy in enemies)
            await DamageCmd.Attack(_damage).FromCard(this).Targeting(enemy).Execute(choiceContext);
        await PowerCmd.Apply<PietyPower>(Owner.Creature, _pietyGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _damage = 16;
        _pietyGain = 5;
        DynamicVars.Damage.BaseValue = _damage;
        DynamicVars["PietyGain"].BaseValue = _pietyGain;
    }
}
