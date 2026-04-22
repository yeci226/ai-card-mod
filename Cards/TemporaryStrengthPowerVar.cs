using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

public class TemporaryStrengthPowerVar : DynamicVar
{
    public const string Key = "AICARDMOD-TemporaryStrengthPower";
    public static readonly string LocKey = Key.ToUpperInvariant();

    public TemporaryStrengthPowerVar(decimal baseValue)
        : base(Key, baseValue)
    {
        this.WithTooltip(LocKey);
    }
}