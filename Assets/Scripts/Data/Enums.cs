using System;

namespace Ascension
{
    public enum ConstellationId { BoneThrone, EndlessHorizon, PrimordialFlame, UnyieldingAegis, VoidExecutioner, FatalParadox, BloodlineScribe, AbyssalHorror, ClockworkFate, Procrastinator, MoneyThrower, TropeCritic, CosmicChef, ChatMod, MuscleBrain }

    public enum SponsorCategory { Grim, Cosmic, Meta }

    [Flags]
    public enum StatusType
    {
        None = 0,
        // Cosmic ailments: any TWO distinct ones on an enemy = Star-Bursted State.
        GraveBound = 1 << 0,
        StellarBurn = 1 << 1,
        Bleed = 1 << 2,
        Ignite = 1 << 3,
        Corroded = 1 << 4,
        Crushed = 1 << 5,
        // Mundane control effects (do not count toward Star-Burst).
        Stun = 1 << 6,
        Slow = 1 << 7,
        CosmicMask = GraveBound | StellarBurn | Bleed | Ignite | Corroded | Crushed
    }

    public enum RoomType { Combat, Elite, Sanctuary, Boss, Transition }
    public enum RewardType { None, Blessing, Gold, Stardust, Vial, Vitality }
    public enum EnemyBehaviour { Chaser, Shooter, Charger, Phaser, Turret }
}
