using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 虔誠 — 用於決定神諭效果品質的疊加型能力。
/// 使用神諭時全數消耗，數量越高效果越佳。
/// </summary>
public class PietyPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/piety.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/piety.png";
}
