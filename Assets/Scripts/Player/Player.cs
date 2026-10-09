using UnityEngine;

namespace Ascension
{
    /// <summary>Facade over Alistair the Defiant. Sponsors and systems talk to this, not to individual components.</summary>
    [RequireComponent(typeof(Health), typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        public static Player Current { get; private set; }

        public Health Health { get; private set; }
        public Rigidbody2D Body { get; private set; }
        public PlayerController Controller { get; private set; }
        public PlayerCombat Combat { get; private set; }
        public SponsorManager Sponsors { get; private set; }
        public RunState Run = new RunState(0);
        public PlayerStats Stats = new PlayerStats();
        public SpriteRenderer Sprite { get; private set; }

        float invisibleTimer;
        public bool IsInvisible => invisibleTimer > 0f;
        public bool TrueSight => Sponsors != null && Sponsors.GrantsTrueSight;
        public Vector2 Position => Body.position;

        public static Player Create(Vector2 pos)
        {
            var go = new GameObject("Alistair the Defiant");
            go.layer = Layers.Player;
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.8f;
            var p = go.AddComponent<Player>();
            return p;
        }

        void Awake()
        {
            Current = this;
            Health = GetComponent<Health>();
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f; Body.freezeRotation = true;
            var col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            Sprite = SpriteFactory.Attach(gameObject, SpriteFactory.Circle, new Color(0.92f, 0.92f, 1f), 5);
            gameObject.AddComponent<YSort>();
            Sponsors = gameObject.AddComponent<SponsorManager>();
            Controller = gameObject.AddComponent<PlayerController>();
            Combat = gameObject.AddComponent<PlayerCombat>();
        }

        void Start()
        {
            // Registered after SponsorManager's hook so free per-run saves (Chronomancer) fire before meta lives.
            Health.AddFatalHook(DeathDefiance);
            Health.Died += info => GameEvents.RaisePlayerDied(info);
        }

        void OnDestroy() { if (Current == this) Current = null; }

        public void RefreshStats(bool fillHealth)
        {
            Stats = PlayerStats.FromMeta();
            Health.Init(Stats.maxHp);
            Health.invulnerable = false;
        }

        public void SetInvisible(float seconds) => invisibleTimer = Mathf.Max(seconds <= 0f ? 0f : invisibleTimer, seconds);

        void Update()
        {
            if (invisibleTimer > 0f) invisibleTimer -= Time.deltaTime;
            var c = Sprite.color; c.a = IsInvisible ? 0.3f : 1f; Sprite.color = c;
        }

        /// <summary>All healing routes through here so sponsors can reinterpret it (e.g. vials -> Blood Frenzy).</summary>
        public float Heal(float amount, bool fromItem)
        {
            Sponsors.ModifyHeal(ref amount, fromItem);
            return Health.Heal(amount);
        }

        bool DeathDefiance(DamageInfo d)
        {
            if (Run.defiancesLeft <= 0) return false;
            Run.defiancesLeft--;
            Health.current = Health.max * 0.5f;
            StartCoroutine(Grace(1.5f));
            Fx.Pulse(Position, 5f, new Color(1f, 0.85f, 0.4f, 0.9f), 0.6f);
            CameraRig.Shake(0.4f);
            return true;
        }

        public System.Collections.IEnumerator Grace(float seconds)
        {
            Health.invulnerable = true;
            yield return new WaitForSeconds(seconds);
            Health.invulnerable = false;
        }
    }
}
