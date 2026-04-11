using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class PropheticShieldCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _multiplier = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Multiplier", _multiplier)];

    public PropheticShieldCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int total = piety * _multiplier;
        if (total > 0)
            await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(total, ValueProp.Move), null);
    }

    protected override void OnUpgrade()
    {
        _multiplier = 4;
        DynamicVars["Multiplier"].BaseValue = _multiplier;
    }
}
