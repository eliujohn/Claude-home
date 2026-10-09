using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class Projectile : MonoBehaviour
    {
        public static readonly List<Projectile> Active = new List<Projectile>(128);

        public bool Friendly;
        public DamageInfo Info;
        public float Speed = 12f, Life = 3f;
        public int Pierce;
        public bool Homing, BendAroundWalls;
        public float TurnRate = 520f;
        public Enemy Target;
        Vector2 dir;
        readonly HashSet<Enemy> alreadyHit = new HashSet<Enemy>();
        public Vector2 Position => transform.position;

        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        public static Projectile Spawn(Vector2 pos, Vector2 dir, bool friendly, DamageInfo info, float speed, float radius, Color color, float life)
        {
            var go = new GameObject(friendly ? "PlayerShot" : "EnemyShot");
            go.layer = Layers.Projectile;
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * radius * 2f;
            SpriteFactory.Attach(go, SpriteFactory.Circle, color, 4000);
            var p = go.AddComponent<Projectile>();   // pulls in Rigidbody2D + CircleCollider2D
            var rb = go.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f;
            var col = go.GetComponent<CircleCollider2D>();
            col.isTrigger = true; col.radius = 0.5f;
            p.Friendly = friendly; p.Info = info; p.Speed = speed; p.Life = life; p.dir = dir.normalized;
            return p;
        }

        public void SetColor(Color c) => GetComponent<SpriteRenderer>().color = c;

        void Update()
        {
            float scale = Friendly ? 1f : GlobalTime.EnemyTimeScale;
            float dt = Time.deltaTime * scale;
            Life -= dt;
            if (Life <= 0f) { Destroy(gameObject); return; }
            if (Homing) Steer(dt);
            transform.position += (Vector3)(dir * Speed * dt);
        }

        void Steer(float dt)
        {
            if (Target == null || Target.Health.IsDead) Target = Enemy.Nearest(Position, 25f);
            if (Target == null) return;
            Vector2 toT = (Vector2)Target.transform.position - Position;
            float dist = toT.magnitude;
            if (dist < 0.01f) return;
            Vector2 want = toT / dist;
            if (BendAroundWalls) want = AvoidObstacles(want, Mathf.Min(dist, 3.5f));
            dir = Vec2Util.RotateToward(dir, want, TurnRate * Mathf.Deg2Rad * dt);
        }

        /// <summary>Probe fan of rays; if the straight path is blocked pick the nearest clear heading.</summary>
        Vector2 AvoidObstacles(Vector2 want, float lookAhead)
        {
            if (!Physics2D.Raycast(Position, want, lookAhead, Layers.ObstacleMask)) return want;
            for (int step = 1; step <= 6; step++)
            {
                float a = step * 25f;
                Vector2 left = Vec2Util.Rotate(want, a), right = Vec2Util.Rotate(want, -a);
                bool lf = !Physics2D.Raycast(Position, left, lookAhead, Layers.ObstacleMask);
                bool rf = !Physics2D.Raycast(Position, right, lookAhead, Layers.ObstacleMask);
                if (lf && rf) return Vector2.Dot(left, dir) >= Vector2.Dot(right, dir) ? left : right;
                if (lf) return left;
                if (rf) return right;
            }
            return want;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.isTrigger) return;
            if (Friendly)
            {
                var br = other.GetComponent<Breakable>();
                if (br != null) { br.Break(); Destroy(gameObject); return; }
                var e = other.GetComponent<Enemy>();
                if (e != null)
                {
                    if (!e.CanBeHit || alreadyHit.Contains(e)) return;
                    alreadyHit.Add(e);
                    Info.direction = dir;
                    if (Player.Current != null) Player.Current.Combat.DealDamage(e, Info, Position);
                    if (Pierce-- <= 0) Destroy(gameObject);
                    return;
                }
            }
            else
            {
                var p = other.GetComponent<Player>();
                if (p != null)
                {
                    var hit = Info.Clone(); hit.direction = dir;
                    p.Health.TakeDamage(hit);
                    Destroy(gameObject);
                    return;
                }
            }
            if (other.GetComponent<Obstacle>() != null) Destroy(gameObject);
        }
    }
}
