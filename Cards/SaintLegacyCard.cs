using BaseLib.Abstracts;

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 聖徒遺產
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class SaintLegacyCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public SaintLegacyCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int basePiety = IsUpgraded ? 14 : 10;
        await PowerCmd.Apply<PietyPower>(Owner.Creature, basePiety, Owner.Creature, this);

        int perTurnPiety = IsUpgraded ? 2 : 1;
        // Graveyard-style sustained piety generation.

        await PowerCmd.Apply<SaintLegacyPower>(Owner.Creature, perTurnPiety, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Upgrade effects handled in OnPlay if needed
    }
}


