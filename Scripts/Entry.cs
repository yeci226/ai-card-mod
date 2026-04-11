using Godot.Bridge;
using MegaCrit.Sts2.Core.Modding;

namespace AICardMod.Scripts;

[ModInitializer("Init")]
public class Entry
{
    public static void Init()
    {
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
    }
}
