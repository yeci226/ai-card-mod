using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：燃燒經文
/// 描述：累計{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]。隨機消耗你手牌中的一張牌，並將其費用加到本次累計的[gold]啟示[/gold]。
/// </summary>
public class BurningScriptureCard : CustomCardModel
{
    private const string RevelationGainKey = RevelationGainVar.Key;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(RevelationGainKey, 5).WithTooltip(RevelationGainVar.LocKey)
    ];

    public BurningScriptureCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int gain = DynamicVars[RevelationGainKey].IntValue;
        var handCards = CardPile.GetCards(Owner, [PileType.Hand]).Where(card => card != this).ToList();
        if (handCards.Count > 0)
        {
            var randomCard = handCards[Random.Shared.Next(handCards.Count)];
            if (randomCard != null)
            {
                await CardCmd.Exhaust(choiceContext, randomCard, false, false);
                gain += GetCardCost(randomCard);
            }
        }

        await PowerCmd.Apply<RevelationPower>(Owner.Creature, gain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[RevelationGainKey].UpgradeValueBy(3);
    }

    private static int GetCardCost(CardModel card)
    {
        var costProperty = card.GetType().GetProperty("EnergyCost");
        var costObj = costProperty?.GetValue(card);
        if (costObj == null)
            return 0;

        var valueProperty = costObj.GetType().GetProperty("CurrentValue") ?? costObj.GetType().GetProperty("Value") ?? costObj.GetType().GetProperty("BaseCost");
        if (valueProperty?.GetValue(costObj) is int value)
            return Math.Max(0, value);

        if (valueProperty?.GetValue(costObj) is decimal decimalValue)
            return Math.Max(0, (int)decimalValue);

        return 0;
    }
}