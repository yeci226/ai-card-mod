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
/// 名稱：盲目追隨
/// 描述：獲得{AICARDMOD-MisstepGain:diff()}層[gold]失誤[/gold]。獲得{AICARDMOD-FaithGain:diff()}點[gold]虔誠[/gold]。
/// </summary>
public class BlindFollowingCard : CustomCardModel
{
    private const string MisstepGainKey = MisstepGainVar.Key;
    private const string FaithGainKey = FaithGainVar.Key;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(MisstepGainKey, 1).WithTooltip(MisstepGainVar.LocKey),
        new DynamicVar(FaithGainKey, 2).WithTooltip(FaithGainVar.LocKey)
    ];

    public BlindFollowingCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<MisstepPower>(Owner.Creature, DynamicVars[MisstepGainKey].IntValue, Owner.Creature, this);
        await FaithManager.ApplyFaith(Owner.Creature, DynamicVars[FaithGainKey].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[FaithGainKey].UpgradeValueBy(1);
    }
}