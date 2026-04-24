using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：HolyChoir
/// 描述：啟示改為攻擊所有敵人。
/// </summary>
public class HolyChoirPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}