using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 虔誠 — 用於決定神諭效果品質的疊加型能力。
/// 每回合結束時衰退：
///   無遺物 → 失去全部
///   神諭石板 → 失去一半（最少 1）
///   升級版 → 失去 25%（最少 1）
/// </summary>
public class PietyPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/piety.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/piety.png";

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || Amount <= 0) return;

        var relics = Owner.Player?.Relics;
        int decay;

        if (relics?.OfType<OracleTabletRelicUpgraded>().Any() == true)
            decay = Math.Max(1, (int)(Amount / 4));
        else if (relics?.OfType<OracleTabletRelic>().Any() == true)
            decay = Math.Max(1, (int)(Amount / 2));
        else
            decay = (int)Amount;

        await PowerCmd.Apply<PietyPower>(Owner, -decay, Owner, null);
    }
}
