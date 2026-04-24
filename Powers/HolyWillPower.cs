using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：HolyWill
/// 描述：本回合下一張打出的牌會額外再打一次。
/// 每觸發一次消耗一層。
/// </summary>
public class HolyWillPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => (int)Amount;

    /// <summary>
    /// 在打出一張牌之後，如果本能力還有層數，消耗一層並自動再打一次同張牌。
    /// </summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner)
            return;

        if (Amount <= 0m)
            return;

        await PowerCmd.Remove(this);
        await CardCmd.AutoPlay(context, cardPlay.Card, cardPlay.Target);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
            return;

        await PowerCmd.Remove(this);
    }
}