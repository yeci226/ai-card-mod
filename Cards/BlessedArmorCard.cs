using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

/// <summary>
/// 神佑護甲 — 每回合開始時根據虔誠獲得格擋。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class BlessedArmorCard : CustomCardModel
{
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _bonusBlock = 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BonusBlock", _bonusBlock)];

    public BlessedArmorCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BlessedArmorPower>(Owner.Creature, _bonusBlock, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _bonusBlock = 2;
        DynamicVars["BonusBlock"].BaseValue = _bonusBlock;
    }
}
