using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 迴響之刃的傷害累加器。
/// 每次啟示箭矢觸發時，由 RevelationPower 呼叫 RegisterArrowTrigger()，
/// 使 BonusDamage 永久遞增。
/// </summary>
public class EchoBladePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;

    /// <summary>累計的額外傷害（顯示於 Power 圖示上）。</summary>
    public int BonusDamage => (int)Amount;

    public int BonusPerTrigger { get; set; } = 1;

    /// <summary>由 RevelationPower 的箭矢迴圈每次觸發時呼叫。</summary>
    public async Task RegisterArrowTrigger(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<EchoBladePower>(Owner, BonusPerTrigger, Owner, null, silent: true);
    }
}
