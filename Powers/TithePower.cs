using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;



namespace AICardMod.Scripts;

/// <summary>
/// 名稱：Tithe
/// 描述：TODO: 補上在地化描述
/// </summary>
public class TithePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner.Player;
        if (player != null)
            room.AddExtraReward(player, new GoldReward(Amount, player, false));
        await Task.CompletedTask;
    }
}