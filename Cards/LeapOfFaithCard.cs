using BaseLib.Abstracts;
using BaseLib.Extensions;
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
/// 名稱：信仰之躍
/// 描述：失去所有[gold]格擋[/gold]。獲得{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]。
/// </summary>
public class LeapOfFaithCard : CustomCardModel
{
    private const string RevelationGainKey = RevelationGainVar.Key;
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(RevelationGainKey, 15).WithTooltip(RevelationGainVar.LocKey)];

    public LeapOfFaithCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var currentBlock = GetCurrentBlock(Owner.Creature);
        if (currentBlock > 0)
            await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(-currentBlock, ValueProp.Move), null);

        await PowerCmd.Apply<RevelationPower>(Owner.Creature, DynamicVars[RevelationGainKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[RevelationGainKey].UpgradeValueBy(5);
    }

    private static int GetCurrentBlock(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature)
    {
        var property = creature.GetType().GetProperty("Block");
        if (property?.GetValue(creature) is decimal block)
            return (int)block;
        return 0;
    }
}