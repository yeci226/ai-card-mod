using BaseLib.Abstracts;

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 信仰崩塌
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class PietyCollapseCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public PietyCollapseCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        
        // Convert all current Piety into a one-turn offensive spike.
        if (piety > 0)
        {
            await PowerCmd.Apply<PietyPower>(Owner.Creature, -piety, Owner.Creature, this);
        }

        int valueBoost = Math.Max(0, piety);
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, valueBoost, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(Owner.Creature, valueBoost, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Upgrade effects handled in OnPlay if needed
    }
}

