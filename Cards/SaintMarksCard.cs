using BaseLib.Abstracts;

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 聖痕
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class SaintMarksCard : CustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public SaintMarksCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int energy = 2;
        int healthPenalty = IsUpgraded ? 3 : 5;
        
        await PlayerCmd.GainEnergy(energy, Owner);
        await PowerCmd.Apply<WeakPower>(Owner.Creature, Math.Max(1, healthPenalty / 2), Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Upgrade effects handled in OnPlay if needed
    }
}

