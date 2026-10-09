using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>Melee arc + ranged shot. All outgoing damage flows through Build() and DealDamage() so sponsors can hook it.</summary>
    public class PlayerCombat : MonoBehaviour
    {
        Player player;
        float meleeCd, rangedCd;
        readonly List<Enemy> buffer = new List<Enemy>(32);

        void Awake() => player = GetComponent<Player>();

        void Update()
        {
            if (GameManager.InputLocked || player.Health.IsDead) return;
            meleeCd -= Time.deltaTime; rangedCd -= Time.deltaTime;
            if (GameManager.State != GameFlow.Run) return;      // no fighting in the hub

            if (InputReader.SpecialPressed) player.Sponsors.NotifySpecial();
            if (InputReader.MeleeHeld && meleeCd <= 0f) Melee();
            else if (InputReader.RangedHeld && rangedCd <= 0f) Ranged();
        }

        DamageInfo Build(bool melee)
        {
            var st = player.Stats;
            var d = new DamageInfo
            {
                amount = (melee ? st.meleeDamage : st.rangedDamage) * st.damageMult * player.Sponsors.StaticDamageMod,
                source = gameObject, sourceName = "Alistair", fromPlayer = true,
                isMelee = melee, isRanged = !melee, direction = player.Controller.Aim
            };
            player.Sponsors.ModifyOutgoing(d);
            return d;
        }

        void Melee()
        {
            var st = player.Stats;
            meleeCd = st.meleeCooldown * player.Sponsors.MeleeCooldownMultiplier;
            Vector2 aim = player.Controller.Aim, pos = player.Position;
            player.Sponsors.BeginAttack();
            var template = Build(true);
            Fx.Slash(pos + aim * 0.3f, aim, st.meleeRange, new Color(1f, 1f, 1f, 0.75f));

            float halfArc = st.meleeArc * 0.5f;
            Enemy.Query(pos, st.meleeRange + 1.5f, buffer);
            bool hitAny = false;
            for (int i = 0; i < buffer.Count; i++)
            {
                var e = buffer[i];
                if (e == null || e.Health.IsDead) continue;
                Vector2 to = e.Position - pos;
                if (to.magnitude - e.Def.radius > st.meleeRange) continue;
                if (Vector2.Angle(aim, to) > halfArc) continue;
                if (DealDamage(e, template, e.Position) > 0f) hitAny = true;
            }
            for (int i = Breakable.Active.Count - 1; i >= 0; i--)
            {
                var b = Breakable.Active[i];
                if (b == null) continue;
                Vector2 to = (Vector2)b.transform.position - pos;
                if (to.magnitude <= st.meleeRange && Vector2.Angle(aim, to) <= halfArc) b.Break();
            }
            if (hitAny) CameraRig.Shake(0.06f);
            player.Sponsors.EndAttack();
        }

        void Ranged()
        {
            if (!player.Sponsors.AllowsRanged || !player.Sponsors.TryPayRangedCost()) { rangedCd = 0.15f; return; }
            var st = player.Stats;
            rangedCd = st.rangedCooldown;
            Vector2 aim = player.Controller.Aim;
            player.Sponsors.BeginAttack();
            var info = Build(false);
            var proj = Projectile.Spawn(player.Position + aim * 0.7f, aim, true, info, st.rangedSpeed, 0.2f, new Color(0.85f, 0.95f, 1f), 2.2f);
            player.Sponsors.ModifyProjectile(proj);
            player.Sponsors.EndAttack();
        }

        /// <summary>Single choke point for every hit Alistair (or his proxies) lands on an enemy.</summary>
        public float DealDamage(Enemy e, DamageInfo template, Vector2 hitPoint)
        {
            if (e == null || !e.CanBeHit) return 0f;
            var d = template.Clone();
            // Enemy facing roughly the same way we are attacking => we are behind it.
            d.isBackstab = d.direction.sqrMagnitude > 0.01f && Vector2.Dot(e.Facing, d.direction.normalized) > 0.35f;
            player.Sponsors.ModifyVsTarget(e, d);
            float dealt = e.Hit(d);
            if (dealt > 0f) player.Sponsors.NotifyHitDealt(e, d, dealt);
            return dealt;
        }
    }
}
