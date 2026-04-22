using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;
/// <summary>
/// 名稱：虔誠
/// 描述：不會在回合結束時消失的資源。
/// </summary>
public class FaithPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/piety.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/piety.png";
}
