using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：朝聖之路
/// 描述：在下個回合獲得{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]並獲得{EnergyNextTurn:diff()}點能量。
/// </summary>
public class PilgrimageRoadCard : CustomCardModel
{
    private const string RevelationGainKey = RevelationGainVar.Key;
    private const string EnergyNextTurnKey = "EnergyNextTurn";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(RevelationGainKey, 3).WithTooltip(RevelationGainVar.LocKey),
        new DynamicVar(EnergyNextTurnKey, 2)
    ];

    public PilgrimageRoadCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<NextTurnRevelationPower>(Owner.Creature, DynamicVars[RevelationGainKey].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<NextTurnEnergyPower>(Owner.Creature, DynamicVars[EnergyNextTurnKey].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[RevelationGainKey].UpgradeValueBy(2);
        DynamicVars[EnergyNextTurnKey].UpgradeValueBy(1);
    }
}
