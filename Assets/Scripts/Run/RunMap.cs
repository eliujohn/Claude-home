using System;
using System.Collections.Generic;

namespace Ascension
{
    public class RoomNode
    {
        public RoomType type;
        public RewardType reward;
        public int biome, layer;
        public bool IsBoss => type == RoomType.Boss;
    }

    /// <summary>
    /// Sequential multi-realm layout. Each biome is a list of layers; every layer offers 1-3 doors whose reward icon
    /// is previewed. The final layer of a biome is its boss; clearing it opens the Transition door to the next realm.
    /// </summary>
    public class RunMap
    {
        public int Biome { get; private set; }
        public List<RoomNode[]> Layers { get; private set; } = new List<RoomNode[]>();
        public int LayerIndex { get; set; } = -1;

        public void EnterBiome(int biome, Random rng)
        {
            Biome = biome;
            LayerIndex = -1;
            Layers = Generate(biome, rng);
        }

        public RoomNode[] PeekNext() => LayerIndex + 1 < Layers.Count ? Layers[LayerIndex + 1] : null;
        public void Commit(RoomNode n) => LayerIndex = n.layer;

        public static List<RoomNode[]> Generate(int biome, Random rng)
        {
            var def = BiomeDatabase.All[biome];
            var layers = new List<RoomNode[]>();
            for (int l = 0; l < def.chambers; l++)
            {
                bool first = l == 0, last = l == def.chambers - 1;
                if (first)
                {
                    // Opening chamber of a realm: single door, always a Blessing (final realm opens with a Sanctuary).
                    layers.Add(new[] { new RoomNode { biome = biome, layer = l, type = biome == 3 ? RoomType.Sanctuary : RoomType.Combat, reward = RewardType.Blessing } });
                    continue;
                }
                int count = rng.Next(2, 4);
                var nodes = new RoomNode[count];
                var used = new HashSet<RewardType>();
                for (int i = 0; i < count; i++)
                {
                    RoomNode n = null;
                    for (int tries = 0; tries < 4; tries++)
                    {
                        n = Roll(biome, l, last, rng);
                        if (used.Add(n.reward)) break;
                    }
                    nodes[i] = n;
                }
                layers.Add(nodes);
            }
            layers.Add(new[] { new RoomNode { biome = biome, layer = def.chambers, type = RoomType.Boss, reward = RewardType.Blessing } });
            return layers;
        }

        static RoomNode Roll(int biome, int layer, bool last, Random rng)
        {
            var n = new RoomNode { biome = biome, layer = layer };
            int t = rng.Next(100);
            n.type = t < 62 ? RoomType.Combat : t < 82 ? RoomType.Elite : RoomType.Sanctuary;
            if (last && n.type == RoomType.Sanctuary) n.type = RoomType.Combat;
            switch (n.type)
            {
                case RoomType.Elite: n.reward = rng.Next(2) == 0 ? RewardType.Blessing : RewardType.Vitality; break;
                case RoomType.Sanctuary: n.reward = rng.Next(100) < 60 ? RewardType.Blessing : RewardType.Vial; break;
                default:
                    int r = rng.Next(100);
                    n.reward = r < 30 ? RewardType.Blessing : r < 55 ? RewardType.Gold : r < 75 ? RewardType.Stardust : r < 90 ? RewardType.Vial : RewardType.Vitality;
                    break;
            }
            return n;
        }

        public static string Describe(RoomNode n, bool revealed)
        {
            if (n.type == RoomType.Boss) return "BOSS";
            if (n.type == RoomType.Transition) return "ASCEND >> " + BiomeDatabase.All[n.biome].name;
            string reward;
            switch (n.reward)
            {
                case RewardType.Blessing: reward = "* Stellar Blessing"; break;
                case RewardType.Gold: reward = "Gold"; break;
                case RewardType.Stardust: reward = "Stardust"; break;
                case RewardType.Vial: reward = "Ambrosia Vial"; break;
                case RewardType.Vitality: reward = "Vitality Shard"; break;
                default: reward = "-"; break;
            }
            if (!revealed) return reward;
            string kind = n.type == RoomType.Elite ? "ELITE" : n.type == RoomType.Sanctuary ? "Sanctuary" : "Chamber";
            return kind + "\n" + reward;
        }
    }
}
