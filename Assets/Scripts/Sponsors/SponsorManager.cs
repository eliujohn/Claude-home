using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>
    /// Runtime state machine for equipped Constellations. Re-equipping an owned sponsor ranks it up.
    /// Dispatches every gameplay hook to all active behaviours.
    /// </summary>
    public class SponsorManager : MonoBehaviour
    {
        readonly List<SponsorBehaviour> list = new List<SponsorBehaviour>(15);
        Player player;
        Health health;

        public IReadOnlyList<SponsorBehaviour> Active => list;

        void Awake()
        {
            health = GetComponent<Health>();
            health.BeforeDamage += OnIncoming;
            health.AddFatalHook(OnFatal);
        }

        void OnEnable()
        {
            GameEvents.EnemyKilled += OnEnemyKilled;
            GameEvents.RoomEntered += OnRoomEntered;
            GameEvents.RoomCleared += OnRoomCleared;
        }

        void OnDisable()
        {
            GameEvents.EnemyKilled -= OnEnemyKilled;
            GameEvents.RoomEntered -= OnRoomEntered;
            GameEvents.RoomCleared -= OnRoomCleared;
        }

        Player Owner => player != null ? player : player = GetComponent<Player>();

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < list.Count; i++) list[i].Tick(dt);
        }

        // ---------- membership ----------
        public bool Has(ConstellationId id) => Find(id) != null;
        public int RankOf(ConstellationId id) { var b = Find(id); return b == null ? 0 : b.Rank; }
        public SponsorBehaviour Find(ConstellationId id) { for (int i = 0; i < list.Count; i++) if (list[i].Id == id) return list[i]; return null; }
        public T Get<T>() where T : SponsorBehaviour { for (int i = 0; i < list.Count; i++) if (list[i] is T t) return t; return null; }

        public SponsorBehaviour Equip(ConstellationId id)
        {
            var existing = Find(id);
            var def = ConstellationDatabase.Get(id);
            if (existing != null)
            {
                if (existing.Rank < def.maxRank) existing.SetRank(existing.Rank + 1);
                return existing;
            }
            var b = SponsorFactory.Create(id);
            b.Bind(def, Owner);
            list.Add(b);
            b.OnEquip();
            return b;
        }

        public void Clear()
        {
            for (int i = list.Count - 1; i >= 0; i--) list[i].OnUnequip();
            list.Clear();
            GlobalTime.Reset();
        }

        // ---------- aggregate policy ----------
        public bool AllowsRanged { get { for (int i = 0; i < list.Count; i++) if (!list[i].AllowsRanged) return false; return true; } }
        public bool DashGivesIFrames { get { for (int i = 0; i < list.Count; i++) if (!list[i].DashGivesIFrames) return false; return true; } }
        public bool GrantsTrueSight { get { for (int i = 0; i < list.Count; i++) if (list[i].GrantsTrueSight) return true; return false; } }
        public bool RevealsMap { get { for (int i = 0; i < list.Count; i++) if (list[i].RevealsMap) return true; return false; } }

        public float MoveSpeedMultiplier
        {
            get { float m = 1f; for (int i = 0; i < list.Count; i++) m *= list[i].MoveSpeedMultiplier * list[i].Def.speedMod; return m; }
        }

        public float MeleeCooldownMultiplier
        {
            get { float m = 1f; for (int i = 0; i < list.Count; i++) m *= list[i].MeleeCooldownMultiplier; return m; }
        }

        public float StaticDamageMod
        {
            get { float m = 1f; for (int i = 0; i < list.Count; i++) m *= list[i].Def.damageMod; return m; }
        }

        // ---------- dispatch ----------
        public void BeginAttack() { for (int i = 0; i < list.Count; i++) list[i].OnAttackBegin(); }
        public void EndAttack() { for (int i = 0; i < list.Count; i++) list[i].OnAttackEnd(); }
        public void ModifyOutgoing(DamageInfo d) { for (int i = 0; i < list.Count; i++) list[i].ModifyOutgoing(d); }
        public void ModifyVsTarget(Enemy e, DamageInfo d) { for (int i = 0; i < list.Count; i++) list[i].ModifyVsTarget(e, d); }
        public void NotifyHitDealt(Enemy e, DamageInfo d, float dealt) { for (int i = 0; i < list.Count; i++) list[i].OnHitDealt(e, d, dealt); }
        public void ModifyProjectile(Projectile p) { for (int i = 0; i < list.Count; i++) list[i].ModifyProjectile(p); }
        public void NotifyDash() { for (int i = 0; i < list.Count; i++) list[i].OnDash(); }
        public void NotifySpecial() { for (int i = 0; i < list.Count; i++) list[i].OnSpecial(); }

        public bool TryPayRangedCost()
        {
            for (int i = 0; i < list.Count; i++) if (!list[i].TryPayRangedCost()) return false;
            return true;
        }

        public void ModifyHeal(ref float amount, bool fromItem)
        {
            for (int i = 0; i < list.Count; i++) list[i].OnHeal(ref amount, fromItem);
        }

        void OnIncoming(DamageInfo d) { for (int i = 0; i < list.Count && !d.negated; i++) list[i].ModifyIncoming(d); }

        bool OnFatal(DamageInfo d)
        {
            for (int i = 0; i < list.Count; i++) if (list[i].InterceptFatal(d)) return true;
            return false;
        }

        void OnEnemyKilled(Enemy e) { for (int i = 0; i < list.Count; i++) list[i].OnEnemyKilled(e); }
        void OnRoomEntered(RoomController r) { for (int i = 0; i < list.Count; i++) list[i].OnRoomEntered(r); }
        void OnRoomCleared(RoomController r) { for (int i = 0; i < list.Count; i++) list[i].OnRoomCleared(r); }
    }
}
