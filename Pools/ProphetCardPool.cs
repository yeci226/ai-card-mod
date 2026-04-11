using BaseLib.Abstracts;
using Godot;

namespace AICardMod.Scripts;

public class ProphetCardPool : CustomCardPoolModel
{
    // 卡池的唯一 ID
    public override string Title => "prophet";

    // 卡牌描述中使用的能量小圖示（24x24）
    public override string? TextEnergyIconPath => "res://images/energy_prophet.png";

    // tooltip 和卡牌左上角的能量大圖示（74x74）
    public override string? BigEnergyIconPath => "res://images/energy_prophet_big.png";

    // 牌組畫面中的卡牌主題色（金色）
    public override Color DeckEntryCardColor => new(0.85f, 0.65f, 0.1f);

    // 卡框著色（金色）
    public override Color ShaderColor => new(0.85f, 0.65f, 0.1f);

    // 非無色卡池
    public override bool IsColorless => false;
}
