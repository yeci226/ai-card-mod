using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace AICardMod.Scripts;

/// <summary>
/// ?戊?????????箇嚗?/// 瘥?圈洛??嚗敺?6 靽∩趕嚗孛?澆?蝷箸炎?乓?/// 瘥活?內憿???1 撘萇?嚗? GoldenScriptureEffectPower 撖虫?嚗?/// </summary>
[Pool(typeof(ProphetRelicPool))]
public class OracleTabletRelicUpgraded : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeCombatStart()
    {
        if (Owner?.Creature == null) return;

        await PowerCmd.Apply<FaithPower>(Owner.Creature, 6, Owner.Creature, null);
    }
}

