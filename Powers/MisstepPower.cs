using System;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;
/// <summary>
/// 名稱：失誤
/// 描述：每次攻擊傷害降低 25%，每觸發一次減少 1 層。
/// </summary>
public class MisstepPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner || amount <= 0m || Amount <= 0m)
            return amount;

        decimal reducedAmount = Math.Max(0m, amount * 0.75m);
        _ = PowerCmd.Apply<MisstepPower>(Owner, -1, Owner, null);
        return reducedAmount;
    }
}