using BaseLib.Abstracts;
using BaseLib.Cards.Variables;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

/// <summary>
/// 淨化 — 獲得大量虔誠。消耗。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class PurificationCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _pietyGain = 4;

    public PurificationCard() : base(0, type, rarity, targetType, shouldShowInLibrary) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new ExhaustiveVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PietyPower>(Owner.Creature, _pietyGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _pietyGain = 6;
    }
}
