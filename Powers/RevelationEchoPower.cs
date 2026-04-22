using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：啟示迴響
/// 描述：每當啟示觸發時，獲得格擋。
/// </summary>
public class RevelationEchoPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;
}