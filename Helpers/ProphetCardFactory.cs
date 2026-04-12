using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

public static class ProphetCardFactory
{
    public static Task AddToHand<TCard>(PlayerChoiceContext choiceContext, Player player)
        where TCard : CustomCardModel, new() =>
        CardPileCmd.AddGeneratedCardToCombat(
            new TCard(),
            PileType.Hand,
            addedByPlayer: true,
            CardPilePosition.Top);
}
