using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

public class FaithGainVar : DynamicVar
{
    public const string Key = "AICARDMOD-FaithGain";
    public static readonly string LocKey = Key.ToUpperInvariant();

    public FaithGainVar(decimal baseValue)
        : base(Key, baseValue)
    {
        this.WithTooltip(LocKey);
    }
}