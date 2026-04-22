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
/// 名稱：聖力增幅
/// 描述：[gold]啟示[/gold]造成的傷害增加{Damage:diff()}點。
/// </summary>
public class HolyMightCard : CustomCardModel
{
    private const string BonusKey = DamageVarKey;
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInLibrary = true;

    private const string DamageVarKey = "Damage";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(BonusKey, 1)];

    public HolyMightCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HolyMightPower>(Owner.Creature, DynamicVars[BonusKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusKey].UpgradeValueBy(1);
    }
}