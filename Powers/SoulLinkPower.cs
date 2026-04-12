using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 靈魂聯結 — 靈魂綁定效果，共享狀態數值。
/// </summary>
public class SoulLinkPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/soul_link.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/soul_link.png";
}

