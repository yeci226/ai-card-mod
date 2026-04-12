using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

/// <summary>
/// 神佑護甲 — 每回合開始時獲得格擋，數量等於當前虔誠（升級後額外加成）。
/// </summary>
public class BlessedArmorPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/blessed_armor.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/blessed_armor.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        int piety = (int)(Owner.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int block = piety + (int)Amount;

        if (block > 0)
            await CreatureCmd.GainBlock(Owner, new BlockVar(block, 0), null);
    }
}

