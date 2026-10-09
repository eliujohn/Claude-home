using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>Multi-phase boss. Each phase can raise a damage-immune Shield that holds until its summoned adds die.</summary>
    public class Boss : Enemy
    {
        public static Boss Current;
        public BossDef BossDef;
        public override bool IsBoss => true;
        public int PhaseIndex { get; private set; }
        public bool ShieldActive { get; private set; }
        public override bool CanBeHit => !ShieldActive && !Health.IsDead;

        readonly List<Enemy> adds = new List<Enemy>();
        float patternTimer = 2f, shieldTimer, runTimer;
        int patternIndex, runShots;
        BossPattern? running;
        float spiralAngle;
        Vector2 chargeDir; float chargeTimer; bool charging;

        protected override void Init(EnemyDef def)
        {
            base.Init(def);
            Current = this;
            Health.armor = 0.1f;
        }

        void OnDestroy() { if (Current == this) Current = null; }

        public void BreakShield()
        {
            if (!ShieldActive) return;
            ShieldActive = false;
            Fx.Pulse(Position, 4f, new Color(1f, 0.5f, 0.7f, 0.8f), 0.5f);
            CameraRig.Shake(0.3f);
        }

        protected override void Think(float dt)
        {
            CheckPhase();

            if (ShieldActive)
            {
                shieldTimer -= dt;
                adds.RemoveAll(a => a == null || a.Health.IsDead);
                if (adds.Count == 0 || shieldTimer <= 0f) BreakShield();
                MoveVelocity(Vector2.zero);
                sprite.color = Color.Lerp(Def.color, Color.white, 0.5f + 0.5f * Mathf.PingPong(Time.time * 3f, 1f));
                return;
            }

            bool target = TryGetTarget(out Vector2 to, out float dist);
            Vector2 dir = target && dist > 0.01f ? to / dist : Facing;
            FaceToward(dir, dt);

            if (charging)
            {
                chargeTimer -= dt;
                MoveVelocity(chargeDir * Def.speed * 4.5f);
                var p = Player.Current;
                if (p != null && (p.Position - Position).magnitude < Def.radius + 0.6f)
                { DamagePlayer(Def.damage * 1.3f, chargeDir); charging = false; }
                if (chargeTimer <= 0f) charging = false;
                return;
            }

            if (running.HasValue) { RunPattern(dt, dir); return; }

            MoveVelocity(target && dist > 3.5f ? dir * Def.speed : Vector2.zero);
            patternTimer -= dt;
            if (patternTimer <= 0f && target)
            {
                var phase = BossDef.phases[PhaseIndex];
                var pat = phase.patterns[patternIndex++ % phase.patterns.Length];
                BeginPattern(pat, dir);
                patternTimer = phase.interval;
            }
        }

        void CheckPhase()
        {
            int next = PhaseIndex + 1;
            if (next < BossDef.phases.Length && Health.Fraction <= BossDef.phases[next].enterAtHpFraction)
            {
                PhaseIndex = next;
                patternIndex = 0; running = null; charging = false;
                var ph = BossDef.phases[PhaseIndex];
                Fx.Pulse(Position, 6f, Def.color, 0.6f);
                CameraRig.Shake(0.4f);
                if (ph.shieldAdds > 0)
                {
                    ShieldActive = true; shieldTimer = 14f;
                    SpawnAdds(ph.shieldAdds);
                }
            }
        }

        void SpawnAdds(int n)
        {
            var room = RoomController.Current;
            if (room == null || BossDef.addPool == null || BossDef.addPool.Length == 0) return;
            for (int i = 0; i < n; i++)
            {
                var def = BossDef.addPool[Random.Range(0, BossDef.addPool.Length)];
                Vector2 pos = Position + Random.insideUnitCircle.normalized * Random.Range(3f, 6f);
                pos = room.ClampToArena(pos, 1.5f);
                adds.Add(Spawn(def, pos, 0.8f, 0.9f));
            }
        }

        void BeginPattern(BossPattern pat, Vector2 dir)
        {
            switch (pat)
            {
                case BossPattern.AimedVolley: FireAt(dir, 5, 18f); break;
                case BossPattern.RadialBurst:
                    int n = 14 + PhaseIndex * 4;
                    float off = Random.value * 360f;
                    for (int i = 0; i < n; i++) FireDir(Vec2Util.FromAngle(off + i * 360f / n), 0.85f);
                    break;
                case BossPattern.Spiral: running = pat; runTimer = 0f; runShots = 26; spiralAngle = Random.value * 360f; break;
                case BossPattern.Charge: charging = true; chargeTimer = 0.7f; chargeDir = dir; break;
                case BossPattern.SummonAdds: SpawnAdds(2 + PhaseIndex); break;
            }
        }

        void RunPattern(float dt, Vector2 dir)
        {
            MoveVelocity(Vector2.zero);
            runTimer -= dt;
            if (runTimer > 0f) return;
            runTimer = 0.09f;
            spiralAngle += 23f;
            FireDir(Vec2Util.FromAngle(spiralAngle), 0.9f);
            FireDir(Vec2Util.FromAngle(spiralAngle + 180f), 0.9f);
            if (--runShots <= 0) running = null;
        }
    }
}
