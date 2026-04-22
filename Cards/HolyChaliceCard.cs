using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：聖杯
/// 描述：每當你累計[gold]啟示[/gold]時，額外累計{AICARDMOD-RevelationGain:diff()}點。
/// </summary>
public class HolyChaliceCard : CustomCardModel
{
    private const string BonusGainKey = RevelationGainVar.Key;
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Retain] : [];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(BonusGainKey, 1).WithTooltip(RevelationGainVar.LocKey)];

    public HolyChaliceCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HolyChalicePower>(Owner.Creature, DynamicVars[BonusGainKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }
}