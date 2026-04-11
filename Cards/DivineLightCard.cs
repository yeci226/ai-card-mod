using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 神光 — 每回合開始時獲得格擋。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class DivineLightCard : CustomCardModel
{
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _blockPerTurn = 3;

    public DivineLightCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DivineLightPower>(Owner.Creature, _blockPerTurn, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _blockPerTurn = 5;
    }
}
