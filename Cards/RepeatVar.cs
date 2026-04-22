using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

public class RepeatVar : DynamicVar
{
    public const string Key = "Repeat";
    public static readonly string LocKey = Key.ToUpperInvariant();

    public RepeatVar(decimal baseValue)
        : base(Key, baseValue)
    {
        this.WithTooltip(LocKey);
    }
}