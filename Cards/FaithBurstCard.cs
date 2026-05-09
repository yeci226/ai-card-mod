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
/// 名稱：信仰迸發
/// 描述：消耗所有【虔誠】。每消耗1層【虔誠】，造成{AICARDMOD-DamagePerFaith:diff()}點傷害。
/// 升級：每層改為造成3點傷害。
/// </summary>
public class FaithBurstCard : CustomCardModel
{
    private const string DamagePerFaithKey = "AICARDMOD-DamagePerFaith";
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FaithPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(DamagePerFaithKey, 2)
    ];

    public FaithBurstCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        int faith = (int)(Owner.Creature.GetPower<FaithPower>()?.Amount ?? 0m);
        if (faith <= 0) return;

        int damagePerFaith = DynamicVars[DamagePerFaithKey].IntValue;
        int totalDamage = faith * damagePerFaith;

        // Consume all faith
        await PowerCmd.Apply<FaithPower>(Owner.Creature, -faith, Owner.Creature, this);

        // Deal damage
        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DamagePerFaithKey].UpgradeValueBy(3);
    }
}
