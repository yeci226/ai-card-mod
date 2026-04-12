using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 神聖屏障
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class HolyBarrierCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public HolyBarrierCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Base block plus conditional Weak application if Piety is present.
        int block = IsUpgraded ? 11 : 8;
        int weakness = IsUpgraded ? 2 : 1;

        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(block, 0), null);

        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        if (piety > 0)
        {
            var enemies = Owner.Creature.CombatState?.Enemies.Where(e => e.IsAlive).ToList();
            if (enemies != null)
            {
                foreach (var enemy in enemies)
                {
                    await PowerCmd.Apply<WeakPower>(enemy, weakness, Owner.Creature, this);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        // Upgrade effects handled in OnPlay if needed
    }
}



