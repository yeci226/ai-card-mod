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
/// 名稱：心靈感應
/// 描述：選擇你抽牌堆中的{Cards:diff()}張牌，放到抽牌堆頂部。
/// </summary>
public class TelepathyCard : CustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [] : [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public TelepathyCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = DynamicVars.Cards.IntValue;
        if (count <= 0)
            return;

        var drawPile = CardPile.Get(PileType.Draw, Owner);
        if (drawPile == null)
        {
            await CardPileCmd.Draw(choiceContext, count, Owner);
            return;
        }

        var picked = drawPile.Cards
            .Take(count)
            .ToList();

        foreach (var card in picked)
            drawPile.MoveToTopInternal(card);
    }

    protected override void OnUpgrade() { }
}