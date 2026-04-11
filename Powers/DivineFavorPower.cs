using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 神明眷顧 — 每回合開始時獲得虔誠（層數決定數量）。
/// </summary>
public class DivineFavorPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/divine_favor.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/divine_favor.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
            await PowerCmd.Apply<PietyPower>(Owner, Amount, Owner, null);
    }
}
