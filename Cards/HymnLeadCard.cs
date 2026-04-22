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
/// 名稱：聖歌引領
/// 描述：抽{AICARDMOD-HymnLeadDraw:diff()}張牌。累計{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]。
/// </summary>
public class HymnLeadCard : CustomCardModel
{
    private const string RevelationGainKey = RevelationGainVar.Key;
    private const string DrawKey = "AICARDMOD-HymnLeadDraw";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DynamicVar(RevelationGainKey, 2).WithTooltip(RevelationGainVar.LocKey),
        new DynamicVar(DrawKey, 1)
    ];

    public HymnLeadCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars[DrawKey].IntValue, Owner);
        await PowerCmd.Apply<RevelationPower>(Owner.Creature, DynamicVars[RevelationGainKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[RevelationGainKey].UpgradeValueBy(2);
    }
}
