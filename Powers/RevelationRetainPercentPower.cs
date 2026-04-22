using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：RevelationRetainPercent
/// 描述：TODO: 補上在地化描述
/// </summary>
public class RevelationRetainPercentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;
}