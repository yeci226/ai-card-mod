using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：異端審判
/// 描述：造成{Damage:diff()}點傷害。該敵人身上每有一種負面狀態，額外造成{AICARDMOD-BonusPerDebuff:diff()}點傷害。
/// </summary>
public class HeresyTrialCard : CustomCardModel
{
    private const string BonusPerDebuffKey = "AICARDMOD-BonusPerDebuff";
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new DynamicVar(BonusPerDebuffKey, 6)
    ];

    public HeresyTrialCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        int debuffTypes = cardPlay.Target.Powers?.Where(power => power.Type == PowerType.Debuff).Select(power => power.Id).Distinct().Count() ?? 0;
        int totalDamage = DynamicVars.Damage.IntValue + debuffTypes * DynamicVars[BonusPerDebuffKey].IntValue;
        await DamageCmd.Attack(totalDamage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars[BonusPerDebuffKey].UpgradeValueBy(2);
    }
}