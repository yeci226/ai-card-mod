using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Linq;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：啟示
/// 描述：每回合結束時，每3點啟示會轉化為1發神聖箭矢。若無保留能力，剩餘層數會清空。
/// </summary>
public class RevelationPower : CustomPowerModel
{
    public override string? CustomPackedIconPath => "res://aiCardMod/powers/revelation.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/revelation.png";

    private const int DivineArrowDamage = 3;
    // 神聖箭矢：使用星光衝擊特效呈現神聖感
    private const string ArrowVfxPath = "vfx/vfx_starry_impact";
    // 啟示結算時的光暈（施放者身上）
    private const string CastVfxPath  = "vfx/vfx_ghostly_power_up";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;

        int totalAtTurnEnd = (int)Amount;
        int arrowCount = totalAtTurnEnd / 3;

        int retainPercent = (int)(Owner.Powers?.OfType<RevelationRetainPercentPower>().MaxBy(p => p.Amount)?.Amount ?? 0m);
        var enemies = CombatState.HittableEnemies.Where(enemy => enemy.IsAlive).ToList();

        if (arrowCount > 0 && enemies.Count > 0)
        {
            // 啟示開始結算時，在先知身上播放光暈特效
            VfxCmd.PlayOnCreature(Owner, CastVfxPath);
            int echoBlock = (int)(Owner.Powers?.OfType<RevelationEchoPower>().FirstOrDefault()?.Amount ?? 0m);
            int holyMight = (int)(Owner.Powers?.OfType<HolyMightPower>().FirstOrDefault()?.Amount ?? 0m);
            int doomsdayTurnGain = (int)(Owner.Powers?.OfType<DoomsdayJudgmentPower>().FirstOrDefault()?.Amount ?? 0m);
            bool choirAllEnemies = Owner.Powers?.OfType<HolyChoirPower>().Any() ?? false;
            int doomsdayTurnBonus = 0;

            var focusTargets = enemies.Where(enemy => enemy.Powers?.OfType<RevelationFocusPower>().Any() ?? false).ToList();
            var saintessForm = Owner.Powers?.OfType<SaintessFormPower>().FirstOrDefault();

            for (int index = 0; index < arrowCount; index++)
            {
                int arrowDamage = Math.Max(0, DivineArrowDamage + holyMight + doomsdayTurnBonus);

                if (choirAllEnemies)
                {
                    foreach (var enemy in enemies.Where(e => e.IsAlive))
                    {
                        VfxCmd.PlayOnCreature(enemy, ArrowVfxPath);
                        await CreatureCmd.Damage(choiceContext, enemy, arrowDamage, ValueProp.Move | ValueProp.Unpowered, Owner);
                    }
                }
                else
                {
                    var aliveFocusTargets = focusTargets.Where(e => e.IsAlive).ToList();
                    var aliveEnemies = enemies.Where(e => e.IsAlive).ToList();
                    if (aliveEnemies.Count == 0) break;
                    var targetPool = aliveFocusTargets.Count > 0 ? aliveFocusTargets : aliveEnemies;
                    var target = Owner.Player?.RunState.Rng.CombatTargets.NextItem(targetPool);
                    if (target == null) break;

                    VfxCmd.PlayOnCreature(target, ArrowVfxPath);
                    await CreatureCmd.Damage(choiceContext, target, arrowDamage, ValueProp.Move | ValueProp.Unpowered, Owner);
                }

                if (echoBlock > 0)
                    await CreatureCmd.GainBlock(Owner, echoBlock, ValueProp.Move | ValueProp.Unpowered, null);

                if (doomsdayTurnGain > 0)
                    doomsdayTurnBonus += doomsdayTurnGain;

                // Notify EchoBlade of each arrow trigger
                var echoBlade = Owner.Powers?.OfType<EchoBladePower>().FirstOrDefault();
                if (echoBlade != null)
                    await echoBlade.RegisterArrowTrigger(choiceContext);

                Flash();
            }

            if (saintessForm != null)
                await saintessForm.RegisterRevelationTriggers(choiceContext, arrowCount);
        }


        int retained = (int)Math.Ceiling(totalAtTurnEnd * (Math.Max(0, retainPercent) / 100m));
        int delta = -totalAtTurnEnd + retained;
        var finalAmount = Amount + delta;

        if (finalAmount > 0m)
        {
            await PowerCmd.Apply<RevelationPower>(Owner, delta, Owner, null, silent: true);
        }
        else
        {
            await PowerCmd.Remove(this);
        }
    }
}