using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;
/// <summary>
/// 名稱：下回啟示
/// 描述：下回合開始時獲得等量啟示，之後移除。
/// </summary>
public class NextTurnRevelationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        int amount = (int)Amount;
        if (amount > 0)
            await PowerCmd.Apply<RevelationPower>(Owner, amount, Owner, null);

        await PowerCmd.Remove(this);
    }
}
