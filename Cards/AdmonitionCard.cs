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
/// 名稱：警世
/// 描述：給予{AICARDMOD-MisstepGain:diff()}層[gold]失誤[/gold]。獲得{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]。
/// </summary>
public class AdmonitionCard : CustomCardModel
{
    private const string MisstepGainKey = MisstepGainVar.Key;
    private const string RevelationGainKey = RevelationGainVar.Key;

    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(RevelationGainKey, 2).WithTooltip(RevelationGainVar.LocKey), new DynamicVar(MisstepGainKey, 1).WithTooltip(MisstepGainVar.LocKey)];

    public AdmonitionCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await PowerCmd.Apply<MisstepPower>(cardPlay.Target, DynamicVars[MisstepGainKey].IntValue, Owner.Creature, this);
        await PowerCmd.Apply<RevelationPower>(Owner.Creature, DynamicVars[RevelationGainKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[RevelationGainKey].UpgradeValueBy(1);
        DynamicVars[MisstepGainKey].UpgradeValueBy(1);
    }
}
