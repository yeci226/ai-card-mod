using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 先知之眼 — 每回合開始時若虔誠 ≥ 5，額外抽一張牌。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class ProphetEyeCard : CustomCardModel
{
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _pietyThreshold = 5;

    public ProphetEyeCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ProphetEyePower>(Owner.Creature, _pietyThreshold, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _pietyThreshold = 3;
    }
}
