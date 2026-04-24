using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：CleanseSin
/// 描述：每當你失去虔誠時，獲得等量格擋。
/// </summary>
public class CleanseSinPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;
}