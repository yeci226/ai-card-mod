using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：懺悔室
/// 描述：獲得{Block:diff()}點[gold]格擋[/gold]。消滅所有狀態牌，每消滅一張狀態牌抽{Cards:diff()}張牌，累計{AICARDMOD-RevelationGain:diff()}點[gold]啟示[/gold]。
/// </summary>
public class ConfessionalCard : CustomCardModel
{
    private const string RevelationGainKey = RevelationGainVar.Key;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6, ValueProp.Move),
        new CardsVar(1),
        new DynamicVar(RevelationGainKey, 1).WithTooltip(RevelationGainVar.LocKey)
    ];

    public ConfessionalCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);

        var statusCards = CardPile.GetCards(Owner, new[] { PileType.Hand })
            .Where(c => c.Type == CardType.Status)
            .ToList();

        foreach (var card in statusCards)
        {
            await CardCmd.Exhaust(choiceContext, card, false, false);
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
            await PowerCmd.Apply<RevelationPower>(Owner.Creature, DynamicVars[RevelationGainKey].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars[RevelationGainKey].UpgradeValueBy(1);
    }
}
