using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 業力轉移 — 轉化傷害為業力資源。
/// </summary>
public class KarmaTransferPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/karma_transfer.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/karma_transfer.png";
}

