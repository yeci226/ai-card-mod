using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;
/// <summary>
/// 名稱：儲備能量
/// 描述：下回合開始時，獲得這麼多能量，之後效果消失。
/// </summary>
public class NextTurnEnergyPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/next_turn_energy.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/next_turn_energy.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        int amount = (int)Amount;
        if (amount > 0)
            await PlayerCmd.GainEnergy(amount, player);
        else if (amount < 0)
            await PlayerCmd.LoseEnergy(-amount, player);
        await PowerCmd.Remove(this);
    }
}
