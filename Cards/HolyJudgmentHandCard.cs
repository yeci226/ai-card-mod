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
/// 神判之手 (Holy Judgment Hand)
/// 費用：1  類型：Attack  稀有度：Uncommon
///
/// 描述：造成等同於你的【虔誠】層數的傷害。失去等同於你手牌數量的【虔誠】。
///       若此牌擊殺敵人，恢復等同失去的【虔誠】數量，並將此牌返回手牌（本回合費用為0）。
/// 升級：傷害額外增加5點。
/// </summary>
public class HolyJudgmentHandCard : CustomCardModel
{
    private const string BonusDamageKey = "AICARDMOD-HolyJudgmentBonus";
    private const int BaseCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    // 擊殺後返回手牌的標記：true = 下一次打出時恢復費用
    private bool _killReturned = false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FaithPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(BonusDamageKey, 0)
    ];

    public HolyJudgmentHandCard() : base(BaseCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 若此次打出是因擊殺回手後的免費打出，恢復費用
        if (_killReturned)
        {
            _killReturned = false;
            EnergyCost.SetThisCombat(BaseCost);
        }

        int faith = (int)(Owner.Creature.GetPower<FaithPower>()?.Amount ?? 0m);
        int bonus = DynamicVars[BonusDamageKey].IntValue;
        int totalDamage = Math.Max(0, faith + bonus);

        // 攻擊
        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);

        // 失去虔誠 = 手牌數（自身已打出，不計入）
        int handCount = CardPile.GetCards(Owner, [PileType.Hand]).Count();
        int faithLost = Math.Min(faith, handCount);

        if (faithLost > 0)
            await PowerCmd.Apply<FaithPower>(Owner.Creature, -faithLost, Owner.Creature, this);

        // 擊殺回報
        if (!cardPlay.Target.IsAlive)
        {
            // 恢復損失的虔誠
            if (faithLost > 0)
                await FaithManager.ApplyFaith(Owner.Creature, faithLost, this);

            // 將此牌設為本回合0費，返回手牌
            _killReturned = true;
            EnergyCost.SetThisCombat(0);
            await CardPileCmd.AddGeneratedCardToCombat(
                this,
                PileType.Hand,
                addedByPlayer: true,
                CardPilePosition.Top);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusDamageKey].UpgradeValueBy(5);
    }
}
