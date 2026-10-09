using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    [RequireComponent(typeof(Health), typeof(StatusController), typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> Active = new List<Enemy>(64);

        public EnemyDef Def { get; protected set; }
        public Health Health { get; private set; }
        public StatusController Status { get; private set; }
        public Rigidbody2D Body { get; private set; }
        public Vector2 Facing { get; protected set; } = Vector2.down;
        public virtual bool IsBoss => false;
        public Vector2 Position => transform.position;

        protected SpriteRenderer sprite;
        Vector2 external;
        enum AtkState { Idle, Windup, Strike, Recover }
        AtkState atk;
        float atkTimer, cooldown, chargeTimer, revealTimer;
        Vector2 lockedDir;
        bool hitThisStrike;
        float wanderTimer; Vector2 wanderDir;

        // ---------- registry ----------
        void OnEnable() { if (!Active.Contains(this)) Active.Add(this); }
        void OnDisable() => Active.Remove(this);

        public static void Query(Vector2 center, float radius, List<Enemy> buffer)
        {
            buffer.Clear();
            float r2 = radius * radius;
            for (int i = 0; i < Active.Count; i++)
            {
                var e = Active[i];
                if (e != null && !e.Health.IsDead && ((Vector2)e.transform.position - center).sqrMagnitude <= r2) buffer.Add(e);
            }
        }

        public static void Snapshot(List<Enemy> buffer)
        {
            buffer.Clear();
            for (int i = 0; i < Active.Count; i++) if (Active[i] != null && !Active[i].Health.IsDead) buffer.Add(Active[i]);
        }

        public static Enemy Nearest(Vector2 from, float maxDist, bool targetableOnly = true)
        {
            Enemy best = null; float bd = maxDist * maxDist;
            for (int i = 0; i < Active.Count; i++)
            {
                var e = Active[i];
                if (e == null || e.Health.IsDead || (targetableOnly && !e.CanBeHit)) continue;
                float d = ((Vector2)e.transform.position - from).sqrMagnitude;
                if (d < bd) { bd = d; best = e; }
            }
            return best;
        }

        // ---------- creation ----------
        public static Enemy Spawn(EnemyDef def, Vector2 pos, float hpScale = 1f, float dmgScale = 1f)
        {
            var go = new GameObject(def.name);
            go.transform.position = pos;
            go.layer = Layers.Enemy;
            Enemy e = def is BossDef ? go.AddComponent<Boss>() : go.AddComponent<Enemy>();
            var scaled = def.Scaled(hpScale, dmgScale);
            if (def is BossDef bd) ((Boss)e).BossDef = new BossDef
            {
                name = bd.name, title = bd.title, hp = scaled.hp, damage = scaled.damage, speed = bd.speed, radius = bd.radius, color = bd.color,
                behaviour = bd.behaviour, projectileSpeed = bd.projectileSpeed, phases = bd.phases, addPool = bd.addPool, gold = bd.gold
            };
            e.Init(def is BossDef ? ((Boss)e).BossDef : scaled);
            return e;
        }

        protected virtual void Init(EnemyDef def)
        {
            Def = def;
            Health = GetComponent<Health>();
            Status = GetComponent<StatusController>();
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f; Body.freezeRotation = true; Body.bodyType = RigidbodyType2D.Dynamic;
            var col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = def.radius;
            sprite = SpriteFactory.Attach(gameObject, def.behaviour == EnemyBehaviour.Charger || def.behaviour == EnemyBehaviour.Turret ? SpriteFactory.Square : SpriteFactory.Circle, def.color, 0);
            transform.localScale = Vector3.one * def.radius * 2f;
            gameObject.AddComponent<YSort>();
            Health.Init(def.hp);
            Health.armor = def.armor;
            Health.Died += HandleDeath;
            cooldown = Random.Range(0.4f, 1.2f);
            Facing = Random.insideUnitCircle.normalized;
        }

        // ---------- targeting rules ----------
        public virtual bool CanBeHit
        {
            get
            {
                if (Health.IsDead) return false;
                if (Def.behaviour == EnemyBehaviour.Phaser && revealTimer <= 0f && !(Player.Current != null && Player.Current.TrueSight)) return false;
                return true;
            }
        }

        public float Hit(DamageInfo d)
        {
            if (!CanBeHit) return 0f;
            Status.Apply(d.statuses, d.statusDuration, d.source);
            return Health.TakeDamage(d);
        }

        public void Push(Vector2 velocity) => external += velocity;

        // ---------- loop ----------
        void Update()
        {
            if (Health.IsDead) return;
            float dt = Time.deltaTime * GlobalTime.EnemyTimeScale;
            external = Vector2.Lerp(external, Vector2.zero, 1f - Mathf.Exp(-6f * Time.deltaTime));
            revealTimer -= dt;
            cooldown -= dt;
            ApplyVisuals();

            if (Status.Stunned) { Body.linearVelocity = external; atk = AtkState.Idle; return; }
            Think(dt);
        }

        void ApplyVisuals()
        {
            var c = Def.color;
            if (Def.behaviour == EnemyBehaviour.Phaser)
            {
                bool vis = revealTimer > 0f || (Player.Current != null && Player.Current.TrueSight);
                c.a = vis ? 1f : 0.18f;
            }
            if (Status.IsStarBursted) c = Color.Lerp(c, new Color(1f, 0.95f, 0.6f), 0.5f + 0.4f * Mathf.PingPong(Time.time * 4f, 1f));
            else if (Status.CosmicCount == 1) c = Color.Lerp(c, new Color(0.7f, 0.5f, 1f), 0.3f);
            if (atk == AtkState.Windup) c = Color.Lerp(c, Color.white, 0.6f);
            sprite.color = c;
        }

        protected void MoveVelocity(Vector2 desired)
        {
            Body.linearVelocity = desired * Status.SpeedMultiplier * GlobalTime.EnemyTimeScale + external;
        }

        protected bool TryGetTarget(out Vector2 toPlayer, out float dist)
        {
            var p = Player.Current;
            toPlayer = Vector2.zero; dist = float.MaxValue;
            if (p == null || p.Health.IsDead) return false;
            if (p.IsInvisible && !p.TrueSight) return false;
            toPlayer = p.Position - Position; dist = toPlayer.magnitude;
            return true;
        }

        protected void FaceToward(Vector2 dir, float dt)
        {
            if (dir.sqrMagnitude > 1e-4f) Facing = Vec2Util.RotateToward(Facing, dir.normalized, 4.2f * dt);
        }

        protected virtual void Think(float dt)
        {
            bool target = TryGetTarget(out Vector2 to, out float dist);
            Vector2 dir = dist > 0.001f && target ? to / dist : Vector2.zero;
            if (!target) { Wander(dt); return; }

            switch (Def.behaviour)
            {
                case EnemyBehaviour.Chaser:
                case EnemyBehaviour.Phaser: ThinkMelee(dt, dir, dist); break;
                case EnemyBehaviour.Shooter: ThinkShooter(dt, dir, dist); break;
                case EnemyBehaviour.Charger: ThinkCharger(dt, dir, dist); break;
                case EnemyBehaviour.Turret: ThinkTurret(dt, dir, dist); break;
            }
        }

        void Wander(float dt)
        {
            wanderTimer -= dt;
            if (wanderTimer <= 0f) { wanderTimer = Random.Range(1f, 2.5f); wanderDir = Random.value < 0.3f ? Vector2.zero : Random.insideUnitCircle.normalized; }
            if (Def.behaviour == EnemyBehaviour.Turret) wanderDir = Vector2.zero;
            FaceToward(wanderDir, dt);
            MoveVelocity(wanderDir * Def.speed * 0.4f);
        }

        void ThinkMelee(float dt, Vector2 dir, float dist)
        {
            FaceToward(dir, dt);
            switch (atk)
            {
                case AtkState.Idle:
                    MoveVelocity(dist > Def.attackRange * 0.8f ? dir * Def.speed : Vector2.zero);
                    if (dist <= Def.attackRange && cooldown <= 0f) { atk = AtkState.Windup; atkTimer = 0.45f; revealTimer = 1.0f; }
                    break;
                case AtkState.Windup:
                    MoveVelocity(Vector2.zero);
                    atkTimer -= dt;
                    if (atkTimer <= 0f)
                    {
                        var p = Player.Current;
                        if (p != null && ((Vector2)p.Position - Position).magnitude <= Def.attackRange * 1.25f) DamagePlayer(Def.damage, dir);
                        atk = AtkState.Recover; atkTimer = 0.35f;
                    }
                    break;
                default:
                    MoveVelocity(Vector2.zero);
                    atkTimer -= dt;
                    if (atkTimer <= 0f) { atk = AtkState.Idle; cooldown = Def.attackCooldown; }
                    break;
            }
        }

        void ThinkShooter(float dt, Vector2 dir, float dist)
        {
            FaceToward(dir, dt);
            float preferred = Def.attackRange * 0.7f;
            Vector2 move = Vector2.zero;
            if (atk == AtkState.Idle)
            {
                if (dist > preferred + 1f) move = dir; else if (dist < preferred - 2f) move = -dir;
                else move = new Vector2(-dir.y, dir.x) * Mathf.Sign(Mathf.Sin(Time.time * 0.7f + GetInstanceID()));
                MoveVelocity(move * Def.speed);
                if (dist <= Def.attackRange && cooldown <= 0f) { atk = AtkState.Windup; atkTimer = 0.4f; }
            }
            else
            {
                MoveVelocity(Vector2.zero);
                atkTimer -= dt;
                if (atkTimer <= 0f) { FireAt(dir, 1); atk = AtkState.Idle; cooldown = Def.attackCooldown; }
            }
        }

        void ThinkTurret(float dt, Vector2 dir, float dist)
        {
            MoveVelocity(Vector2.zero);
            FaceToward(dir, dt);
            if (atk == AtkState.Idle)
            {
                if (dist <= Def.attackRange && cooldown <= 0f) { atk = AtkState.Windup; atkTimer = 0.6f; }
            }
            else
            {
                atkTimer -= dt;
                if (atkTimer <= 0f) { FireAt(dir, 3); atk = AtkState.Idle; cooldown = Def.attackCooldown; }
            }
        }

        void ThinkCharger(float dt, Vector2 dir, float dist)
        {
            switch (atk)
            {
                case AtkState.Idle:
                    FaceToward(dir, dt);
                    MoveVelocity(dir * Def.speed);
                    if (dist < 8f && cooldown <= 0f) { atk = AtkState.Windup; atkTimer = 0.7f; lockedDir = dir; }
                    break;
                case AtkState.Windup:
                    MoveVelocity(Vector2.zero);
                    lockedDir = Vec2Util.RotateToward(lockedDir, dir, 1.5f * dt);
                    Facing = lockedDir;
                    atkTimer -= dt;
                    if (atkTimer <= 0f) { atk = AtkState.Strike; atkTimer = 0.5f; hitThisStrike = false; }
                    break;
                case AtkState.Strike:
                    MoveVelocity(lockedDir * Def.speed * 3.6f);
                    atkTimer -= dt;
                    var p = Player.Current;
                    if (!hitThisStrike && p != null && ((Vector2)p.Position - Position).magnitude <= Def.radius + 0.6f) { hitThisStrike = true; DamagePlayer(Def.damage, lockedDir); }
                    if (atkTimer <= 0f) { atk = AtkState.Recover; atkTimer = 0.8f; }
                    break;
                default:
                    MoveVelocity(Vector2.zero);
                    atkTimer -= dt;
                    if (atkTimer <= 0f) { atk = AtkState.Idle; cooldown = Def.attackCooldown; }
                    break;
            }
        }

        // ---------- helpers shared with Boss ----------
        protected void DamagePlayer(float amount, Vector2 dir)
        {
            var p = Player.Current;
            if (p == null) return;
            p.Health.TakeDamage(new DamageInfo { amount = amount, source = gameObject, sourceName = Def.name, direction = dir, isMelee = true });
        }

        protected void FireAt(Vector2 dir, int count, float spread = 14f)
        {
            for (int i = 0; i < count; i++)
            {
                float off = count == 1 ? 0f : Mathf.Lerp(-spread, spread, i / (float)(count - 1));
                FireDir(Vec2Util.Rotate(dir, off));
            }
        }

        protected void FireDir(Vector2 dir, float speedMul = 1f, float dmgMul = 1f)
        {
            Projectile.Spawn(Position + dir * (Def.radius + 0.2f), dir, false,
                new DamageInfo { amount = Def.damage * dmgMul, source = gameObject, sourceName = Def.name, isRanged = true },
                Def.projectileSpeed * speedMul, 0.22f, Def.color, 5f);
        }

        void HandleDeath(DamageInfo killingBlow)
        {
            bool burst = Status.IsStarBursted;
            Active.Remove(this);
            GameEvents.RaiseEnemyKilled(this);
            RunDrops.Roll(this);
            if (burst) StarBurst.Detonate(Position, Health.max);
            Destroy(gameObject);
        }
    }
}
