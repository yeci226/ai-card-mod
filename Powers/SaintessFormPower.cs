using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：SaintessForm
/// 描述：每累積 7 次啟示觸發，下回合獲得 1 點能量。
/// 實作：用 Amount 做計數器（可序列化），每 7 次觸發後減 7 並給予能量。
/// </summary>
public class SaintessFormPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task RegisterRevelationTriggers(PlayerChoiceContext choiceContext, int triggers)
    {
        if (triggers <= 0)
            return;

        await PowerCmd.Apply<SaintessFormPower>(Owner, triggers, Owner, null);

        while (Amount >= 7m)
        {
            await PowerCmd.Apply<SaintessFormPower>(Owner, -7, Owner, null);
            await PowerCmd.Apply<NextTurnEnergyPower>(Owner, 1, Owner, null);
        }
    }
}