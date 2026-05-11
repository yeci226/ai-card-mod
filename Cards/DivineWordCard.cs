using BaseLib.Abstracts;
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

    // VFX 路徑（官方格式 "vfx/vfx_xxx"）
    private const string StarryImpactVfx = "vfx/vfx_starry_impact";
    private const string SweepingBeamVfx = "vfx/vfx_sweeping_beam";

    /// <summary>
    /// 本回合已打出的牌數。
    /// 參考官方 Normality 做法：直接讀 PlayerCombatState.PlayPile，
    /// 不需要額外 Power 也不需要 AfterCardPlayed hook。
    /// </summary>
    private int CardsPlayedThisTurn =>
        Owner?.Creature?.Player?.PlayerCombatState?.PlayPile?.GetCards()?.Count() ?? 0;

    /// <summary>
    /// 預先宣告本卡需要的 VFX 場景（官方 FanOfKnives 做法）。
    /// </summary>
    protected override IEnumerable<string> ExtraRunAssetPaths =>
        [StarryImpactVfx, SweepingBeamVfx];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(DmgPerCardKey, 3)];

    public DivineWordCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    /// <summary>
    /// 打出後回到手牌 — 官方 ParticleWall 做法：override GetResultPileType。
    /// </summary>
    protected override PileType GetResultPileType() => PileType.Hand;

    /// <summary>
    /// 出牌 VFX — 官方做法：接受 Creature? target 一個參數。
    /// </summary>
    public override async Task OnEnqueuePlayVfx(Creature? target)
    {
        if (Owner?.Creature != null)
            await VfxCmd.PlayOnCreature(Owner.Creature, SweepingBeamVfx);
        if (target != null)
            await VfxCmd.PlayOnCreature(target, StarryImpactVfx);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
            return;

        // PlayPile 在 OnPlay 執行時已包含本卡。
        int cardsPlayed = Math.Max(1, CardsPlayedThisTurn);
        int damage = cardsPlayed * DynamicVars[DmgPerCardKey].IntValue;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DmgPerCardKey].UpgradeValueBy(1);
    }
}
