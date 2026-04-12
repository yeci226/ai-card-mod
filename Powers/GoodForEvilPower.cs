using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 以善制惡 — 受到傷害時回復血量。
/// </summary>
public class GoodForEvilPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/good_for_evil.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/good_for_evil.png";
}

