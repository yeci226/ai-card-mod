using Godot.Bridge;
using BaseLib.Config;
using MegaCrit.Sts2.Core.Modding;

namespace AICardMod.Scripts;

[ModInitializer("Init")]
public class Entry
{
    public static void Init()
    {
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
        ModConfigRegistry.Register("aiCardMod", new AiCardModConfig());
    }
}
