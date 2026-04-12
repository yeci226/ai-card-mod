using BaseLib.Abstracts;

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 領域絕對。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class DomainAbsoluteCard : CustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public DomainAbsoluteCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int bonus = IsUpgraded ? 2 : 1;
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, bonus, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(Owner.Creature, bonus, Owner.Creature, this);
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    protected override void OnUpgrade()
    {
        // Upgrade effects handled in OnPlay if needed
    }
}

