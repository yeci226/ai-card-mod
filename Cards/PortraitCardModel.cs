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

    protected virtual string PortraitFolder => "res://images/packed/card_portraits/prophet";

    protected virtual string PortraitName => GetPortraitName();

    private string GetPortraitName()
    {
        var className = GetType().Name; // e.g. "AdmonitionCard"
        // Convert PascalCase to snake_case
        var snake = System.Text.RegularExpressions.Regex.Replace(className, "(?<!^)(?=[A-Z])", "_").ToLower();
        return $"aicardmod-{snake}";
    }

    public override string? CustomPortraitPath => $"{PortraitFolder}/{PortraitName}.png";

    public override string BetaPortraitPath => CustomPortraitPath ?? base.BetaPortraitPath;
}