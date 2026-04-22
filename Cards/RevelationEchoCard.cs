using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：啟示迴響
/// 描述：每當[gold]啟示[/gold]觸發時，獲得{Block:diff()}點[gold]格擋[/gold]。
/// </summary>
public class RevelationEchoCard : CustomCardModel
{
    private const string BlockKey = BlockVarKey;
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInLibrary = true;

    private const string BlockVarKey = "Block";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<RevelationPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(BlockKey, 2)];

    public RevelationEchoCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RevelationEchoPower>(Owner.Creature, DynamicVars[BlockKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BlockKey].UpgradeValueBy(1);
    }
}