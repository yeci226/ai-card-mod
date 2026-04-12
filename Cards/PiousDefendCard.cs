using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class PiousDefendCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _baseBlock = 7;
    private int _upgradedBlock = 10;
    private int _basePiety = 1;
    private int _upgradedPiety = 2;

    public PiousDefendCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int block = IsUpgraded ? _upgradedBlock : _baseBlock;
        int piety = IsUpgraded ? _upgradedPiety : _basePiety;

        // Gain block
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(block, 0), null);

        // Gain piety
        await PowerCmd.Apply<PietyPower>(Owner.Creature, piety, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Upgrade mechanics handled via IsUpgraded check in OnPlay
    }
}



