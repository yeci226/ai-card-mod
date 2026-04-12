using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 心靈寧靜 — 沒有打出攻擊卡時自動終結回合。
/// </summary>
public class QuietMindPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/quiet_mind.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/quiet_mind.png";
}

