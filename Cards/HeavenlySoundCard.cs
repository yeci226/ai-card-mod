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
/// 名稱：天音
/// 描述：對所有敵人造成{Damage:diff()}點傷害。每攻擊一個敵人累計{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]。
/// </summary>
public class HeavenlySoundCard : CustomCardModel
{
    private const string RevelationPerEnemyKey = RevelationGainVar.Key;
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar(RevelationPerEnemyKey, 3).WithTooltip(RevelationGainVar.LocKey)
    ];

    public HeavenlySoundCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = Owner.Creature.CombatState?.Enemies?.Where(enemy => enemy.IsAlive).ToList() ?? [];
        if (enemies.Count == 0)
            return;

        await CommonActions.CardAttack(this, cardPlay, 1).Execute(choiceContext);

        int revelationGain = enemies.Count * DynamicVars[RevelationPerEnemyKey].IntValue;
        if (revelationGain > 0)
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, revelationGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars[RevelationPerEnemyKey].UpgradeValueBy(1);
    }
}