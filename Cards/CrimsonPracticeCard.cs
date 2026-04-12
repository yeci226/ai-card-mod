using BaseLib.Abstracts;

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 紅蓮火刑
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class CrimsonPracticeCard : CustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInLibrary = true;

    public CrimsonPracticeCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int baseDamage = 18;
        int upgradedDamage = 24;
        int damage = IsUpgraded ? upgradedDamage : baseDamage;

        // Apply AOE with extra damage per enemy debuff.
        var enemies = Owner.Creature.CombatState?.Enemies.Where(e => e.IsAlive).ToList();
        if (enemies != null)
        {
            foreach (var enemy in enemies)
            {
                int totalDamage = damage;
                
                // Approximate "negative effects" as count of debuff powers.
                int debuffBonus = IsUpgraded ? 14 : 10;
                int debuffCount = enemy.Powers?.Count(p => p.Type == PowerType.Debuff) ?? 0;
                totalDamage += debuffCount * debuffBonus;
                
                await DamageCmd.Attack(totalDamage)
                    .FromCard(this)
                    .Targeting(enemy)
                    .Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // Upgrade effects handled in OnPlay if needed
    }
}

