using System.Collections.Generic;

namespace Ascension
{
    public struct BlessingChoice
    {
        public ConstellationSponsor def;
        public int fromRank, toRank;
        public bool IsNew => fromRank == 0;
    }

    public static class BlessingService
    {
        public static int OfferSize => 3 + MetaProgression.Rank(MatrixId.SponsorFavor);

        /// <summary>Weighted draw without replacement; unowned sponsors are favoured, maxed ones excluded.</summary>
        public static List<BlessingChoice> Roll(RunState run, SponsorManager sponsors, int count)
        {
            var pool = new List<BlessingChoice>();
            foreach (var def in ConstellationDatabase.All)
            {
                int rank = sponsors.RankOf(def.id);
                if (rank >= def.maxRank) continue;
                pool.Add(new BlessingChoice { def = def, fromRank = rank, toRank = rank + 1 });
            }
            var result = new List<BlessingChoice>();
            while (result.Count < count && pool.Count > 0)
            {
                float total = 0f;
                foreach (var c in pool) total += c.IsNew ? 3f : 2f;
                float roll = (float)run.rng.NextDouble() * total;
                for (int i = 0; i < pool.Count; i++)
                {
                    roll -= pool[i].IsNew ? 3f : 2f;
                    if (roll <= 0f || i == pool.Count - 1) { result.Add(pool[i]); pool.RemoveAt(i); break; }
                }
            }
            return result;
        }
    }
}
