using BaseLib.Config;

namespace AICardMod.Scripts;

[ConfigHoverTipsByDefault]
public sealed class AiCardModConfig : SimpleModConfig
{
    [ConfigSection("AICard")]
    [ConfigTextInput("https?://.+")]
    public static string AiApiUrl { get; set; } = "";

    [ConfigTextInput(".+")]
    public static string AiModel { get; set; } = "";

    public static double AiTimeoutSeconds { get; set; } = 20;
}
