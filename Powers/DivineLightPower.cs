using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

/// <summary>
/// 神光 — 每回合開始時獲得固定格擋（層數決定格擋量）。
/// </summary>
public class DivineLightPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/divine_light.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/divine_light.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
            await CreatureCmd.GainBlock(Owner, new BlockVar((int)Amount, 0), null);
    }
}

