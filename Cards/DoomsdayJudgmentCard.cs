using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetCardPool))]
/// <summary>
/// 名稱：末日審判
/// 描述：每當[gold]啟示[/gold]在本回合觸發時，本回合後續[gold]啟示[/gold]傷害增加{Damage:diff()}點。
/// </summary>
public class DoomsdayJudgmentCard : CustomCardModel
{
    private const string BonusKey = DamageVarKey;
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInLibrary = true;
    private const string DamageVarKey = "Damage";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RevelationPower>()];


    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(BonusKey, 1)];

    public DoomsdayJudgmentCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DoomsdayJudgmentPower>(Owner.Creature, DynamicVars[BonusKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusKey].UpgradeValueBy(1);
    }
}