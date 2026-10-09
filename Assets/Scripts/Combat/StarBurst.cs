using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>Star-Bursted State payoff: enemies carrying two cosmic ailments detonate into starlight on death.</summary>
    public static class StarBurst
    {
        public const float Radius = 3.4f;

        public static void Detonate(Vector2 pos, float victimMaxHp)
        {
            float dmg = 30f + victimMaxHp * 0.6f;
            Fx.Pulse(pos, Radius, new Color(1f, 0.95f, 0.7f, 0.9f), 0.45f);
            Fx.Pulse(pos, Radius * 0.6f, new Color(0.7f, 0.8f, 1f, 0.9f), 0.3f);
            CameraRig.Shake(0.25f);
            var buffer = new List<Enemy>(16);   // local: chain detonations re-enter this method
            Enemy.Query(pos, Radius, buffer);
            for (int i = 0; i < buffer.Count; i++)
            {
                var e = buffer[i];
                if (e == null || e.Health.IsDead) continue;
                var info = new DamageInfo { amount = dmg, fromPlayer = true, ignoreArmor = true, isStatusTick = true, sourceName = "Star-Burst" };
                e.Hit(info);
            }
        }
    }
}
