using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>Marker for solid scenery (walls, pillars). Lives on Layers.Obstacle.</summary>
    public class Obstacle : MonoBehaviour { }

    public class Breakable : MonoBehaviour
    {
        public static readonly List<Breakable> Active = new List<Breakable>();
        public int gold = 2;
        bool broken;

        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        public void Break()
        {
            if (broken) return;
            broken = true;
            Fx.Pulse(transform.position, 0.8f, new Color(0.8f, 0.7f, 0.5f, 0.8f), 0.2f);
            if (Player.Current != null && gold > 0) Player.Current.Run.gold += gold;
            Destroy(gameObject);
        }
    }

    /// <summary>Timed damage area: magma pools (always on) or laser beams (telegraphed, then firing).</summary>
    public class HazardZone : MonoBehaviour
    {
        public string hazardName = "Magma";
        public Vector2 size = new Vector2(3, 3);
        public float damage = 8f, tickRate = 0.5f;
        public float period = 0f, telegraph = 1.0f, onTime = 1.2f;   // period 0 = always on
        public float phaseOffset;
        SpriteRenderer sr;
        float clock, nextTick;
        Color baseColor;
        bool Active => period <= 0f || ((clock + phaseOffset) % period) >= period - onTime;
        bool Telegraphing => period > 0f && !Active && ((clock + phaseOffset) % period) >= period - onTime - telegraph;

        public void Setup(Color color)
        {
            baseColor = color;
            sr = SpriteFactory.Attach(gameObject, SpriteFactory.Square, color, -9000);
            transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        void Update()
        {
            clock += Time.deltaTime;
            if (sr != null)
            {
                var c = baseColor;
                c.a = period <= 0f ? 0.8f : Active ? 0.95f : Telegraphing ? 0.25f + 0.2f * Mathf.PingPong(clock * 8f, 1f) : 0.06f;
                sr.color = c;
            }
            var p = Player.Current;
            if (p == null || p.Health.IsDead || !Active || clock < nextTick) return;
            Vector2 d = p.Position - (Vector2)transform.position;
            if (Mathf.Abs(d.x) <= size.x * 0.5f && Mathf.Abs(d.y) <= size.y * 0.5f)
            {
                nextTick = clock + tickRate;
                p.Health.TakeDamage(new DamageInfo { amount = damage, isHazard = true, sourceName = hazardName });
            }
        }
    }
}
