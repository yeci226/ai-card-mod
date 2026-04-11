using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 神明眷顧 — 每回合自動獲得虔誠。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class DivineFavorCard : CustomCardModel
{
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _pietyPerTurn = 1;

    public DivineFavorCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DivineFavorPower>(Owner.Creature, _pietyPerTurn, Owner.Creature, this);
    }

    protected override void OnUpgrade() => _pietyPerTurn = 2;
}
