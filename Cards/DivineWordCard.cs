using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：聖言降世
/// 描述：造成本回合已打出牌數 × {AICARDMOD-DmgPerCard:diff()} 點傷害。打出後返回手牌。
/// </summary>
public class DivineWordCard : CustomCardModel
{
    private const string DmgPerCardKey = "AICARDMOD-DmgPerCard";
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(DmgPerCardKey, 3)];

    public DivineWordCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
            return;

        // Ensure the counter power is active so future plays are tracked.
        if (!Owner.Creature.HasPower<CardsPlayedCounterPower>())
            await PowerCmd.Apply<CardsPlayedCounterPower>(Owner.Creature, 1, Owner.Creature, this);

        // Amount already includes this card (AfterCardPlayed fires before OnPlay returns,
        // but OnPlay fires first — so we read the count BEFORE it increments via AfterCardPlayed).
        // We add 1 manually to count this card itself.
        int cardsPlayed = (int)(Owner.Creature.Powers?.OfType<CardsPlayedCounterPower>().FirstOrDefault()?.Amount ?? 0m) + 1;
        int damage = cardsPlayed * DynamicVars[DmgPerCardKey].IntValue;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // Return this card to hand.
        await CardPileCmd.AddGeneratedCardToCombat(
            this,
            PileType.Hand,
            addedByPlayer: false,
            CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DmgPerCardKey].UpgradeValueBy(1);
    }
}
