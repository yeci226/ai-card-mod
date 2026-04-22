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
/// 名稱：聖水加持
/// 描述：抽{Cards:diff()}張牌。至多消耗{AICARDMOD-MaxPietySpend:diff()}點[gold]虔誠[/gold]，每消耗一點便額外抽{AICARDMOD-DrawPerFaith:diff()}張牌。
/// </summary>
public class HolyWaterBlessingCard : CustomCardModel
{
    private const string MaxSpendKey = "AICARDMOD-MaxPietySpend";
    private const string DrawPerFaithKey = "AICARDMOD-DrawPerFaith";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DynamicVar(MaxSpendKey, 2),
        new DynamicVar(DrawPerFaithKey, 1)
    ];

    public HolyWaterBlessingCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        int faith = (int)(Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault()?.Amount ?? 0m);
        int spend = Math.Min(faith, DynamicVars[MaxSpendKey].IntValue);
        if (spend <= 0)
            return;

        int drawPerFaith = DynamicVars[DrawPerFaithKey].IntValue;
        await PowerCmd.Apply<FaithPower>(Owner.Creature, -spend, Owner.Creature, this);
        await CardPileCmd.Draw(choiceContext, spend * drawPerFaith, Owner);
    }

    protected override void OnUpgrade() { }
}