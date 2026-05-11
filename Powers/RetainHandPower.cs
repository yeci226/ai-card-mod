using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 保留手牌（本回合）：在回合結束棄牌前，對所有手牌施加 Retain。
/// 觸發後自動移除。
/// </summary>
public class RetainHandPower : CustomPowerModel
{
    public override string? CustomPackedIconPath => "res://aiCardMod/powers/retain_hand.png";
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;

        var player = Owner.Player;
        if (player == null)
            return;

        var handCards = CardPile.GetCards(player, [PileType.Hand]).ToList();
        foreach (var card in handCards)
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
        }

        await PowerCmd.Remove(this);
    }
}
