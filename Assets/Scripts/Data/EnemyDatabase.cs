using UnityEngine;

namespace Ascension
{
    public static class EnemyDatabase
    {
        static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

        // ---------- Biome 1: Ashlands of Hell ----------
        public static readonly EnemyDef Skeleton = new EnemyDef { name = "Charred Skeleton", behaviour = EnemyBehaviour.Chaser, hp = 40, speed = 3.4f, damage = 8, color = C("#d9d2c0"), gold = 3 };
        public static readonly EnemyDef EmberImp = new EnemyDef { name = "Ember Imp", behaviour = EnemyBehaviour.Shooter, hp = 26, speed = 2.8f, damage = 7, attackRange = 7f, attackCooldown = 2.0f, projectileSpeed = 7f, color = C("#ff6a3d"), gold = 4, radius = 0.38f };
        public static readonly EnemyDef MagmaBrute = new EnemyDef { name = "Magma Brute", behaviour = EnemyBehaviour.Charger, hp = 95, speed = 2.6f, damage = 16, attackRange = 1.5f, attackCooldown = 2.6f, armor = 0.25f, color = C("#8a1c0c"), gold = 7, radius = 0.6f, tier = 1 };

        // ---------- Biome 2: Haunted Kingdoms of Earth ----------
        public static readonly EnemyDef Militiaman = new EnemyDef { name = "Corrupt Militiaman", behaviour = EnemyBehaviour.Chaser, hp = 65, speed = 3.2f, damage = 11, color = C("#6b6f4a"), gold = 4 };
        public static readonly EnemyDef Specter = new EnemyDef { name = "Wailing Specter", behaviour = EnemyBehaviour.Phaser, hp = 45, speed = 3.6f, damage = 10, color = C("#9ab4ff"), gold = 5, tier = 1 };
        public static readonly EnemyDef CursedArcher = new EnemyDef { name = "Cursed Archer", behaviour = EnemyBehaviour.Shooter, hp = 50, speed = 2.9f, damage = 10, attackRange = 8f, attackCooldown = 1.7f, projectileSpeed = 10f, color = C("#7a4aa8"), gold = 5, radius = 0.4f };
        public static readonly EnemyDef RuinKnight = new EnemyDef { name = "Ruin Knight", behaviour = EnemyBehaviour.Charger, hp = 130, speed = 2.8f, damage = 20, attackRange = 1.6f, attackCooldown = 2.4f, armor = 0.35f, color = C("#555a66"), gold = 9, radius = 0.62f, tier = 1 };

        // ---------- Biome 3: Astral Gates of Heaven ----------
        public static readonly EnemyDef Zealot = new EnemyDef { name = "Zealot", behaviour = EnemyBehaviour.Chaser, hp = 85, speed = 4.2f, damage = 13, color = C("#f4e7b0"), gold = 6 };
        public static readonly EnemyDef SeraphTurret = new EnemyDef { name = "Seraph Construct", behaviour = EnemyBehaviour.Turret, hp = 110, speed = 0f, damage = 12, attackRange = 14f, attackCooldown = 2.2f, projectileSpeed = 9f, armor = 0.3f, color = C("#fff3c4"), gold = 8, radius = 0.55f, tier = 1 };
        public static readonly EnemyDef HaloLancer = new EnemyDef { name = "Halo Lancer", behaviour = EnemyBehaviour.Shooter, hp = 75, speed = 3.2f, damage = 14, attackRange = 9f, attackCooldown = 1.5f, projectileSpeed = 12f, color = C("#ffe27a"), gold = 7, radius = 0.4f };
        public static readonly EnemyDef GildedSentinel = new EnemyDef { name = "Gilded Sentinel", behaviour = EnemyBehaviour.Charger, hp = 170, speed = 3f, damage = 24, attackRange = 1.7f, attackCooldown = 2.2f, armor = 0.5f, color = C("#e6b800"), gold = 12, radius = 0.68f, tier = 1 };

        public static readonly BossDef Inquisitor = new BossDef
        {
            name = "Grand Inquisitor of the Flame", title = "Biome 1 Boss", behaviour = EnemyBehaviour.Chaser, hp = 650, speed = 2.2f, damage = 14, color = C("#ff4d1a"), radius = 1.1f, gold = 0, projectileSpeed = 8f,
            addPool = new[] { Skeleton, EmberImp },
            phases = new[]
            {
                new BossPhase { enterAtHpFraction = 1.0f, interval = 2.6f, patterns = new[] { BossPattern.AimedVolley, BossPattern.RadialBurst } },
                new BossPhase { enterAtHpFraction = 0.6f, interval = 2.2f, shieldAdds = 3, patterns = new[] { BossPattern.Charge, BossPattern.RadialBurst, BossPattern.AimedVolley } },
                new BossPhase { enterAtHpFraction = 0.3f, interval = 1.8f, shieldAdds = 4, patterns = new[] { BossPattern.Spiral, BossPattern.Charge, BossPattern.SummonAdds } },
            }
        };

        public static readonly BossDef Warlord = new BossDef
        {
            name = "Phantom Warlord", title = "Biome 2 Boss", behaviour = EnemyBehaviour.Chaser, hp = 950, speed = 2.6f, damage = 18, color = C("#8ec5ff"), radius = 1.1f, gold = 0, projectileSpeed = 9f,
            addPool = new[] { Militiaman, Specter },
            phases = new[]
            {
                new BossPhase { enterAtHpFraction = 1.0f, interval = 2.3f, patterns = new[] { BossPattern.Charge, BossPattern.AimedVolley } },
                new BossPhase { enterAtHpFraction = 0.65f, interval = 2.0f, shieldAdds = 4, patterns = new[] { BossPattern.RadialBurst, BossPattern.Charge, BossPattern.SummonAdds } },
                new BossPhase { enterAtHpFraction = 0.3f, interval = 1.6f, shieldAdds = 4, patterns = new[] { BossPattern.Spiral, BossPattern.Charge, BossPattern.AimedVolley } },
            }
        };

        public static readonly BossDef Herald = new BossDef
        {
            name = "Archangel Herald", title = "Biome 3 Boss", behaviour = EnemyBehaviour.Chaser, hp = 1350, speed = 2.8f, damage = 22, color = C("#fff0a0"), radius = 1.2f, gold = 0, projectileSpeed = 11f,
            addPool = new[] { Zealot, HaloLancer },
            phases = new[]
            {
                new BossPhase { enterAtHpFraction = 1.0f, interval = 2.0f, patterns = new[] { BossPattern.AimedVolley, BossPattern.Spiral } },
                new BossPhase { enterAtHpFraction = 0.65f, interval = 1.8f, shieldAdds = 4, patterns = new[] { BossPattern.RadialBurst, BossPattern.Charge, BossPattern.AimedVolley } },
                new BossPhase { enterAtHpFraction = 0.35f, interval = 1.5f, shieldAdds = 5, patterns = new[] { BossPattern.Spiral, BossPattern.RadialBurst, BossPattern.SummonAdds, BossPattern.Charge } },
            }
        };

        public static readonly BossDef Sovereign = new BossDef
        {
            name = "The Sovereign of Judgment", title = "Final Boss", behaviour = EnemyBehaviour.Chaser, hp = 2300, speed = 2.4f, damage = 26, color = C("#ffffff"), radius = 1.35f, gold = 0, projectileSpeed = 11f,
            addPool = new[] { Zealot, HaloLancer, GildedSentinel },
            phases = new[]
            {
                new BossPhase { enterAtHpFraction = 1.0f, interval = 1.9f, patterns = new[] { BossPattern.AimedVolley, BossPattern.RadialBurst } },
                new BossPhase { enterAtHpFraction = 0.75f, interval = 1.7f, shieldAdds = 4, patterns = new[] { BossPattern.Spiral, BossPattern.Charge } },
                new BossPhase { enterAtHpFraction = 0.5f, interval = 1.5f, shieldAdds = 5, patterns = new[] { BossPattern.RadialBurst, BossPattern.Spiral, BossPattern.SummonAdds } },
                new BossPhase { enterAtHpFraction = 0.25f, interval = 1.2f, shieldAdds = 6, patterns = new[] { BossPattern.Spiral, BossPattern.Charge, BossPattern.RadialBurst, BossPattern.AimedVolley } },
            }
        };
    }

    public class BiomeDef
    {
        public string name, subtitle;
        public Color floor, wall, accent;
        public int chambers;               // regular rooms before the boss
        public EnemyDef[] pool;
        public BossDef boss;
        public int pillars;
        public int hazards;                // magma pools / laser beams
        public bool lasers;                // hazards are timed laser beams instead of magma
    }

    public static class BiomeDatabase
    {
        static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

        public static readonly BiomeDef[] All =
        {
            new BiomeDef { name = "The Ashlands of Hell", subtitle = "Skeletons, demons, magma.", floor = C("#2b1410"), wall = C("#120706"), accent = C("#ff5a1f"), chambers = 4,
                pool = new[] { EnemyDatabase.Skeleton, EnemyDatabase.EmberImp, EnemyDatabase.MagmaBrute }, boss = EnemyDatabase.Inquisitor, pillars = 4, hazards = 4 },
            new BiomeDef { name = "The Haunted Kingdoms of Earth", subtitle = "Corrupt mortals, specters, castle ruins.", floor = C("#1d2530"), wall = C("#0b0f14"), accent = C("#8ec5ff"), chambers = 4,
                pool = new[] { EnemyDatabase.Militiaman, EnemyDatabase.Specter, EnemyDatabase.CursedArcher, EnemyDatabase.RuinKnight }, boss = EnemyDatabase.Warlord, pillars = 8, hazards = 0 },
            new BiomeDef { name = "The Astral Gates of Heaven", subtitle = "Angelic constructs, laser matrices, zealots.", floor = C("#d9d4c3"), wall = C("#8d8870"), accent = C("#ffe27a"), chambers = 4,
                pool = new[] { EnemyDatabase.Zealot, EnemyDatabase.SeraphTurret, EnemyDatabase.HaloLancer, EnemyDatabase.GildedSentinel }, boss = EnemyDatabase.Herald, pillars = 5, hazards = 3, lasers = true },
            new BiomeDef { name = "The Apex Throne", subtitle = "Judgment is absolute.", floor = C("#f4f1e6"), wall = C("#b8b39a"), accent = C("#ffffff"), chambers = 1,
                pool = new[] { EnemyDatabase.Zealot, EnemyDatabase.HaloLancer }, boss = EnemyDatabase.Sovereign, pillars = 4, hazards = 4, lasers = true },
        };
    }
}
