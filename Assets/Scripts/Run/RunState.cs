using System;
using System.Collections.Generic;

namespace Ascension
{
    /// <summary>Everything that resets on death: temporary buffs, run currency, progression pointer.</summary>
    public class RunState
    {
        public readonly int seed;
        public readonly Random rng;
        public int gold, stardustEarned, enemiesKilled, roomsCleared;
        public float metaDropMult = 1f, chiliTimer, elapsed;
        public int defiancesLeft, rerollsLeft;
        public int biomeIndex, layerIndex;
        public readonly List<ConstellationId> picks = new List<ConstellationId>();

        public RunState(int seed) { this.seed = seed; rng = new Random(seed); }

        public int Depth => biomeIndex * 5 + layerIndex;
    }

    [Serializable]
    public class DeathRecord
    {
        public string killer = "";
        public bool byHazard, byBoss, victory;
        public int biome, layer, gold, stardust, enemiesKilled;
        public float seconds;
        public List<int> sponsors = new List<int>();
        public int highestWealth;
    }
}
