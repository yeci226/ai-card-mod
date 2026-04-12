using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 荊棘試煉 — 敵人造成傷害時對其反彈傷害。
/// </summary>
public class ThornTrialPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/thorn_trial.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/thorn_trial.png";
}

