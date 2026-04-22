using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：信仰之盾
/// 描述：獲得{Block:diff()}點[gold]格擋[/gold]。每擁有{AICARDMOD-FaithUnit:diff()}點[gold]虔誠[/gold]，格擋增加{AICARDMOD-BlockPerFaith:diff()}點。
/// </summary>
public class FaithShieldCard : CustomCardModel
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    private const string BlockPerFaithKey = "AICARDMOD-BlockPerFaith";
    private const string FaithUnitKey = "AICARDMOD-FaithUnit";
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12, ValueProp.Move),
        new DynamicVar(BlockPerFaithKey, 3),
        new DynamicVar(FaithUnitKey, 1)
    ];

    public FaithShieldCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int faith = (int)(Owner.Creature.Powers?.OfType<FaithPower>().FirstOrDefault()?.Amount ?? 0m);
        int faithUnit = Math.Max(1, DynamicVars[FaithUnitKey].IntValue);
        int totalBlock = DynamicVars.Block.IntValue + (faith / faithUnit) * DynamicVars[BlockPerFaithKey].IntValue;
        await CreatureCmd.GainBlock(Owner.Creature, totalBlock, ValueProp.Move, null);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars[BlockPerFaithKey].UpgradeValueBy(1);
    }
}