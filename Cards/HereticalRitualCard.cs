using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：異教儀式
/// 描述：失去所有[gold]虔誠[/gold]。每{AICARDMOD-FaithPerEnergy:diff()}點[gold]虔誠[/gold]獲得{AICARDMOD-EnergyPerThreshold:diff()}點能量。
/// </summary>
public class HereticalRitualCard : CustomCardModel
{
    private const string FaithPerEnergyKey = "AICARDMOD-FaithPerEnergy";
    private const string EnergyPerThresholdKey = "AICARDMOD-EnergyPerThreshold";
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [] : [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(FaithPerEnergyKey, 3),
        new DynamicVar(EnergyPerThresholdKey, 1)
    ];

    public HereticalRitualCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int faith = (int)(Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault()?.Amount ?? 0m);
        if (faith <= 0)
            return;

        await PowerCmd.Apply<FaithPower>(Owner.Creature, -faith, Owner.Creature, this);

        int faithPerEnergy = Math.Max(1, DynamicVars[FaithPerEnergyKey].IntValue);
        int energyPerThreshold = DynamicVars[EnergyPerThresholdKey].IntValue;
        int energy = (faith / faithPerEnergy) * energyPerThreshold;
        if (energy > 0)
            await PlayerCmd.GainEnergy(energy, Owner);
    }

    protected override void OnUpgrade() { }
}