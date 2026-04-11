using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class InnerPeaceCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _block = 5;
    private int _pietyThreshold = 5;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(_block, ValueProp.Move), new DynamicVar("PietyThreshold", _pietyThreshold)];

    public InnerPeaceCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(_block, ValueProp.Move), null);
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        if (piety >= _pietyThreshold)
            await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    protected override void OnUpgrade()
    {
        _block = 8;
        _pietyThreshold = 3;
        DynamicVars.Block.BaseValue = _block;
        DynamicVars["PietyThreshold"].BaseValue = _pietyThreshold;
    }
}
