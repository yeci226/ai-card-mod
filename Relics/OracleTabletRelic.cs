using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace AICardMod.Scripts;

/// <summary>
/// 神諭石板（起始遺物）
/// 每回合結束時，虔誠只衰退一半（最少 1）而非全部。
/// 升級後由 OracleTabletRelicUpgraded 取代（只衰退 25%，最少 1）。
/// </summary>
[Pool(typeof(ProphetRelicPool))]
public class OracleTabletRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicModel? GetUpgradeReplacement() =>
        ModelDb.Relic<OracleTabletRelicUpgraded>();
}
