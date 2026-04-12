using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace AICardMod.Scripts;

[Pool(typeof(ProphetRelicPool))]
public class PrayerBeadsRelicUpgraded : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;
}
