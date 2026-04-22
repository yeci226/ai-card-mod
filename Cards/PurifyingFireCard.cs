using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：淨化之火
/// 描述：造成等同你目前[gold]虔誠[/gold]點數的傷害{Repeat:diff()}次。
/// </summary>
public class PurifyingFireCard : CustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new RepeatVar(1)
    ];

    public PurifyingFireCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        int repeatCount = Math.Max(0, ResolveEnergyXValue());
        int faith = (int)(Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault()?.Amount ?? 0m);
        if (faith <= 0 || repeatCount <= 0)
            return;

        for (int index = 0; index < repeatCount; index++)
            await DamageCmd.Attack(faith).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Repeat"].UpgradeValueBy(1);
    }
}