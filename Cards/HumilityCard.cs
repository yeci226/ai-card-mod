using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：謙卑
/// 描述：丟棄{Cards:diff()}張手牌。獲得{AICARDMOD-FaithGain:diff()}點[gold]虔誠[/gold]。
/// </summary>
public class HumilityCard : CustomCardModel
{
    private const string FaithGainKey = FaithGainVar.Key;
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DynamicVar(FaithGainKey, 2).WithTooltip(FaithGainVar.LocKey)
    ];

    public HumilityCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var prompt = new LocString("cards", "AICARDMOD-SELECT_PROMPT");
        var card = await CommonActions.SelectSingleCard(this, prompt, choiceContext, PileType.Hand);
        if (card != null)
            await CardCmd.Discard(choiceContext, card);

        await FaithManager.ApplyFaith(Owner.Creature, DynamicVars[FaithGainKey].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[FaithGainKey].UpgradeValueBy(1);
    }
}
