using BaseLib.Abstracts;
using BaseLib.Cards.Variables;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

/// <summary>
/// 神諭洞見 — 抽牌並獲得虔誠。消耗。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class OracleInsightCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _draw = 3;
    private int _pietyGain = 2;

    public OracleInsightCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new ExhaustiveVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, _draw, Owner);
        await PowerCmd.Apply<PietyPower>(Owner.Creature, _pietyGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _draw = 4;
        _pietyGain = 3;
    }
}
