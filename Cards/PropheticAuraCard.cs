using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 先知光環 — 每回合對所有敵人施加易傷。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class PropheticAuraCard : CustomCardModel
{
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _vulnerable = 1;

    public PropheticAuraCard() : base(2, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PropheticAuraPower>(Owner.Creature, _vulnerable, Owner.Creature, this);
    }

    protected override void OnUpgrade() => _vulnerable = 2;
}
