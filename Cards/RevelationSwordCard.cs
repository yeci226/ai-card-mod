using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
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

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move)];

    public RevelationSwordCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        int durabilityBefore = GetDurability(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.IntValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        int durabilityAfter = GetDurability(cardPlay.Target);

        int damageDealt = Math.Max(0, durabilityBefore - durabilityAfter);
        if (damageDealt > 0)
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, damageDealt, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    private static int GetDurability(Creature creature)
    {
        int hp = GetFirstAvailableIntProperty(creature, "Hp", "CurrentHp", "Health");
        int block = GetFirstAvailableIntProperty(creature, "Block");
        return hp + block;
    }

    private static int GetFirstAvailableIntProperty(object instance, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (TryGetIntProperty(instance, propertyName, out var value))
                return value;
        }

        return 0;
    }

    private static bool TryGetIntProperty(object instance, string propertyName, out int value)
    {
        var property = instance.GetType().GetProperty(propertyName);
        if (property == null)
        {
            value = 0;
            return false;
        }

        var rawValue = property.GetValue(instance);
        if (rawValue is decimal decimalValue)
        {
            value = (int)decimalValue;
            return true;
        }

        if (rawValue is int intValue)
        {
            value = intValue;
            return true;
        }

        if (rawValue is long longValue)
        {
            value = (int)longValue;
            return true;
        }

        value = 0;
        return false;
    }
}
