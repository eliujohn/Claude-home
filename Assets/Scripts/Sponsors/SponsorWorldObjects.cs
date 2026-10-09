using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>Autonomous undead ally raised by The Monarch of the Bone Throne.</summary>
    public class SpectralThrall : MonoBehaviour
    {
        static readonly List<SpectralThrall> all = new List<SpectralThrall>();
        public static int Count => all.Count;

        float lifetime = 10f, damageMultiplier = 1f, baseDamage = 10f, attackClock;
        Transform targetEnemy;

        public static SpectralThrall Spawn(Vector2 pos, float baseDamage, float damageBonus, float duration = 10f)
        {
            var go = new GameObject("Spectral Thrall");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.6f;
            SpriteFactory.Attach(go, SpriteFactory.Circle, new Color(0.6f, 0.95f, 0.85f, 0.75f), 4);
            go.AddComponent<YSort>();
            var t = go.AddComponent<SpectralThrall>();
            t.baseDamage = baseDamage;
            t.InitializeThrall(duration, damageBonus);
            return t;
        }

        public void InitializeThrall(float duration, float damageBonus) { lifetime = duration; damageMultiplier = damageBonus; FindTarget(); }

        void OnEnable() => all.Add(this);
        void OnDisable() => all.Remove(this);

        public static void DestroyAll()
        {
            for (int i = all.Count - 1; i >= 0; i--) if (all[i] != null) Destroy(all[i].gameObject);
            all.Clear();
        }

        void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f || GameManager.State != GameFlow.Run) { Destroy(gameObject); return; }
            attackClock -= Time.deltaTime;
            if (targetEnemy == null) FindTarget();
            if (targetEnemy != null) MoveAndAttack(); else FollowOwner();
        }

        void FindTarget()
        {
            var e = Enemy.Nearest(transform.position, 15f);
            targetEnemy = e != null ? e.transform : null;
        }

        void FollowOwner()
        {
            var p = Player.Current;
            if (p == null) return;
            if (((Vector2)p.Position - (Vector2)transform.position).magnitude > 2.5f)
                transform.position = Vector2.MoveTowards(transform.position, p.Position, 4f * Time.deltaTime);
        }

        void MoveAndAttack()
        {
            Vector2 to = (Vector2)targetEnemy.position - (Vector2)transform.position;
            transform.position = Vector2.MoveTowards(transform.position, targetEnemy.position, 4f * Time.deltaTime);
            if (to.magnitude <= 0.9f && attackClock <= 0f)
            {
                attackClock = 0.6f;
                var e = targetEnemy.GetComponent<Enemy>();
                var p = Player.Current;
                if (e != null && p != null)
                    p.Combat.DealDamage(e, new DamageInfo
                    {
                        amount = baseDamage * damageMultiplier, fromPlayer = true, isMelee = true, source = p.gameObject,
                        sourceName = "Thrall", direction = to.normalized, statuses = StatusType.GraveBound
                    }, e.Position);
            }
        }
    }

    /// <summary>Miniature sun orbiting Alistair: dissolves armor on contact, devours enemy projectiles.</summary>
    public class OrbitingSun : MonoBehaviour
    {
        Transform owner;
        float angle, damage, tickClock;
        const float OrbitRadius = 1.9f, AngularSpeed = 130f, ContactRadius = 0.75f;

        public static OrbitingSun Create(Transform owner, float startAngle, float damage, Color color)
        {
            var go = new GameObject("Orbiting Sun");
            go.transform.localScale = Vector3.one * 0.55f;
            SpriteFactory.Attach(go, SpriteFactory.Circle, color, 4000);
            var s = go.AddComponent<OrbitingSun>();
            s.owner = owner; s.angle = startAngle; s.damage = damage;
            return s;
        }

        void Update()
        {
            if (owner == null) { Destroy(gameObject); return; }
            angle += AngularSpeed * Time.deltaTime;
            transform.position = (Vector2)owner.position + Vec2Util.FromAngle(angle) * OrbitRadius;

            for (int i = Projectile.Active.Count - 1; i >= 0; i--)
            {
                var pr = Projectile.Active[i];
                if (pr != null && !pr.Friendly && (pr.Position - (Vector2)transform.position).sqrMagnitude < ContactRadius * ContactRadius)
                { Fx.Pulse(pr.Position, 0.5f, new Color(1f, 0.8f, 0.3f, 0.8f), 0.15f); Destroy(pr.gameObject); }
            }

            tickClock -= Time.deltaTime;
            if (tickClock > 0f) return;
            tickClock = 0.3f;
            var p = Player.Current;
            if (p == null) return;
            var near = new List<Enemy>(8);
            Enemy.Query(transform.position, ContactRadius + 0.4f, near);
            foreach (var e in near)
                p.Combat.DealDamage(e, new DamageInfo
                {
                    amount = damage, fromPlayer = true, source = p.gameObject, sourceName = "Star Core",
                    statuses = StatusType.Corroded, statusDuration = 4f, direction = (e.Position - (Vector2)transform.position).normalized
                }, e.Position);
        }
    }

    /// <summary>Leviathan vortex: drags every enemy toward one choke point and crushes them there.</summary>
    public class GravityVortex : MonoBehaviour
    {
        float life, radius, strength;
        SpriteRenderer sr;
        readonly List<Enemy> buf = new List<Enemy>(64);

        public static GravityVortex Spawn(Vector2 pos, float lifetime, float pullStrength, Color color)
        {
            var go = new GameObject("Abyssal Vortex");
            go.transform.position = pos;
            var v = go.AddComponent<GravityVortex>();
            v.life = lifetime; v.strength = pullStrength; v.radius = 40f;      // room-wide
            v.sr = SpriteFactory.Attach(go, SpriteFactory.Circle, new Color(color.r * 0.4f, color.g * 0.4f, color.b * 0.5f, 0.75f), -5);
            return v;
        }

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }
            float pulse = 2.2f + 0.3f * Mathf.Sin(Time.time * 9f);
            transform.localScale = Vector3.one * pulse * Mathf.Clamp01(life);
            transform.Rotate(0, 0, 220f * Time.deltaTime);

            Enemy.Query(transform.position, radius, buf);
            foreach (var e in buf)
            {
                Vector2 to = (Vector2)transform.position - e.Position;
                float d = to.magnitude;
                float resist = e.IsBoss ? 0.4f : 1f;
                if (d > 0.7f) e.Push(to / d * strength * resist * 6f * Time.deltaTime);
                if (d < 2.5f) e.Status.Apply(StatusType.Crushed, 1.2f, Player.Current != null ? Player.Current.gameObject : null);
            }
        }
    }

    public enum SpiceType { Salt, Chili, MSG }

    /// <summary>Spice dropped by fallen foes (Cosmic Chef). Salt stuns, Chili ignites, MSG boosts Stardust drops.</summary>
    public class Spice : MonoBehaviour
    {
        SpiceType type;
        float power, life = 14f;

        public static Spice Spawn(Vector2 pos, SpiceType type, float power)
        {
            var go = new GameObject("Spice " + type);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.4f;
            Color c = type == SpiceType.Salt ? Color.white : type == SpiceType.Chili ? new Color(1f, 0.2f, 0.1f) : new Color(1f, 0.9f, 0.2f);
            SpriteFactory.Attach(go, SpriteFactory.Square, c, 6);
            go.AddComponent<YSort>();
            var s = go.AddComponent<Spice>();
            s.type = type; s.power = power;
            return s;
        }

        void Update()
        {
            life -= Time.deltaTime;
            transform.Rotate(0, 0, 90f * Time.deltaTime);
            var p = Player.Current;
            if (life <= 0f || p == null) { Destroy(gameObject); return; }
            if (((Vector2)transform.position - p.Position).magnitude > 1.3f) return;

            var buf = new List<Enemy>(32);
            Enemy.Query(transform.position, 6f, buf);
            switch (type)
            {
                case SpiceType.Salt:
                    foreach (var e in buf) e.Status.Apply(StatusType.Stun, 2.5f * power, p.gameObject);
                    Fx.Pulse(transform.position, 6f, new Color(1, 1, 1, 0.5f), 0.4f);
                    break;
                case SpiceType.Chili:
                    foreach (var e in buf) e.Status.Apply(StatusType.Ignite, 5f * power, p.gameObject);
                    p.Run.chiliTimer = 8f * power;
                    Fx.Pulse(transform.position, 6f, new Color(1, 0.3f, 0.1f, 0.5f), 0.4f);
                    break;
                case SpiceType.MSG:
                    p.Run.metaDropMult = p.Run.metaDropMult < 2f ? 2f : p.Run.metaDropMult + 0.25f;
                    Fx.Pulse(p.Position, 2f, new Color(1, 0.9f, 0.2f, 0.7f), 0.4f);
                    break;
            }
            Destroy(gameObject);
        }
    }

    /// <summary>Static board of on-screen Chat Moderator prompts. The sponsor posts; ChatPromptView renders and accepts.</summary>
    public static class ChatPromptBoard
    {
        public class Prompt { public string title, subtitle; public float remaining = 4.5f; public Action onAccept; }

        public static readonly List<Prompt> Prompts = new List<Prompt>();

        public static void Post(string title, string subtitle, Action onAccept)
        {
            if (Prompts.Count >= 3) Prompts.RemoveAt(0);
            Prompts.Add(new Prompt { title = title, subtitle = subtitle, onAccept = onAccept });
        }

        public static void Accept(int index)
        {
            if (index < 0 || index >= Prompts.Count) return;
            var p = Prompts[index];
            Prompts.RemoveAt(index);
            p.onAccept?.Invoke();
        }

        public static void Tick(float dt)
        {
            for (int i = Prompts.Count - 1; i >= 0; i--)
            {
                Prompts[i].remaining -= dt;
                if (Prompts[i].remaining <= 0f) Prompts.RemoveAt(i);
            }
        }

        public static void Clear() => Prompts.Clear();
    }
}
