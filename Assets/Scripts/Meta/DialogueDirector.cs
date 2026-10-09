using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>
    /// Hub NPC dialogue. Lines are data: a predicate over the last death + overall progress, a priority and a once-flag.
    /// The highest-priority eligible line plays; ties and the generic pool are random.
    /// </summary>
    public static class DialogueDirector
    {
        public const string Vesper = "Vesper, Ferrywoman of the Terminal";
        public const string Orrin = "Orrin, Keeper of the Stellar Mirror";
        public const string Cassian = "Cassian, Chronicler of Deeds";

        class Line
        {
            public string id, npc;
            public int priority;
            public bool once;
            public Func<Ctx, bool> when;
            public string[] text;
        }

        public class Ctx
        {
            public SaveData S => MetaProgression.Data;
            public bool HasLast => S.hasLastDeath;
            public DeathRecord L => S.lastDeath;
            public bool Victory => HasLast && L.victory;
            public bool Killed(string who) => HasLast && !L.victory && L.killer == who;
            public bool Had(ConstellationId id) => HasLast && L.sponsors.Contains((int)id);
        }

        static readonly List<Line> lines = new List<Line>();
        static readonly Ctx ctx = new Ctx();

        static void Add(string id, string npc, int priority, bool once, Func<Ctx, bool> when, params string[] text)
            => lines.Add(new Line { id = id, npc = npc, priority = priority, once = once, when = when, text = text });

        static DialogueDirector()
        {
            // ---------------- Vesper: reacts to HOW you died ----------------
            Add("v.first", Vesper, 100, true, c => !c.HasLast && c.S.runs == 0,
                "A soul that walks in without being dragged. Interesting.",
                "They call this the Abyss Terminal. Everything that falls ends up here, and everything that ends up here is meant to stay.",
                "You are not meant to stay, are you, Alistair? The Ascent gate is behind me. Climb.");
            Add("v.victory", Vesper, 99, false, c => c.Victory,
                "...The Sovereign's throne is empty.",
                "I have ferried souls for ten thousand years and never once seen Heaven's light change colour. Well done, Defiant.");
            Add("v.magma", Vesper, 80, true, c => c.Killed("Magma"),
                "Killed by the floor. The floor, Alistair.",
                "Lava is not a boss. It does not need to be defeated. It needs to be walked around.");
            Add("v.magma.again", Vesper, 79, false, c => c.Killed("Magma") && MetaProgression.KilledByCount("Magma") >= 3,
                "Magma again. At this point I think you're doing it on purpose.");
            Add("v.laser", Vesper, 80, true, c => c.Killed("Laser Matrix"),
                "A laser. Heaven's idea of a welcome mat.",
                "Watch the telegraph. The beams always warn you first. They're polite like that.");
            Add("v.inquisitor", Vesper, 85, true, c => c.Killed("Grand Inquisitor of the Flame"),
                "The Inquisitor's flame still clings to your soul. It will fade.",
                "...Probably.");
            Add("v.warlord", Vesper, 85, true, c => c.Killed("Phantom Warlord"),
                "The Warlord died centuries ago. Nobody told him. Nobody dares.");
            Add("v.herald", Vesper, 85, true, c => c.Killed("Archangel Herald"),
                "The Herald announces you to the Sovereign. Every time. With trumpets.",
                "I would prefer you didn't make him do it again, but I'm not your keeper.");
            Add("v.sovereign", Vesper, 90, true, c => c.Killed("The Sovereign of Judgment"),
                "So you stood before Him. And He judged you. Hm.",
                "Judgment is just another word for a fight you haven't won yet.");
            Add("v.boss.again", Vesper, 60, false, c => c.HasLast && c.L.byBoss,
                "Slain by {killer}. I'll chalk it up as a learning experience.");
            Add("v.hell", Vesper, 50, false, c => c.HasLast && !c.L.victory && c.L.biome == 0 && c.L.layer <= 1,
                "Barely out of the pit and back already. Pace yourself.");
            Add("v.earth", Vesper, 45, false, c => c.HasLast && !c.L.victory && c.L.biome == 1,
                "The mortals of Earth are crueller than anything below. You get used to it.");
            Add("v.heaven", Vesper, 45, false, c => c.HasLast && !c.L.victory && c.L.biome == 2,
                "Heaven's zealots don't hate you. They simply can't imagine you existing. It's worse.");
            Add("v.rich", Vesper, 55, false, c => c.HasLast && !c.L.victory && c.L.gold >= 300,
                "You died holding {gold} gold. Coin does not ferry anyone anywhere, you know.");
            Add("v.thralls", Vesper, 55, false, c => c.Had(ConstellationId.BoneThrone) && !c.L.victory,
                "Your thralls were lovely. For about ten seconds each.");
            Add("v.many", Vesper, 40, false, c => c.S.deaths >= 8,
                "You've fallen {deaths} times now. I've stopped bringing the blanket.");
            Add("v.gen1", Vesper, 0, false, c => true, "Back so soon.");
            Add("v.gen2", Vesper, 0, false, c => true, "Slain by {killer}. I'll note it down.");
            Add("v.gen3", Vesper, 0, false, c => true, "Again? Good. The Terminal likes persistent souls.");

            // ---------------- Orrin: the Matrix and the Constellations ----------------
            Add("o.first", Orrin, 100, true, c => c.S.runs == 0,
                "This is the Stellar Mirror Matrix. Spend the Stardust you bring home and the Constellations remember you stronger.",
                "They are watching, you know. Every swing of yours is a broadcast.");
            Add("o.meathead", Orrin, 85, false, c => c.Had(ConstellationId.MuscleBrain) && !c.L.victory,
                "The Entity Obsessed with Direct Punching is thrilled. Everyone else is concerned.");
            Add("o.whale", Orrin, 85, false, c => c.Had(ConstellationId.MoneyThrower) && !c.L.victory,
                "You died with {gold} gold to your name. The Whale weeps. Wallets are not armour.");
            Add("o.chrono", Orrin, 80, false, c => c.Had(ConstellationId.ClockworkFate) && !c.L.victory,
                "Even the Architect of Fate cannot rewind a mistake you insist on repeating.");
            Add("o.slacker", Orrin, 80, false, c => c.Had(ConstellationId.Procrastinator) && !c.L.victory,
                "The God of Tomorrow wanted you standing still. Not dying still.");
            Add("o.trickster", Orrin, 80, false, c => c.Had(ConstellationId.FatalParadox) && !c.L.victory,
                "The Weaver says the hit that killed you was always going to happen. She also says she has no refunds.");
            Add("o.critic", Orrin, 80, false, c => c.Had(ConstellationId.TropeCritic) && !c.L.victory,
                "The Critic gave your death two stars: 'predictable, lacks stakes'.");
            Add("o.chat", Orrin, 80, false, c => c.Had(ConstellationId.ChatMod) && !c.L.victory,
                "Chat says: 'F'. Which, I gather, is exactly what you were supposed to press.");
            Add("o.dust", Orrin, 60, false, c => c.HasLast && c.L.stardust >= 10,
                "{stardust} Stardust. Not bad. Feed it to the Matrix.");
            Add("o.win", Orrin, 99, false, c => c.Victory,
                "The Constellations are... unsettled. They have never had a mortal reach the Apex before.");
            Add("o.gen1", Orrin, 0, false, c => true, "Spend your Stardust. Hoarding is for dragons.");
            Add("o.gen2", Orrin, 0, false, c => true, "Each Constellation answers to a different wager. Choose the ones that match how you fight.");
            Add("o.gen3", Orrin, 0, false, c => true, "Two cosmic ailments on one foe, and it bursts into starlight. The Matrix cannot teach you that. Experiment.");

            // ---------------- Cassian: chronicler, meta-commentary ----------------
            Add("c.first", Cassian, 100, true, c => c.S.runs == 0,
                "I am Cassian. I record what happens to you. You're the protagonist, apparently.",
                "Try to make it a good story.");
            Add("c.win", Cassian, 99, false, c => c.Victory, "Heaven has a vacancy. I have never had to write a sentence like that.");
            Add("c.killer", Cassian, 20, false, c => c.HasLast && !c.L.victory, "Entry written: 'Slain by {killer}, {seconds} seconds into the climb.' Dramatic.");
            Add("c.three", Cassian, 30, false, c => c.S.runs >= 3 && c.S.victories == 0, "Three attempts. Statistically, the protagonist always wins on the fourth.");
            Add("c.gen1", Cassian, 0, false, c => true, "The Critic of Terrible Tropes keeps submitting notes on your story. I keep deleting them.");
            Add("c.gen2", Cassian, 0, false, c => true, "Rumour says the Sovereign has never lost an argument. Rumour has clearly never met you.");
            Add("c.gen3", Cassian, 0, false, c => true, "I've made a chart of your deaths. It is, regrettably, beautiful.");
        }

        public static string[] Pick(string npc)
        {
            Line best = null;
            var tied = new List<Line>();
            foreach (var l in lines)
            {
                if (l.npc != npc) continue;
                if (l.once && MetaProgression.Seen(l.id)) continue;
                bool ok;
                try { ok = l.when(ctx); } catch (Exception) { ok = false; }
                if (!ok) continue;
                if (best == null || l.priority > best.priority) { best = l; tied.Clear(); tied.Add(l); }
                else if (l.priority == best.priority) tied.Add(l);
            }
            if (best == null) return new[] { "..." };
            var chosen = tied[UnityEngine.Random.Range(0, tied.Count)];
            if (chosen.once) MetaProgression.MarkSeen(chosen.id);

            var res = new string[chosen.text.Length];
            for (int i = 0; i < res.Length; i++) res[i] = Fill(chosen.text[i]);
            return res;
        }

        static string Fill(string t)
        {
            var s = MetaProgression.Data;
            var d = s.lastDeath;
            return t.Replace("{killer}", d.killer ?? "something")
                    .Replace("{gold}", d.gold.ToString())
                    .Replace("{stardust}", d.stardust.ToString())
                    .Replace("{deaths}", s.deaths.ToString())
                    .Replace("{seconds}", Mathf.RoundToInt(d.seconds).ToString());
        }
    }
}
