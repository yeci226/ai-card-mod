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
/// 描述：每回合結束時，每3點啟示會轉化為1發神聖箭矢。
/// </summary>
public class RevelationPower : CustomPowerModel
{
    private const int DivineArrowDamage = 3;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;

        int arrowCount = (int)(Amount / 3m);
        if (arrowCount <= 0)
            return;

        var enemies = CombatState.HittableEnemies.Where(enemy => enemy.IsAlive).ToList();
        if (enemies.Count == 0)
            return;

        int echoBlock = (int)(Owner.Powers?.OfType<RevelationEchoPower>().FirstOrDefault()?.Amount ?? 0m);
        int holyMight = (int)(Owner.Powers?.OfType<HolyMightPower>().FirstOrDefault()?.Amount ?? 0m);
        int doomsdayTurnGain = (int)(Owner.Powers?.OfType<DoomsdayJudgmentPower>().FirstOrDefault()?.Amount ?? 0m);
        int retainPercent = (int)(Owner.Powers?.OfType<RevelationRetainPercentPower>().MaxBy(power => power.Amount)?.Amount ?? 0m);
        bool choirAllEnemies = (Owner.Powers?.OfType<HolyChoirPower>().Any() ?? false);
        int doomsdayTurnBonus = 0;

        var focusTargets = enemies.Where(enemy => enemy.Powers?.OfType<RevelationFocusPower>().Any() ?? false).ToList();
        var saintessForm = Owner.Powers?.OfType<SaintessFormPower>().FirstOrDefault();

        for (int index = 0; index < arrowCount; index++)
        {
            int arrowDamage = Math.Max(0, DivineArrowDamage + holyMight + doomsdayTurnBonus);

            if (choirAllEnemies)
            {
                foreach (var enemy in enemies.Where(enemy => enemy.IsAlive))
                {
                    await CreatureCmd.Damage(
                        choiceContext,
                        enemy,
                        arrowDamage,
                        ValueProp.Move | ValueProp.Unblockable | ValueProp.Unpowered,
                        Owner,
                        null);
                }
            }
            else
            {
                var targetPool = focusTargets.Count > 0 ? focusTargets : enemies;
                var target = Owner.Player?.RunState.Rng.CombatTargets.NextItem(targetPool);
                if (target == null)
                    break;

                await CreatureCmd.Damage(
                    choiceContext,
                    target,
                    arrowDamage,
                    ValueProp.Move | ValueProp.Unblockable | ValueProp.Unpowered,
                    Owner,
                    null);
            }

            if (echoBlock > 0)
                await CreatureCmd.GainBlock(Owner, echoBlock, ValueProp.Move | ValueProp.Unpowered, null);

            if (doomsdayTurnGain > 0)
                doomsdayTurnBonus += doomsdayTurnGain;
        }

        if (saintessForm != null)
            await saintessForm.RegisterRevelationTriggers(choiceContext, arrowCount);

        int consumed = arrowCount * 3;
        int retained = (int)Math.Ceiling(consumed * (Math.Max(0, retainPercent) / 100m));
        int delta = -consumed + retained;
        var finalAmount = Amount + delta;

        if (finalAmount > 0m)
            await PowerCmd.Apply<RevelationPower>(Owner, delta, Owner, null, silent: true);
        else
            await PowerCmd.Remove(this);
    }
}