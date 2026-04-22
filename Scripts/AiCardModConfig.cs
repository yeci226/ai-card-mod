using BaseLib.Config;

namespace AICardMod.Scripts;

[ConfigHoverTipsByDefault]
public sealed class AiCardModConfig : SimpleModConfig
{
    [ConfigSection("AICard")]
    public static bool UseAiMode { get; set; } = false;

    [ConfigSlider(0, 4, 1, Format = "{0:0}")]
    public static double PrefixIndex { get; set; } = 0;

    [ConfigSlider(0, 4, 1, Format = "{0:0}")]
    public static double MiddleIndex { get; set; } = 0;

    [ConfigSlider(0, 4, 1, Format = "{0:0}")]
    public static double SuffixIndex { get; set; } = 0;

    [ConfigSlider(0.5, 2.5, 0.25, Format = "{0:0.##}x")]
    public static double EffectScale { get; set; } = 1.0;

    [ConfigSlider(0, 20, 1, Format = "{0:0}")]
    public static double DefaultPietySpend { get; set; } = 0;

    public static bool GeneratedCardEthereal { get; set; } = false;

    [ConfigSection("AICard_AI")]
    [ConfigTextInput("https?://.+")]
    public static string AiApiUrl { get; set; } = "";

    [ConfigTextInput(".+")]
    public static string AiModel { get; set; } = "";

    public static double AiTimeoutSeconds { get; set; } = 20;
}
