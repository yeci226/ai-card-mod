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
/// 名稱：盲目輻射
/// 描述：對所有敵人造成{Damage:diff()}點傷害，並施加{AICARDMOD-MisstepGain:diff()}層[gold]失誤[/gold]。
/// </summary>
public class BlindRadianceCard : CustomCardModel
{
    private const string MisstepGainKey = MisstepGainVar.Key;
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13, ValueProp.Move),
        new DynamicVar(MisstepGainKey, 1).WithTooltip(MisstepGainVar.LocKey)
    ];

    public BlindRadianceCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);

        var enemies = Owner.Creature.CombatState?.Enemies?.Where(enemy => enemy.IsAlive).ToList() ?? [];
        foreach (var enemy in enemies)
            await PowerCmd.Apply<MisstepPower>(enemy, DynamicVars[MisstepGainKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars[MisstepGainKey].UpgradeValueBy(1);
    }
}