using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 神諭石板（升級版）
/// 每回合結束時補回 2 層虔誠，完全抵消衰退（淨衰退 0）。
/// </summary>
[Pool(typeof(ProphetRelicPool))]
public class OracleTabletRelicUpgraded : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player)
        {
            var creature = Owner?.Creature;
            var piety = creature?.Powers?.OfType<PietyPower>().FirstOrDefault();
            if (creature != null && piety != null && piety.Amount > 0)
                await PowerCmd.Apply<PietyPower>(creature, 2, creature, null);
        }
    }
}
