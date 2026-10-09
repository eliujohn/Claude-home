using UnityEngine;

namespace Ascension
{
    /// <summary>Cosmic status ailments on an enemy. Two distinct cosmic ailments = Star-Bursted State.</summary>
    public class StatusController : MonoBehaviour
    {
        static readonly StatusType[] Slots =
        {
            StatusType.GraveBound, StatusType.StellarBurn, StatusType.Bleed, StatusType.Ignite,
            StatusType.Corroded, StatusType.Crushed, StatusType.Stun, StatusType.Slow
        };

        // damage per second for damaging ailments
        static float Dps(StatusType t)
        {
            switch (t)
            {
                case StatusType.StellarBurn: return 7f;
                case StatusType.Bleed: return 5f;
                case StatusType.Ignite: return 9f;
                case StatusType.Crushed: return 10f;
                default: return 0f;
            }
        }

        readonly float[] remaining = new float[Slots.Length];
        Health health;
        float tickClock;
        public GameObject LastSource;

        void Awake() => health = GetComponent<Health>();

        public bool Has(StatusType t)
        {
            for (int i = 0; i < Slots.Length; i++) if (Slots[i] == t) return remaining[i] > 0f;
            return false;
        }

        public bool Stunned => Has(StatusType.Stun);

        public int CosmicCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < Slots.Length; i++)
                    if ((Slots[i] & StatusType.CosmicMask) != 0 && remaining[i] > 0f) n++;
                return n;
            }
        }

        public bool IsStarBursted => CosmicCount >= 2;

        public float SpeedMultiplier
        {
            get
            {
                if (Stunned) return 0f;
                float m = 1f;
                if (Has(StatusType.Slow)) m *= 0.5f;
                if (Has(StatusType.GraveBound)) m *= 0.8f;
                if (Has(StatusType.Crushed)) m *= 0.6f;
                return m;
            }
        }

        public void Apply(StatusType flags, float duration, GameObject source = null)
        {
            if (flags == StatusType.None) return;
            if (source != null) LastSource = source;
            for (int i = 0; i < Slots.Length; i++)
                if ((flags & Slots[i]) != 0) remaining[i] = Mathf.Max(remaining[i], duration);
        }

        public void Clear() { for (int i = 0; i < remaining.Length; i++) remaining[i] = 0f; }

        void Update()
        {
            if (health == null || health.IsDead) return;
            float dt = Time.deltaTime;
            tickClock += dt;
            bool tick = tickClock >= 0.5f;
            float tickDt = tickClock;
            if (tick) tickClock = 0f;

            health.armorDissolved = remaining[4] > 0f; // Corroded

            for (int i = 0; i < Slots.Length; i++)
            {
                if (remaining[i] <= 0f) continue;
                remaining[i] -= dt;
                if (tick && Dps(Slots[i]) > 0f && !health.IsDead)
                {
                    health.TakeDamage(new DamageInfo
                    {
                        amount = Dps(Slots[i]) * tickDt, fromPlayer = true, ignoreArmor = true,
                        isStatusTick = true, sourceName = Slots[i].ToString(), source = LastSource
                    });
                }
            }
        }
    }
}
