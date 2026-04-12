using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 避難所 — 永久儲存格擋，不會在回合結束時消散。
/// </summary>
public class RefugePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/refuge.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/refuge.png";
}

