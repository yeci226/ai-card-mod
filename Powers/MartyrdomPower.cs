using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 殉道 — 當受到傷害時對隨機敵人造成傷害。
/// </summary>
public class MartyrdomPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/martyrdom.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/martyrdom.png";
}

