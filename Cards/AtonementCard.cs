using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class AtonementCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _block = 8;
    private const int PietyCost = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(_block, ValueProp.Move)];

    public AtonementCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int loss = Math.Min(PietyCost, piety);
        if (loss > 0)
            await PowerCmd.Apply<PietyPower>(Owner.Creature, -loss, Owner.Creature, null);
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(_block, ValueProp.Move), null);
    }

    protected override void OnUpgrade()
    {
        _block = 12;
        DynamicVars.Block.BaseValue = _block;
    }
}
