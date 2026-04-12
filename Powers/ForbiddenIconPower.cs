using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 禁忌聖像 — 將生命損失轉化為格擋。
/// </summary>
public class ForbiddenIconPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/forbidden_icon.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/forbidden_icon.png";
}

