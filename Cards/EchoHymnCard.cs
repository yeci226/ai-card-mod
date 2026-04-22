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
/// 名稱：迴聲聖歌
/// 描述：結算[gold]啟示[/gold]後，保留{AICARDMOD-RetainPercent:diff()}%的點數。
/// </summary>
public class EchoHymnCard : CustomCardModel
{
    private const string RetainPercentKey = "AICARDMOD-RetainPercent";
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(RetainPercentKey, 20)];

    public EchoHymnCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RevelationRetainPercentPower>(Owner.Creature, DynamicVars[RetainPercentKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }
}