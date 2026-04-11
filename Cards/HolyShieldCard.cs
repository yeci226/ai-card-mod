using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

/// <summary>
/// 聖盾 — 根據當前虔誠值獲得格擋。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class HolyShieldCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _baseBlock = 4;

    public HolyShieldCard() : base(2, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        int total = _baseBlock + piety;

        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(total, 0), null);
    }

    protected override void OnUpgrade()
    {
        _baseBlock = 7;
    }
}
