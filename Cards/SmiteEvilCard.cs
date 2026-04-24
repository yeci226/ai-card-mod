using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：破邪
/// 描述：造成{Damage:diff()}點傷害。如果該敵人有[gold]失誤[/gold]狀態，則造成{Repeat:diff()}次傷害。
/// </summary>
public class SmiteEvilCard : CustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MisstepPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move)];

    public SmiteEvilCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await CommonActions.CardAttack(this, cardPlay, 1).Execute(choiceContext);
        if (cardPlay.Target.HasPower<MisstepPower>())
        {
            for (var i = 1; i < DynamicVars["Repeat"].IntValue; i++)
                await CommonActions.CardAttack(this, cardPlay, 1).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(4); }
}
