using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// 神聖儀式 — 將當前虔誠翻倍（最多 12 層）。
/// </summary>
[Pool(typeof(ProphetCardPool))]
public class HolyRitualCard : CustomCardModel
{
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInLibrary = true;

    private int _cap = 12;

    public HolyRitualCard() : base(2, type, rarity, targetType, shouldShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int piety = (int)(Owner.Creature.Powers?.OfType<PietyPower>().FirstOrDefault()?.Amount ?? 0m);
        if (piety <= 0) return;

        int gain = Math.Min(piety, _cap - piety);
        if (gain > 0)
            await PowerCmd.Apply<PietyPower>(Owner.Creature, gain, Owner.Creature, this);
    }

    protected override void OnUpgrade() => _cap = 20;
}
