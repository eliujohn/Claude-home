using UnityEngine;

namespace Ascension
{
    /// <summary>1. The Monarch of the Bone Throne — Necromancer.</summary>
    public class BoneThroneBehaviour : SponsorBehaviour
    {
        public override void ModifyOutgoing(DamageInfo d) => d.statuses |= StatusType.GraveBound;

        public override void OnEnemyKilled(Enemy e)
        {
            if (e.IsBoss || GameManager.State != GameFlow.Run) return;
            if (SpectralThrall.Count >= 4 + 2 * Rank) return;
            SpectralThrall.Spawn(e.Position, 10f + 2f * Rank, 1f + 0.35f * (Rank - 1));
        }

        public override void OnUnequip() => SpectralThrall.DestroyAll();
        public override void OnRoomCleared(RoomController room) { }
        public override string HudStatus => SpectralThrall.Count > 0 ? "Thralls " + SpectralThrall.Count : null;
    }

    /// <summary>2. The Weaver of Fatal Paradoxes — Trickster.</summary>
    public class FatalParadoxBehaviour : SponsorBehaviour
    {
        float Missing => 1f - P.Health.Fraction;

        public override void ModifyOutgoing(DamageInfo d) => d.amount *= 1f + Missing * 2f * Scale;
        public override float MoveSpeedMultiplier => 1f + Missing * 0.35f * Scale;
        public override bool DashGivesIFrames => false;   // replaced by probability manipulation

        public override void ModifyIncoming(DamageInfo d)
        {
            float chance = Mathf.Min(0.75f, 0.2f + Missing * 0.55f * Scale);
            if (Random.value < chance)
            {
                d.negated = true;
                Fx.Pulse(P.Position, 1.2f, new Color(0.75f, 0.4f, 1f, 0.8f), 0.25f);
            }
        }

        public override string HudStatus => "Paradox " + Mathf.RoundToInt(Mathf.Min(0.75f, 0.2f + Missing * 0.55f * Scale) * 100f) + "%";
    }

    /// <summary>3. The Scribe of Bloodlines — Vampire.</summary>
    public class BloodlineScribeBehaviour : SponsorBehaviour
    {
        int stacks;
        float timer;

        public override void OnHeal(ref float amount, bool fromItem)
        {
            if (!fromItem) return;
            stacks = Mathf.Min(stacks + 1, 8);
            timer = 20f;
            amount = 0f;                              // healing items become damage instead
            Fx.Pulse(P.Position, 1.5f, new Color(0.85f, 0.1f, 0.25f, 0.8f), 0.3f);
        }

        public override void Tick(float dt)
        {
            if (timer <= 0f) return;
            timer -= dt;
            if (timer <= 0f) stacks = 0;
        }

        public override void ModifyOutgoing(DamageInfo d)
        {
            d.amount *= 1f + 0.6f * stacks * Scale;
            d.statuses |= StatusType.Bleed;
        }

        public override void OnHitDealt(Enemy target, DamageInfo d, float dealt) => P.Health.Heal(dealt * 0.06f * Scale);
        public override string HudStatus => stacks > 0 ? "Frenzy x" + stacks : null;
    }

    /// <summary>4. The Executioner of the Quiet Void — Assassin.</summary>
    public class VoidExecutionerBehaviour : SponsorBehaviour
    {
        public override void OnDash() => P.SetInvisible(2.5f * Scale);

        public override void ModifyVsTarget(Enemy target, DamageInfo d)
        {
            if (!d.isBackstab) return;
            d.amount *= 5f;                           // 500% critical
            d.isCrit = true;
        }

        public override void OnHitDealt(Enemy target, DamageInfo d, float dealt)
        {
            if (d.isMelee) P.SetInvisible(0f);        // striking breaks the veil
        }

        public override string HudStatus => P.IsInvisible ? "Unseen" : null;
    }
}
