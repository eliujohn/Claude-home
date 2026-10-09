using UnityEngine;

namespace Ascension
{
    public enum SponsorState { Active, Charging, Cooldown, Spent }

    /// <summary>
    /// One state in the 15-way modifier machine. SponsorManager owns the instances and routes gameplay
    /// events to every equipped behaviour; behaviours never reference each other. All hooks are no-ops by default
    /// so each sponsor overrides only what it changes.
    /// </summary>
    public abstract class SponsorBehaviour
    {
        public ConstellationSponsor Def { get; private set; }
        public Player P { get; private set; }
        public int Rank { get; private set; } = 1;
        public SponsorState State { get; protected set; } = SponsorState.Active;
        public ConstellationId Id => Def.id;

        /// <summary>Rank scalar: +35% potency per extra rank.</summary>
        protected float Scale => 1f + 0.35f * (Rank - 1);

        internal void Bind(ConstellationSponsor def, Player player) { Def = def; P = player; }
        internal void SetRank(int rank) { Rank = rank; OnRankChanged(); }

        // lifecycle
        public virtual void OnEquip() { }
        public virtual void OnUnequip() { }
        public virtual void OnRankChanged() { }
        public virtual void Tick(float dt) { }

        // attack pipeline
        public virtual void OnAttackBegin() { }
        public virtual void OnAttackEnd() { }
        public virtual void ModifyOutgoing(DamageInfo d) { }                  // at swing/fire time, no target yet
        public virtual void ModifyVsTarget(Enemy target, DamageInfo d) { }    // per victim (backstabs etc.)
        public virtual void OnHitDealt(Enemy target, DamageInfo d, float dealt) { }
        public virtual void ModifyProjectile(Projectile p) { }
        public virtual bool TryPayRangedCost() => true;

        // defence / world
        public virtual void ModifyIncoming(DamageInfo d) { }
        public virtual bool InterceptFatal(DamageInfo d) => false;
        public virtual void OnHeal(ref float amount, bool fromItem) { }
        public virtual void OnDash() { }
        public virtual void OnSpecial() { }
        public virtual void OnEnemyKilled(Enemy e) { }
        public virtual void OnRoomEntered(RoomController room) { }
        public virtual void OnRoomCleared(RoomController room) { }

        // policy queries
        public virtual bool AllowsRanged => true;
        public virtual bool DashGivesIFrames => true;
        public virtual float MoveSpeedMultiplier => 1f;
        public virtual float MeleeCooldownMultiplier => 1f;
        public virtual bool GrantsTrueSight => false;
        public virtual bool RevealsMap => false;

        /// <summary>Short HUD readout (stacks, charge, stored damage).</summary>
        public virtual string HudStatus => null;
    }
}
