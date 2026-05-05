using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：神判之手
/// 描述：造成等同於你的【虔誠】層數的傷害。失去等同於你手牌數量的【虔誠】。若此牌擊殺敵人，恢復等同失去的【虔誠】數量。
/// 升級：傷害額外增加5點。
/// </summary>
public class HolyJudgmentHandCard : CustomCardModel
{
    private const string BonusDamageKey = "AICARDMOD-HolyJudgmentBonus";
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FaithPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(BonusDamageKey, 0)
    ];

    public HolyJudgmentHandCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        int faith = (int)(Owner.Creature.GetPower<FaithPower>()?.Amount ?? 0m);
        int bonus = DynamicVars[BonusDamageKey].IntValue;
        int totalDamage = Math.Max(0, faith + bonus);

        // Attack for faith + bonus damage
        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);

        // Lose faith equal to hand size (not counting self, since already played)
        int handCount = CardPile.GetCards(Owner, [PileType.Hand]).Count();
        int faithLost = Math.Min(faith, handCount);

        if (faithLost > 0)
            await PowerCmd.Apply<FaithPower>(Owner.Creature, -faithLost, Owner.Creature, this);

        // If target is dead, restore the faith lost
        if (!cardPlay.Target.IsAlive && faithLost > 0)
            await FaithManager.ApplyFaith(Owner.Creature, faithLost, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusDamageKey].UpgradeValueBy(5);
    }
}
