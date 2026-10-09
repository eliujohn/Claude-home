using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>5. The Sovereign of the Endless Horizon — Cosmos Seeker.</summary>
    public class EndlessHorizonBehaviour : SponsorBehaviour
    {
        public override bool GrantsTrueSight => true;
        public override bool RevealsMap => true;
        public override void ModifyOutgoing(DamageInfo d) { if (d.isRanged) d.statuses |= StatusType.StellarBurn; }

        public override void ModifyProjectile(Projectile p)
        {
            p.Homing = true;
            p.BendAroundWalls = true;                 // target-lock that curves around solid objects
            p.TurnRate = 520f + 120f * Rank;
            p.Target = Enemy.Nearest(P.Controller.AimPoint, 14f);
            p.SetColor(Def.constellationColor);
        }
    }

    /// <summary>6. The Primordial Flame of Genesis — Star Core.</summary>
    public class PrimordialFlameBehaviour : SponsorBehaviour
    {
        readonly List<OrbitingSun> suns = new List<OrbitingSun>();

        public override void OnEquip() => Rebuild();
        public override void OnRankChanged() => Rebuild();
        public override void OnUnequip() { foreach (var s in suns) if (s != null) Object.Destroy(s.gameObject); suns.Clear(); }

        void Rebuild()
        {
            OnUnequip();
            int n = Mathf.Min(6, 2 + Rank);
            for (int i = 0; i < n; i++) suns.Add(OrbitingSun.Create(P.transform, i * 360f / n, 5f * Scale, Def.constellationColor));
        }
    }

    /// <summary>7. The Unyielding Aegis of the Zenith — Bastion.</summary>
    public class UnyieldingAegisBehaviour : SponsorBehaviour
    {
        float stored;
        float Threshold => Mathf.Max(30f, P.Health.max * 0.35f);

        public override void ModifyIncoming(DamageInfo d)
        {
            float absorbed = d.amount * 0.35f;
            d.amount -= absorbed;
            stored += absorbed * Scale;
            if (stored >= Threshold) Release();
        }

        public override void OnSpecial() { if (stored > 8f) Release(); }

        readonly List<Enemy> buf = new List<Enemy>(64);
        void Release()
        {
            float dmg = stored * 1.5f;
            stored = 0f;
            Fx.Pulse(P.Position, 20f, new Color(0.95f, 0.9f, 0.7f, 0.7f), 0.7f);
            CameraRig.Shake(0.5f);
            Enemy.Snapshot(buf);
            foreach (var e in buf)
            {
                if (e == null || e.Health.IsDead) continue;
                Vector2 pull = (P.Position - e.Position);
                e.Push(pull.normalized * Mathf.Min(14f, pull.magnitude * 1.2f));
                var info = new DamageInfo { amount = dmg, fromPlayer = true, source = P.gameObject, sourceName = "Aegis", statuses = StatusType.Stun, statusDuration = 2.5f * Scale, direction = -pull.normalized };
                P.Combat.DealDamage(e, info, e.Position);
                e.Status.Apply(StatusType.Stun, 2.5f * Scale, P.gameObject);
            }
        }

        public override string HudStatus => "Stored " + Mathf.RoundToInt(stored) + "/" + Mathf.RoundToInt(Threshold);
    }

    /// <summary>8. The Abyssal Deep-Sea Horror — Leviathan.</summary>
    public class AbyssalHorrorBehaviour : SponsorBehaviour
    {
        float cd = 4f;
        float Cooldown => 10f / Scale;

        public override void Tick(float dt)
        {
            if (GameManager.State != GameFlow.Run) return;
            cd -= dt;
            if (cd <= 0f && Enemy.Active.Count >= 3)
            {
                Vector2 sum = Vector2.zero; int n = 0;
                foreach (var e in Enemy.Active) if (e != null && !e.Health.IsDead) { sum += e.Position; n++; }
                if (n > 0) Cast(sum / n);
            }
        }

        public override void OnSpecial() { if (cd <= 0f) Cast(P.Controller.AimPoint); }

        void Cast(Vector2 pos)
        {
            cd = Cooldown;
            GravityVortex.Spawn(pos, 4.5f * Scale, 9f, Def.constellationColor);
        }

        public override string HudStatus => cd > 0f ? "Vortex " + cd.ToString("0.0") + "s" : "Vortex READY [Q]";
    }

    /// <summary>9. The Clockwork Architect of Fate — Chronomancer.</summary>
    public class ClockworkFateBehaviour : SponsorBehaviour
    {
        struct Snap { public Vector2 pos; public float hp; }
        readonly Queue<Snap> history = new Queue<Snap>(40);
        float sampleClock;
        int usesLeft;

        public override void OnEquip() { GlobalTime.EnemyTimeScale = 0.8f; usesLeft = 1; }
        public override void OnRankChanged() { if (Rank >= 3 && usesLeft == 0 && State != SponsorState.Spent) usesLeft = 1; }
        public override void OnUnequip() => GlobalTime.Reset();

        public override void Tick(float dt)
        {
            sampleClock += dt;
            if (sampleClock < 0.1f) return;
            sampleClock = 0f;
            history.Enqueue(new Snap { pos = P.Position, hp = P.Health.current });
            while (history.Count > 30) history.Dequeue();      // 30 samples x 0.1s = 3 seconds
        }

        public override bool InterceptFatal(DamageInfo d)
        {
            if (usesLeft <= 0) return false;
            usesLeft--;
            if (usesLeft <= 0) State = SponsorState.Spent;
            Snap s = history.Count > 0 ? history.Peek() : new Snap { pos = P.Position, hp = P.Health.max * 0.5f };
            P.Body.position = s.pos;
            P.transform.position = s.pos;
            P.Health.current = Mathf.Max(s.hp, P.Health.max * 0.3f);
            history.Clear();
            for (int i = Projectile.Active.Count - 1; i >= 0; i--)
                if (Projectile.Active[i] != null && !Projectile.Active[i].Friendly) Object.Destroy(Projectile.Active[i].gameObject);
            P.StartCoroutine(P.Grace(1.5f));
            Fx.Pulse(s.pos, 5f, new Color(0.9f, 0.8f, 0.4f, 0.9f), 0.7f);
            CameraRig.Shake(0.4f);
            return true;
        }

        public override string HudStatus => usesLeft > 0 ? "Rewind ready" : "Rewind spent";
    }
}
