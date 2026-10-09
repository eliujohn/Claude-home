using UnityEngine;

namespace Ascension
{
    /// <summary>Tiny sprite-based effects: expanding pulses and melee slashes.</summary>
    public class FxAnim : MonoBehaviour
    {
        public float duration = 0.3f, startScale = 0.2f, endScale = 1f;
        public Color color = Color.white;
        float t;
        SpriteRenderer sr;

        void Awake() => sr = GetComponent<SpriteRenderer>();

        void Update()
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float s = Mathf.Lerp(startScale, endScale, 1f - (1f - k) * (1f - k));
            transform.localScale = new Vector3(s, s, 1f);
            var c = color; c.a = color.a * (1f - k);
            sr.color = c;
            if (k >= 1f) Destroy(gameObject);
        }
    }

    public static class Fx
    {
        public static void Pulse(Vector2 pos, float radius, Color color, float duration = 0.35f)
        {
            Spawn(SpriteFactory.Circle, pos, 0f, radius * 2f, color, duration, 5000);
        }

        public static void Slash(Vector2 pos, Vector2 dir, float range, Color color)
        {
            Spawn(SpriteFactory.Arc, pos, Vec2Util.Angle(dir), range * 2f, color, 0.14f, 5001, 0.7f);
        }

        static void Spawn(Sprite sprite, Vector2 pos, float angle, float endDiameter, Color color, float duration, int order, float startFrac = 0.15f)
        {
            var go = new GameObject("Fx");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
            var sr = SpriteFactory.Attach(go, sprite, color, order);
            var fx = go.AddComponent<FxAnim>();
            fx.duration = duration; fx.color = color;
            fx.startScale = endDiameter * startFrac; fx.endScale = endDiameter;
            sr.color = color;
        }
    }
}
