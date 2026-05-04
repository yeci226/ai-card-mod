using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：HolyChalice
/// 描述：每當你累計【啟示】時，額外累計 N 點。
/// 實作：覆寫 TryModifyPowerAmountReceived，攔截 RevelationPower 的正向增益並追加等量倍數。
/// </summary>
public class HolyChalicePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;

    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        MegaCrit.Sts2.Core.Entities.Creatures.Creature target,
        decimal amount,
        MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier,
        out decimal modifiedAmount)
    {
        // 不修改原始數值，讓它正常套用；另外追加 Amount 點啟示
        modifiedAmount = amount;

        if (target == Owner && canonicalPower is RevelationPower && amount > 0m && Amount > 0m)
        {
            // 使用 fire-and-forget 方式追加（框架內常見做法）
            _ = PowerCmd.Apply<RevelationPower>(Owner, Amount, Owner, null);
        }

        return false;
    }
}