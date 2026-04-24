using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：RevelationRetainPercent
/// 描述：回合結束時，保留這個百分比的啟示而非全部消耗。
/// </summary>
public class RevelationRetainPercentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override int DisplayAmount => (int)Amount;
}