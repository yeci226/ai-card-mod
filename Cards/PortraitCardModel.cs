using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace AICardMod.Scripts;

/// <summary>
/// 名稱：PortraitCardModel
/// 描述：TODO: 補上在地化描述
/// </summary>
public abstract class PortraitCardModel : CustomCardModel
{
    protected PortraitCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInLibrary)
    {
    }

    protected virtual string PortraitFolder => "res://aiCardMod/images/cards";

    protected virtual string PortraitName => GetType().Name;

    public override string PortraitPath => $"{PortraitFolder}/{PortraitName}.png";

    public override string BetaPortraitPath => PortraitPath;
}