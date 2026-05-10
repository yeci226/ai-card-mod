using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：末日審判
/// 描述：每次啟示觸發後，本回合後續啟示傷害提升（回合結束時重置計數）。
/// 實作：Amount 為每次啟示觸發後疊加的傷害量；回合結束時用 _turnBonus 記錄當回合加成，
///       RevelationPower 每發箭前從此 Power 讀取 Amount（基底），實際遞增邏輯在 RevelationPower 的 loop 內。
///       此 Power 永久保留（Amount 不重置），只控制「每箭增量」，不是「當回合累積值」。
/// </summary>
public class DoomsdayJudgmentPower : CustomPowerModel
{
    public override string? CustomPackedIconPath => "res://aiCardMod/powers/doomsday_judgment.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/doomsday_judgment.png";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;
}