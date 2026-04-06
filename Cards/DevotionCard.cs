using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

/// <summary>
/// 信仰 — 獲得信仰能力，每回合開始時自動累積虔誠。
/// </summary>
[Pool(typeof(ColorlessCardPool))]
public class DevotionCard : CustomCardModel
{
    private const int EnergyCost = 1;
    private const CardType Type = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType Target = TargetType.None;
    private const bool ShowInLibrary = true;

    private int _devotionGain = 1;

    public DevotionCard() : base(EnergyCost, Type, Rarity, Target, ShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DevotionPower>(Owner.Creature, _devotionGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _devotionGain = 2;
    }
}
