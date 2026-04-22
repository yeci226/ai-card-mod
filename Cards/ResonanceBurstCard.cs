using System;
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
/// 名稱：共鳴爆發
/// 描述：立即發射箭矢，且保留結算後 {AICARDMOD-RetainPercent:diff()}% 的[gold]啟示[/gold]點數。
/// </summary>
public class ResonanceBurstCard : PortraitCardModel
{
    private const string RetainPercentKey = "AICARDMOD-RetainPercent";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(RetainPercentKey, 25)];

    public ResonanceBurstCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int revelation = (int)(Owner.Creature.GetPower<RevelationPower>()?.Amount ?? 0m);
        if (revelation <= 0) return;

        var enemies = Owner.Creature.CombatState?.Enemies?.Where(e => e.IsAlive).ToList() ?? [];
        foreach (var enemy in enemies)
            await DamageCmd.Attack(revelation).FromCard(this).Targeting(enemy).Execute(choiceContext);

        int retainPercent = Math.Max(0, DynamicVars[RetainPercentKey].IntValue);
        int retain = (int)Math.Ceiling(revelation * (retainPercent / 100m));
        var delta = retain - revelation;
        if (delta != 0)
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, delta, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }
}
