using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：NoAttackThisTurn
/// 描述：本回合你的攻擊不造成傷害，回合結束移除。
/// </summary>
public class NoAttackThisTurnPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner && amount > 0m && IsAttackCard(cardSource))
            return 0m;

        return amount;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;

        await PowerCmd.Remove(this);
    }

    private static bool IsAttackCard(CardModel? cardSource)
    {
        return cardSource?.Type == MegaCrit.Sts2.Core.Entities.Cards.CardType.Attack;
    }
}