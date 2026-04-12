using MegaCrit.Sts2.Core.Entities.Creatures;

namespace AICardMod.Scripts;

public static class ProphetSynergy
{
    public static int HolyBonus(Creature creature) =>
        (int)(creature.Powers?.OfType<HolyResonancePower>().FirstOrDefault()?.Amount ?? 0m);
}
