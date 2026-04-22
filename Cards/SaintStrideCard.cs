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
/// 名稱：聖徒步伐
/// 描述：本回合每打出一張牌，獲得{AICARDMOD-RevelationPerCard:diff()}點[gold]啟示[/gold]。
/// </summary>
public class SaintStrideCard : CustomCardModel
{
    private const string RevelationPerCardKey = "AICARDMOD-RevelationPerCard";
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(RevelationPerCardKey, 1)];

    public SaintStrideCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RevelationPerCardThisTurnPower>(Owner.Creature, DynamicVars[RevelationPerCardKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }
}