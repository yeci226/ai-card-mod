using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：懲戒
/// 描述：造成{Damage:diff()}點傷害。如果敵人的意圖是攻擊，則攻擊兩次。
/// </summary>
public class ChastisementCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    public ChastisementCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay, 1).Execute(choiceContext);
        if (cardPlay.Target != null && IsLikelyAttackIntent(cardPlay.Target))
            await CommonActions.CardAttack(this, cardPlay, 1).Execute(choiceContext);
    }

    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(2); }

    private static bool IsLikelyAttackIntent(Creature target)
    {
        return target.Monster?.IntendsToAttack ?? false;
    }
}
