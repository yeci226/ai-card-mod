using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AICardMod.Scripts;

public static class CombatHelper
{
    public static async Task ForEachAliveEnemy(Creature source, Func<Creature, Task> action)
    {
        var enemies = source.CombatState?.Enemies?.Where(e => e.IsAlive).ToList();
        if (enemies == null || enemies.Count == 0) return;

        foreach (var enemy in enemies)
            await action(enemy);
    }
}
