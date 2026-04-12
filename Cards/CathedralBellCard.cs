using BaseLib.Abstracts;

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 教堂鐘聲
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class CathedralBellCard : CustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInLibrary = true;

    public CathedralBellCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Damage all enemies and apply Vulnerable.
        int damage = IsUpgraded ? 14 : 10;
        int vulnerable = IsUpgraded ? 3 : 2;

        await CombatHelper.ForEachAliveEnemy(Owner.Creature, async enemy =>
        {
            await DamageCmd.Attack(damage)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);

            await PowerCmd.Apply<VulnerablePower>(enemy, vulnerable, Owner.Creature, this);
        });
    }

    protected override void OnUpgrade()
    {
        // Upgrade effects handled in OnPlay if needed
    }
}

