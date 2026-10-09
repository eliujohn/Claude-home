using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>
    /// Source of truth for the 15 sponsors. Any ConstellationSponsor assets under Resources/Constellations
    /// override the built-in defaults (designers can tune data without touching code).
    /// </summary>
    public static class ConstellationDatabase
    {
        static Dictionary<ConstellationId, ConstellationSponsor> map;

        public static IReadOnlyCollection<ConstellationSponsor> All { get { Ensure(); return map.Values; } }

        public static ConstellationSponsor Get(ConstellationId id) { Ensure(); return map[id]; }

        static void Ensure()
        {
            if (map != null) return;
            map = new Dictionary<ConstellationId, ConstellationSponsor>();
            foreach (var d in Defaults()) map[d.id] = d;
            foreach (var asset in Resources.LoadAll<ConstellationSponsor>("Constellations")) map[asset.id] = asset;
        }

        static ConstellationSponsor Make(ConstellationId id, SponsorCategory cat, string name, string title, string hex, string desc, float dmg = 1f, float spd = 1f, bool alters = false)
        {
            var s = ScriptableObject.CreateInstance<ConstellationSponsor>();
            s.id = id; s.category = cat; s.sponsorName = name; s.sponsorTitle = title;
            ColorUtility.TryParseHtmlString(hex, out s.constellationColor);
            s.blessingDescription = desc; s.damageMod = dmg; s.speedMod = spd; s.altersAttackBehavior = alters;
            s.hideFlags = HideFlags.HideAndDontSave;
            return s;
        }

        static IEnumerable<ConstellationSponsor> Defaults()
        {
            const SponsorCategory G = SponsorCategory.Grim, C = SponsorCategory.Cosmic, M = SponsorCategory.Meta;
            yield return Make(ConstellationId.BoneThrone, G, "The Monarch of the Bone Throne", "The Necromancer", "#9fd8c8",
                "Attacks apply Grave Bound. Slain enemies rise as Spectral Thralls that fight for you.", alters: true);
            yield return Make(ConstellationId.FatalParadox, G, "The Weaver of Fatal Paradoxes", "The Trickster", "#c06bff",
                "Damage and speed grow as your health falls. Dashing no longer grants invulnerability — instead, incoming hits may simply never have happened.");
            yield return Make(ConstellationId.BloodlineScribe, G, "The Scribe of Bloodlines", "The Vampire", "#d4213d",
                "Healing items become stacking damage frenzies. Strikes cause Bleed and leech life.");
            yield return Make(ConstellationId.VoidExecutioner, G, "The Executioner of the Quiet Void", "The Assassin", "#5a5f7a",
                "Dashing grants full invisibility. Strikes from behind deal 500% critical damage.", spd: 1.05f);
            yield return Make(ConstellationId.EndlessHorizon, C, "The Sovereign of the Endless Horizon", "The Cosmos Seeker", "#4fc3ff",
                "True sight and full map reveal. Ranged shots lock on and bend around walls, applying Stellar Burn.");
            yield return Make(ConstellationId.PrimordialFlame, C, "The Primordial Flame of Genesis", "The Star Core", "#ffb347",
                "Miniature suns orbit you, dissolving armor (Corroded) and swallowing enemy projectiles.");
            yield return Make(ConstellationId.UnyieldingAegis, C, "The Unyielding Aegis of the Zenith", "The Bastion", "#e8e2c0",
                "Reduces and stores incoming damage; when full (or on [Q]) it detonates a screen-wide stunning gravity shockwave.");
            yield return Make(ConstellationId.AbyssalHorror, C, "The Abyssal Deep-Sea Horror", "The Leviathan", "#1f7a8c",
                "Conjures crushing vortexes that drag every enemy in the room to a single choke point. [Q] to cast early.");
            yield return Make(ConstellationId.ClockworkFate, C, "The Clockwork Architect of Fate", "The Chronomancer", "#d9c36a",
                "Enemies move slower. A fatal blow rewinds you 3 seconds into the past (once per run).");
            yield return Make(ConstellationId.Procrastinator, M, "The Procrastinating God of Tomorrow", "The Slacker", "#8fb8a0",
                "Stand perfectly still for 2 seconds: your next attack deals +300% damage.");
            yield return Make(ConstellationId.MoneyThrower, M, "The Sponsor Who Throws Money At Problems", "The Whale", "#ffd700",
                "Ranged attacks fire your gold as ammo. All damage scales with your wallet.", alters: true);
            yield return Make(ConstellationId.TropeCritic, M, "The Critic of Terrible Tropes", "The Meta-Commentator", "#ff7f9e",
                "Boss phase shields shatter instantly ('uninspired writing'). Breakables break on sight.");
            yield return Make(ConstellationId.CosmicChef, M, "The Chef Who Seasoned the Universe", "The Culinaromancer", "#ff6a3d",
                "Fallen foes drop spices: Salt stuns, Chili ignites, MSG doubles Stardust drops.");
            yield return Make(ConstellationId.ChatMod, M, "The Over-Enthusiastic Chat Moderator", "The Streamer", "#b185ff",
                "Chat prompts appear on screen. Press [F] (or click) in time to cash in the hype.");
            yield return Make(ConstellationId.MuscleBrain, M, "The Entity Obsessed with Direct Punching", "The Meathead", "#ff9248",
                "Ranged weapons disabled. Melee strikes hit for +1000%, but swing a little slower.", spd: 1.1f, alters: true);
        }
    }
}
