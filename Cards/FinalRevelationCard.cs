using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：最後啟示
/// 描述：將現有[gold]啟示[/gold]翻倍。你接下來無法再獲得[gold]啟示[/gold]。
/// </summary>
public class FinalRevelationCard : CustomCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RevelationPower>()];

    public FinalRevelationCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int revelation = (int)(Owner.Creature.GetPower<RevelationPower>()?.Amount ?? 0m);
        if (revelation > 0)
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, revelation, Owner.Creature, this);

        await PowerCmd.Apply<FinalRevelationLockPower>(Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.SetCustomBaseCost(2);
    }
}