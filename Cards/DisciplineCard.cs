using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：戒律
/// 描述：本回合打出的下一張攻擊牌傷害增加{AICARDMOD-TemporaryStrengthPower:diff()}點。
/// </summary>
public class DisciplineCard : CustomCardModel
{
    private const string AmountKey = TemporaryStrengthPowerVar.Key;
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(AmountKey, 4)];

    public DisciplineCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TemporaryStrengthPower>(Owner.Creature, DynamicVars[AmountKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { DynamicVars[AmountKey].UpgradeValueBy(2); }
}
