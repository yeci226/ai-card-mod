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
/// 名稱：三位一體
/// 描述：抽{Cards:diff()}張牌。獲得{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]。獲得{AICARDMOD-FaithGain:diff()}點[gold]虔誠[/gold]。
/// </summary>
public class TrinityCard : CustomCardModel
{
    private const string RevelationGainKey = RevelationGainVar.Key;
    private const string FaithGainKey = FaithGainVar.Key;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new DynamicVar(RevelationGainKey, 3).WithTooltip(RevelationGainVar.LocKey),
        new DynamicVar(FaithGainKey, 3).WithTooltip(FaithGainVar.LocKey)
    ];

    public TrinityCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        await PowerCmd.Apply<RevelationPower>(Owner.Creature, DynamicVars[RevelationGainKey].IntValue, Owner.Creature, this);
        await FaithManager.ApplyFaith(Owner.Creature, DynamicVars[FaithGainKey].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.SetCustomBaseCost(0);
    }
}