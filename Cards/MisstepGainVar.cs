using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

public class MisstepGainVar : DynamicVar
{
    public const string Key = "AICARDMOD-MisstepGain";
    public static readonly string LocKey = Key.ToUpperInvariant();
    public MisstepGainVar(decimal baseValue)
        : base(Key, baseValue)
    {
        this.WithTooltip(LocKey);
    }
}