using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace AICardMod.Scripts;

/// <summary>
/// 破損的經文（初始遺物）
/// 每場戰鬥開始：獲得 3 信仰，觸發啟示檢查。
/// 升級後由 OracleTabletRelicUpgraded（聖女的黃金經文）取代。
/// </summary>
[Pool(typeof(ProphetRelicPool))]
public class OracleTabletRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicModel? GetUpgradeReplacement() =>
        ModelDb.Relic<OracleTabletRelicUpgraded>();

    public override async Task BeforeCombatStart()
    {
        if (Owner?.Creature == null) return;
        await PowerCmd.Apply<FaithPower>(Owner.Creature, 3, Owner.Creature, null);
    }
}
