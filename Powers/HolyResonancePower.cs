using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// Holy cards gain bonus damage and block while this power is active.
/// </summary>
public class HolyResonancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/holy_resonance.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/holy_resonance.png";
}

