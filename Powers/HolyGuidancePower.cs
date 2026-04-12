using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 聖光導引 — 每次打出攻擊卡時觸發效果。
/// </summary>
public class HolyGuidancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://aiCardMod/powers/holy_guidance.png";
    public override string? CustomBigIconPath => "res://aiCardMod/powers/holy_guidance.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 每回合開始時獲得虔誠
        if (player.Creature == Owner)
            await PowerCmd.Apply<PietyPower>(Owner, (int)Amount, Owner, null);
    }
}

