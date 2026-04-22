using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

public class RevelationGainVar : DynamicVar
{
    public const string Key = "AICARDMOD-RevelationGain";
    public static readonly string LocKey = Key.ToUpperInvariant();
    public RevelationGainVar(decimal baseValue)
        : base(Key, baseValue)
    {
        this.WithTooltip(LocKey);
    }
}