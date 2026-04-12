using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 聖血修養 — 累積聖血層數，影響後續卡牌效果。
/// </summary>
public class HolyBloodCultivationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/holy_blood_cultivation.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/holy_blood_cultivation.png";
}

