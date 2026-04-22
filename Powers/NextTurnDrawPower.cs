using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;
/// <summary>
/// 名稱：下回抽牌
/// 描述：下回合開始時調整抽牌數，之後移除。正數代表多抽；負數目前暫不處理。
/// </summary>
public class NextTurnDrawPower : CustomPowerModel
{
    public override PowerType Type => Amount >= 0 ? PowerType.Buff : PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        int amount = (int)Amount;
        if (amount > 0)
            await CardPileCmd.Draw(choiceContext, amount, player);
        // Negative draw-reduction is approximated as no-op for now due missing direct API in this mod layer.

        await PowerCmd.Remove(this);
    }
}
