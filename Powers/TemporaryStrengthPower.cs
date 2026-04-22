using System;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;
/// <summary>
/// 名稱：TemporaryStrength
/// 描述：TODO: 補上在地化描述
/// </summary>
public class TemporaryStrengthPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
            await PowerCmd.Remove(this);
    }

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner && amount > 0m && IsAttackCard(cardSource))
            return amount + Amount;
        return amount;
    }

    private static bool IsAttackCard(CardModel? cardSource)
    {
        if (cardSource == null)
            return false;

        var cardTypeProperty = cardSource.GetType().GetProperty("Type");
        if (cardTypeProperty?.PropertyType != typeof(CardType))
            return false;

        return cardTypeProperty.GetValue(cardSource) is CardType cardType && cardType == CardType.Attack;
    }
}