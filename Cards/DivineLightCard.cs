using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：神啟
/// 描述：獲得{AICARDMOD-RevelationGain:diff()}點【啟示】。
/// </summary>
public class DivineLightCard : CustomCardModel
{
    private const string RevelationGainKey = RevelationGainVar.Key;
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(RevelationGainKey, 2).WithTooltip(RevelationGainVar.LocKey)
    ];

    public DivineLightCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RevelationPower>(Owner.Creature, DynamicVars[RevelationGainKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[RevelationGainKey].UpgradeValueBy(3);
    }
}
