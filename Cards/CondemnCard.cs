using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
public class CondemnCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    private int _weak = 2;
    private int _vulnerable = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("WeakAmount", _weak), new DynamicVar("VulnAmount", _vulnerable)];

    public CondemnCard() : base(1, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, _weak, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, _vulnerable, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _weak = 3;
        _vulnerable = 3;
        DynamicVars["WeakAmount"].BaseValue = _weak;
        DynamicVars["VulnAmount"].BaseValue = _vulnerable;
    }
}
