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
/// 名稱：開導
/// 描述：造成{Damage:diff()}點傷害。你的消耗牌堆中每有一張《祈禱》，傷害增加{AICARDMOD-BonusPerPrayer:diff()}。
/// </summary>
public class EnlightenmentCard : CustomCardModel
{
    private const string BonusPerPrayerKey = "AICARDMOD-BonusPerPrayer";
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar(BonusPerPrayerKey, 3)
    ];

    public EnlightenmentCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
            return;

        int prayerCount = CardPile.GetCards(Owner, [PileType.Exhaust]).Count(card => card is PrayerCard);
        int totalDamage = DynamicVars.Damage.IntValue + prayerCount * DynamicVars[BonusPerPrayerKey].IntValue;
        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusPerPrayerKey].UpgradeValueBy(1);
    }
}