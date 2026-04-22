using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace AICardMod.Scripts;

public class HpLossVar : DynamicVar
{
    public const string Key = "AICARDMOD-HpLoss";
    public static readonly string LocKey = Key.ToUpperInvariant();

    public HpLossVar(decimal baseValue)
        : base(Key, baseValue)
    {
        this.WithTooltip(LocKey);
    }
}