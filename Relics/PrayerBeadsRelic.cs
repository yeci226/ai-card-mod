using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetRelicPool))]
public class PrayerBeadsRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override RelicModel? GetUpgradeReplacement() =>
        ModelDb.Relic<PrayerBeadsRelicUpgraded>();
}
