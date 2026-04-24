using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：天啟預兆
/// 描述：抽{Cards:diff()}張牌。打出其中帶有[gold]啟示[/gold]效果的牌。
/// </summary>
public class ApocalypseOmenCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<RevelationPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public ApocalypseOmenCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handBefore = CardPile.GetCards(Owner, [PileType.Hand]).ToHashSet();
        int draw = DynamicVars.Cards.IntValue;
        await CardPileCmd.Draw(choiceContext, draw, Owner);

        var drawnCards = CardPile.GetCards(Owner, [PileType.Hand])
            .Where(card => !handBefore.Contains(card))
            .ToList();

        var revelationCards = drawnCards.Where(CardHasRevelationTrait).ToList();
        int fallbackRevelationGain = 0;
        foreach (var drawnCard in revelationCards)
        {
            bool played = await TryAutoPlayCard(choiceContext, drawnCard, cardPlay.Target);
            if (!played)
                fallbackRevelationGain += 2;
        }

        if (fallbackRevelationGain > 0)
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, fallbackRevelationGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }

    private static bool CardHasRevelationTrait(CardModel card)
    {
        if (card.GetType().Name.Contains("Revelation", StringComparison.OrdinalIgnoreCase))
            return true;

        var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        foreach (var property in card.GetType().GetProperties(flags))
        {
            if (property.PropertyType != typeof(string))
                continue;

            if (property.GetValue(card) is string value &&
                (value.Contains("REVELATION", StringComparison.OrdinalIgnoreCase)
                 || value.Contains("AICARDMOD-Revelation", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<bool> TryAutoPlayCard(PlayerChoiceContext choiceContext, CardModel card, Creature? preferredTarget)
    {
        try
        {
            await CardCmd.AutoPlay(choiceContext, card, preferredTarget, AutoPlayType.Default, false, false);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
