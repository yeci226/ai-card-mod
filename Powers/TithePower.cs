using System;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;



namespace AICardMod.Scripts;

/// <summary>
/// 名稱：Tithe
/// 描述：戰鬥結束時，獲得等量金幣。
/// </summary>
public class TithePower : CustomPowerModel
{
    public override string? CustomPackedIconPath => "res://aiCardMod/powers/tithe.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/tithe.png";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner.Player;
        if (player != null)
        {
            int goldGain = Math.Max(1, (int)Math.Round(player.Gold * 0.1f));
            room.AddExtraReward(player, new GoldReward(goldGain, player, false));
        }
        await Task.CompletedTask;
    }
}