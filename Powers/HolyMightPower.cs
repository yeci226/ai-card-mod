using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：聖力增幅
/// 描述：啟示傷害提高。
/// </summary>
public class HolyMightPower : CustomPowerModel
{
    public override string? CustomPackedIconPath => "res://aiCardMod/powers/holy_might.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/holy_might.png";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;
}