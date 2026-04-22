using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：迴響重擊
/// 描述：造成等同於目前[gold]啟示[/gold]點數總和的傷害。
/// </summary>
public class EchoSmiteCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    public EchoSmiteCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
            return;

        int revelation = (int)(Owner.Creature.Powers?.OfType<RevelationPower>().FirstOrDefault()?.Amount ?? 0m);
        if (revelation <= 0)
            return;

        await DamageCmd.Attack(revelation).FromCard(this).Targeting(target).Execute(choiceContext);
    }

    protected override void OnUpgrade() { }
}