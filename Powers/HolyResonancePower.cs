using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 神聖共鳴 — 每回合開始時，若虔誠 ≥ 閾值，獲得 1 層力量。
/// </summary>
public class HolyResonancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/holy_resonance.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/holy_resonance.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        int piety = (int)(Owner.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int threshold = (int)Amount;

        if (piety >= threshold)
            await PowerCmd.Apply<StrengthPower>(Owner, 1, Owner, null);
    }
}
