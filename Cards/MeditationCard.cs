using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class MeditationCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _block = 6;
    private int _pietyGain = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(_block, ValueProp.Move), new DynamicVar("PietyGain", _pietyGain)];

    public MeditationCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(_block, ValueProp.Move), null);
        await PowerCmd.Apply<PietyPower>(Owner.Creature, _pietyGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _block = 9;
        _pietyGain = 3;
        DynamicVars.Block.BaseValue = _block;
        DynamicVars["PietyGain"].BaseValue = _pietyGain;
    }
}
