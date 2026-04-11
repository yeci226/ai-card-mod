using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 先知光環 — 每回合對所有存活敵人施加易傷（層數決定數量）。
/// </summary>
public class PropheticAuraPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/prophetic_aura.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/prophetic_aura.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        var enemies = Owner.CombatState?.Enemies.Where(e => e.IsAlive).ToList() ?? [];
        foreach (var enemy in enemies)
            await PowerCmd.Apply<VulnerablePower>(enemy, (int)Amount, Owner, null);
    }
}
