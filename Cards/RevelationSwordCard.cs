using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：啟示之劍
/// 描述：造成{Damage:diff()}點傷害。獲得等量於所造成傷害的[gold]啟示[/gold]。
/// </summary>
public class RevelationSwordCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<RevelationPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move)];

    public RevelationSwordCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        int before = GetDurability(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.IntValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        int after = GetDurability(cardPlay.Target);

        int damageDealt = Math.Max(0, before - after);
        if (damageDealt > 0)
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, damageDealt, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    private static int GetDurability(Creature creature)
    {
        int hp = creature.CurrentHp;
        int block = GetBlock(creature);
        return hp + block;
    }

    private static int GetBlock(Creature creature)
    {
        var property = creature.GetType().GetProperty("Block");
        if (property?.GetValue(creature) is decimal block)
            return (int)block;
        return 0;
    }
}
