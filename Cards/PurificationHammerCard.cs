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
/// 名稱：淨化之錘
/// 描述：造成{Damage:diff()}點傷害。移除目標敵人身上所有的[gold]失誤[/gold]，每移除一層，額外造成{AICARDMOD-BonusPerMisstep:diff()}點傷害。
/// </summary>
public class PurificationHammerCard : CustomCardModel
{
    private const string BonusPerMisstepKey = "AICARDMOD-BonusPerMisstep";
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18, ValueProp.Move),
        new DynamicVar(BonusPerMisstepKey, 10)
    ];

    public PurificationHammerCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
            return;

        int misstep = (int)(target.Powers?.OfType<MisstepPower>().FirstOrDefault()?.Amount ?? 0m);
        int totalDamage = DynamicVars.Damage.IntValue + misstep * DynamicVars[BonusPerMisstepKey].IntValue;

        if (misstep > 0)
            await PowerCmd.Apply<MisstepPower>(target, -misstep, Owner.Creature, this);

        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars[BonusPerMisstepKey].UpgradeValueBy(5);
    }
}