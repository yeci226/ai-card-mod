using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：FinalRevelationLock
/// 描述：無法再獲得啟示。
/// 實作：覆寫 TryModifyPowerAmountReceived，將 RevelationPower 的增益量歸零。
/// </summary>
public class FinalRevelationLockPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 當擁有者被施加 RevelationPower 且增益量為正數時，攔截並歸零。
    /// </summary>
    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        decimal amount,
        Creature? applier,
        out decimal modifiedAmount)
    {
        if (target == Owner && canonicalPower is RevelationPower && amount > 0m)
        {
            modifiedAmount = 0m;
            return true;
        }

        modifiedAmount = amount;
        return false;
    }
}