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
/// 名稱：靈光顯現
/// 描述：獲得等同於你的【虔誠】層數的【啟示】。
/// 升級：費用改為0。
/// </summary>
public class DivineManifestationCard : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FaithPower>(),
        HoverTipFactory.FromPower<RevelationPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public DivineManifestationCard() : base(energyCost, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int faith = (int)(Owner.Creature.GetPower<FaithPower>()?.Amount ?? 0m);
        if (faith <= 0) return;

        await PowerCmd.Apply<RevelationPower>(Owner.Creature, faith, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.SetCustomBaseCost(0);
    }
}
