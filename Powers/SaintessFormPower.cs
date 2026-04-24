using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：SaintessForm
/// 描述：每累積 7 次啟示觸發，下回合獲得 1 點能量。
/// </summary>
public class SaintessFormPower : CustomPowerModel
{
    private int _revelationTriggers;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task RegisterRevelationTriggers(PlayerChoiceContext choiceContext, int triggers)
    {
        if (triggers <= 0)
            return;

        _revelationTriggers += triggers;
        while (_revelationTriggers >= 7)
        {
            _revelationTriggers -= 7;
            await PowerCmd.Apply<NextTurnEnergyPower>(Owner, 1, Owner, null);
        }
    }
}