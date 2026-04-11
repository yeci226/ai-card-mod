using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class PiousDefendCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _block = 5;
    private int _bonusBlock = 3;
    private const int PietyThreshold = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(_block, ValueProp.Move), new DynamicVar("BonusBlock", _bonusBlock)];

    public PiousDefendCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int total = _block + (piety >= PietyThreshold ? _bonusBlock : 0);
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(total, ValueProp.Move), null);
    }

    protected override void OnUpgrade()
    {
        _block = 7;
        _bonusBlock = 4;
        DynamicVars.Block.BaseValue = _block;
        DynamicVars["BonusBlock"].BaseValue = _bonusBlock;
    }
}
