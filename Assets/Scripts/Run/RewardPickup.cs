using UnityEngine;

namespace Ascension
{
    public class RewardPickup : MonoBehaviour
    {
        RewardType type;
        int amount;
        bool taken;

        public static RewardPickup Spawn(Transform parent, Vector2 pos, RewardType type, int amount)
        {
            var go = new GameObject("Reward " + type);
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = pos;
            float size = type == RewardType.Blessing ? 0.9f : 0.55f;
            go.transform.localScale = Vector3.one * size;
            SpriteFactory.Attach(go, type == RewardType.Vitality ? SpriteFactory.Square : SpriteFactory.Circle, ColorFor(type), 6);
            go.AddComponent<YSort>();
            var r = go.AddComponent<RewardPickup>();
            r.type = type; r.amount = amount;
            return r;
        }

        public static Color ColorFor(RewardType t)
        {
            switch (t)
            {
                case RewardType.Blessing: return new Color(0.8f, 0.55f, 1f);
                case RewardType.Gold: return new Color(1f, 0.85f, 0.2f);
                case RewardType.Stardust: return new Color(0.5f, 0.95f, 1f);
                case RewardType.Vial: return new Color(1f, 0.25f, 0.3f);
                case RewardType.Vitality: return new Color(0.4f, 1f, 0.5f);
                default: return Color.white;
            }
        }

        void Update()
        {
            float bob = 1f + 0.08f * Mathf.Sin(Time.time * 5f);
            transform.localScale = Vector3.one * (type == RewardType.Blessing ? 0.9f : 0.55f) * bob;
            var p = Player.Current;
            if (taken || p == null || p.Health.IsDead || GameManager.State != GameFlow.Run) return;
            if (((Vector2)transform.position - p.Position).magnitude > 1.2f) return;
            taken = true;
            Collect(p);
            Destroy(gameObject);
        }

        void Collect(Player p)
        {
            var run = p.Run;
            switch (type)
            {
                case RewardType.Blessing: RunManager.I.OfferBlessings(); break;
                case RewardType.Gold: run.gold += amount; break;
                case RewardType.Stardust: RunDrops.AddStardust(run, amount); break;
                case RewardType.Vial: p.Heal(p.Health.max * 0.3f, true); break;
                case RewardType.Vitality: p.Health.SetMax(p.Health.max + 10f, true); p.Health.Heal(10f); break;
            }
            Fx.Pulse(transform.position, 1.5f, ColorFor(type), 0.3f);
        }
    }
}
