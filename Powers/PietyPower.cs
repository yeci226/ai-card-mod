using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

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
    private const int VulnerableThreshold = -3;
    private const int FrailThreshold = -6;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/piety.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/piety.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount >= 0) return;

        await PowerCmd.Apply<WeakPower>(Owner, 1, Owner, null);

        if (Amount <= VulnerableThreshold)
            await PowerCmd.Apply<VulnerablePower>(Owner, 1, Owner, null);

        if (Amount <= FrailThreshold)
            await PowerCmd.Apply<FrailPower>(Owner, 1, Owner, null);
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player || Amount == 0) return;

        if (Amount < 0)
        {
            await PowerCmd.Apply<PietyPower>(Owner, 1, Owner, null);
            return;
        }

        var relics = Owner.Player?.Relics;
        int decay;

        if (relics?.OfType<OracleTabletRelicUpgraded>().Any() == true)
            decay = Math.Max(1, (int)(Amount / 4));
        else if (relics?.OfType<OracleTabletRelic>().Any() == true)
            decay = Math.Max(1, (int)(Amount / 2));
        else
            decay = (int)Amount;

        if (relics?.OfType<PrayerBeadsRelicUpgraded>().Any() == true)
            decay = Math.Max(0, decay - 2);
        else if (relics?.OfType<PrayerBeadsRelic>().Any() == true)
            decay = Math.Max(0, decay - 1);

        if (decay <= 0) return;

        await PowerCmd.Apply<PietyPower>(Owner, -decay, Owner, null);
    }
}

