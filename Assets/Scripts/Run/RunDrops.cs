using UnityEngine;

namespace Ascension
{
    /// <summary>Per-kill economy: gold, Stardust (affected by MSG and the Stardust Siphon upgrade), occasional vials.</summary>
    public static class RunDrops
    {
        public static void Roll(Enemy e)
        {
            var p = Player.Current;
            if (p == null || GameManager.State != GameFlow.Run) return;
            var run = p.Run;
            run.enemiesKilled++;
            run.gold += e.Def.gold;
            if (Random.Range(0, 100) < e.Def.stardustChance) AddStardust(run, 1);
            if (!e.IsBoss && Random.value < 0.035f) RewardPickup.Spawn(RoomController.Current != null ? RoomController.Current.transform : null, e.Position, RewardType.Vial, 0);
        }

        public static void AddStardust(RunState run, int amount)
        {
            float mult = run.metaDropMult * (1f + 0.1f * MetaProgression.Rank(MatrixId.StardustSiphon));
            run.stardustEarned += Mathf.Max(1, Mathf.RoundToInt(amount * mult));
        }
    }
}
