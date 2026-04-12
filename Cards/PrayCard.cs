using BaseLib.Abstracts;

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class PrayCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _basePiety = 3;
    private int _upgradedPiety = 5;
    private int _drawCount = 1;

    public PrayCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int piety = _basePiety;
        if (IsUpgraded) piety = _upgradedPiety;

        // Gain Piety.
        await PowerCmd.Apply<PietyPower>(Owner.Creature, piety, Owner.Creature, this);
        
        // Draw 1.
        await CardPileCmd.Draw(choiceContext, _drawCount, Owner);
    }

    protected override void OnUpgrade()
    {
        // Upgrade mechanics handled via _basePiety/_upgradedPiety check
    }
}

