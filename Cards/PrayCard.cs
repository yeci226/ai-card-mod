using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace AICardMod.Scripts;

/// <summary>
/// 祈禱 — 賦予虔誠。虔誠決定神諭的效果品質。
/// </summary>
[Pool(typeof(ColorlessCardPool))]
public class PrayCard : CustomCardModel
{
    private const int EnergyCost = 1;
    private const CardType Type = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Common;
    private const TargetType Target = TargetType.None;
    private const bool ShowInLibrary = true;

    private int _pietyGain = 3;

    public PrayCard() : base(EnergyCost, Type, Rarity, Target, ShowInLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PietyPower>(Owner.Creature, _pietyGain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _pietyGain = 5;
    }
}
