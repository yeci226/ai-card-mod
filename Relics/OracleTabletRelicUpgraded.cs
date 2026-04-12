using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace AICardMod.Scripts;

/// <summary>
/// 神諭石板（升級版）
/// 每回合結束時補回 2 層虔誠，完全抵消衰退（淨衰退 0）。
/// </summary>
[Pool(typeof(ProphetRelicPool))]
public class OracleTabletRelicUpgraded : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;
}
