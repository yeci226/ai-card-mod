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
/// 名稱：洗清罪惡
/// 描述：在本場戰鬥中，你每失去1點[gold]虔誠[/gold]，獲得{AICARDMOD-BlockPerFaithLoss:diff()}點[gold]格擋[/gold]。
/// </summary>
public class CleanseSinCard : CustomCardModel
{
    private const string BlockPerFaithLossKey = "AICARDMOD-BlockPerFaithLoss";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(BlockPerFaithLossKey, 1)];

    public CleanseSinCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<CleanseSinPower>(Owner.Creature, DynamicVars[BlockPerFaithLossKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BlockPerFaithLossKey].UpgradeValueBy(1);
    }
}