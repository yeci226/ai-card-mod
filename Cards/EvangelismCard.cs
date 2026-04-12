using BaseLib.Abstracts;

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 傳教
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class EvangelismCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    public EvangelismCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        int damage = IsUpgraded ? 7 : 7;
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        await ProphetCardFactory.AddToHand<LightFlowCard>(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        // Upgrade effects handled in OnPlay if needed
    }
}

