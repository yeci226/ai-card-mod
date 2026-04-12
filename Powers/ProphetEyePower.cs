using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// At the start of turn, if Piety meets the threshold, add a Prayer to hand.
/// </summary>
public class ProphetEyePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/prophet_eye.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/prophet_eye.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        int piety = (int)(Owner.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int threshold = (int)Amount;

        if (piety >= threshold)
            await ProphetCardFactory.AddToHand<PrayCard>(choiceContext, player);
    }
}

