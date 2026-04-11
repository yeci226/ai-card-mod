using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 神聖共鳴 — 虔誠充足時每回合獲得力量。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class HolyResonanceCard : CustomCardModel
{
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _pietyThreshold = 3;

    public HolyResonanceCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HolyResonancePower>(Owner.Creature, _pietyThreshold, Owner.Creature, this);
    }

    protected override void OnUpgrade() => _pietyThreshold = 1;
}
