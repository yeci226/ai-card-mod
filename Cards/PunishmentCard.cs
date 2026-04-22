using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：責罰
/// 描述：如果敵方擁有[gold]失誤[/gold]，則給予{AICARDMOD-MisstepGain:diff()}層[gold]失誤[/gold]。
/// </summary>
public class PunishmentCard : CustomCardModel
{
    private const string MisstepGainKey = MisstepGainVar.Key;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(MisstepGainKey, 2).WithTooltip(MisstepGainVar.LocKey)];

    public PunishmentCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
            return;

        if (!target.HasPower<MisstepPower>())
            return;

        await PowerCmd.Apply<MisstepPower>(target, DynamicVars[MisstepGainKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[MisstepGainKey].UpgradeValueBy(1);
    }
}