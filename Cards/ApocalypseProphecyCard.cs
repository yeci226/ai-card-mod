using BaseLib.Abstracts;

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

/// <summary>
/// 末日預言
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class ApocalypseProphecyCard : CustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public ApocalypseProphecyCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int debuff = IsUpgraded ? 3 : 2;
        var enemies = Owner.Creature.CombatState?.Enemies.Where(e => e.IsAlive).ToList();
        if (enemies == null) return;

        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<WeakPower>(enemy, debuff, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(enemy, debuff, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Upgrade effects handled in OnPlay if needed
    }
}

