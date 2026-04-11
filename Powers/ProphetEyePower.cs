using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 先知之眼 — 每回合開始時，若虔誠層數 ≥ 閾值，額外抽一張牌。
/// 層數儲存的是虔誠閾值。
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
            await CardPileCmd.Draw(choiceContext, 1, player);
    }
}
