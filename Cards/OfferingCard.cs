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
/// 名稱：奉獻
/// 描述：獲得{AICARDMOD-EnergyGain:diff()}點能量。下回合開始時，少抽{Cards:diff()}張牌。
/// </summary>
public class OfferingCard : PortraitCardModel
{
    private const string EnergyGainKey = "AICARDMOD-EnergyGain";
    private const string DrawPenaltyKey = "DrawPenalty";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(EnergyGainKey, 2),
        new DynamicVar(DrawPenaltyKey, 1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [] : [CardKeyword.Exhaust];

    public OfferingCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars[EnergyGainKey].IntValue, Owner);
        await PowerCmd.Apply<NextTurnDrawPower>(Owner.Creature, -DynamicVars[DrawPenaltyKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }
}
