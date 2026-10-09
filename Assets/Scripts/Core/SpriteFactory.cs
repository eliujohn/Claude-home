using UnityEngine;

namespace Ascension
{
    /// <summary>Procedural placeholder art so the prototype runs with zero imported assets.</summary>
    public static class SpriteFactory
    {
        static Sprite circle, square, arc;

        public static Sprite Circle => circle != null ? circle : circle = Build(64, (x, y, r) => Mathf.Clamp01(r - Mathf.Sqrt(x * x + y * y)));
        public static Sprite Square => square != null ? square : square = Build(8, (x, y, r) => 1f);

        /// <summary>120 degree wedge pointing +X, diameter = 1 unit.</summary>
        public static Sprite Arc => arc != null ? arc : arc = Build(128, (x, y, r) =>
        {
            float d = Mathf.Sqrt(x * x + y * y);
            if (d > r || d < r * 0.35f) return 0f;
            float ang = Mathf.Abs(Mathf.Atan2(y, x) * Mathf.Rad2Deg);
            return ang <= 60f ? 1f - Mathf.Clamp01((d - r * 0.35f) / (r * 0.65f)) * 0.6f : 0f;
        });

        delegate float Shader(float x, float y, float radius);

        static Sprite Build(int size, Shader shade)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float a = shade(x + 0.5f - r, y + 0.5f - r, r);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a) * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static SpriteRenderer Attach(GameObject go, Sprite sprite, Color color, int order = 0)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        /// <summary>Creates a child object with a sprite, sized in world units.</summary>
        public static SpriteRenderer Child(Transform parent, string name, Sprite sprite, Color color, Vector2 size, int order = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            return Attach(go, sprite, color, order);
        }
    }
}
