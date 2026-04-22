using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：真理之言
/// 描述：使用時，手牌中每有一張牌，獲得{AICARDMOD-RevelationPerCard:diff()}點[gold]啟示[/gold]。
/// </summary>
public class TruthWordCard : CustomCardModel
{
    private const string RevelationPerCardKey = "AICARDMOD-RevelationPerCard";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded ? [CardKeyword.Exhaust, CardKeyword.Retain] : [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(RevelationPerCardKey, 1)];

    public TruthWordCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int handCount = CardPile.GetCards(Owner, [PileType.Hand]).Count();
        int revelationPerCard = DynamicVars[RevelationPerCardKey].IntValue;
        if (handCount > 0)
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, handCount * revelationPerCard, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }
}