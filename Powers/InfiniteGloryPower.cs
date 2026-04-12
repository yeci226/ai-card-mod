using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 無限榮耀 — 虔誠翻倍效果，可疊加。
/// </summary>
public class InfiniteGloryPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/infinite_glory.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/infinite_glory.png";
}

