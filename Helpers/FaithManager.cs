using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

/// <summary>
/// 信仰管理器 — 負責虔誠數值的增減。
///
/// 虔誠是一種不會隨回合結束而消失的資源，和啟示沒有直接關聯。
/// </summary>
public static class FaithManager
{
    /// <summary>
    /// 便利方法：套用虔誠。
    /// </summary>
    public static async Task ApplyFaith(
        Creature owner,
        int amount,
        CustomCardModel? sourceCard = null)
    {
        await PowerCmd.Apply<FaithPower>(owner, amount, owner, sourceCard);

        if (amount < 0)
        {
            int perFaithBlock = (int)(owner.Powers?.OfType<CleanseSinPower>().FirstOrDefault()?.Amount ?? 0m);
            if (perFaithBlock > 0)
                await CreatureCmd.GainBlock(owner, -amount * perFaithBlock, ValueProp.Move | ValueProp.Unpowered, null);
        }
    }
}
