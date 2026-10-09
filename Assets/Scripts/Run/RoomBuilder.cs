using UnityEngine;

namespace Ascension
{
    /// <summary>Builds arena geometry, scenery and biome hazards for a room.</summary>
    public static class RoomBuilder
    {
        public static void Build(RoomController room, BiomeDef b, System.Random rng, bool hostile)
        {
            var root = room.transform;
            Rect a = room.Arena;

            BuildShell(root, a, b.floor, b.wall);

            if (!hostile) return;

            for (int i = 0; i < b.pillars; i++)
            {
                Vector2 p = RandomInterior(room, rng, 3f);
                var w = Wall(root, p, new Vector2(1.4f, 1.4f), Color.Lerp(b.wall, b.accent, 0.15f));
                w.gameObject.AddComponent<YSort>();
                w.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }

            for (int i = 0; i < b.hazards; i++)
            {
                var go = new GameObject(b.lasers ? "Laser" : "Magma");
                go.transform.SetParent(root, false);
                var hz = go.AddComponent<HazardZone>();
                if (b.lasers)
                {
                    bool horizontal = rng.Next(2) == 0;
                    hz.hazardName = "Laser Matrix";
                    hz.size = horizontal ? new Vector2(a.width, 0.9f) : new Vector2(0.9f, a.height);
                    hz.damage = 14f; hz.tickRate = 0.35f; hz.period = 5f; hz.telegraph = 1.1f; hz.onTime = 1.4f; hz.phaseOffset = (float)rng.NextDouble() * 5f;
                    go.transform.position = horizontal ? new Vector2(a.center.x, Mathf.Lerp(a.yMin + 3f, a.yMax - 3f, (float)rng.NextDouble())) : new Vector2(Mathf.Lerp(a.xMin + 3f, a.xMax - 3f, (float)rng.NextDouble()), a.center.y);
                    hz.Setup(new Color(1f, 0.95f, 0.5f));
                }
                else
                {
                    hz.hazardName = "Magma";
                    float s = 2.5f + (float)rng.NextDouble() * 1.5f;
                    hz.size = new Vector2(s, s); hz.damage = 8f; hz.tickRate = 0.5f;
                    go.transform.position = RandomInterior(room, rng, 3.5f);
                    hz.Setup(new Color(1f, 0.35f, 0.05f));
                }
            }

            int urns = rng.Next(3, 7);
            for (int i = 0; i < urns; i++)
            {
                Vector2 p = RandomInterior(room, rng, 2.2f);
                var u = SpriteFactory.Child(root, "Urn", SpriteFactory.Square, new Color(0.55f, 0.4f, 0.28f), new Vector2(0.7f, 0.7f), 1);
                u.transform.position = p;
                var col = u.gameObject.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;
                u.gameObject.AddComponent<Breakable>();
                u.gameObject.AddComponent<YSort>();
            }
        }


        /// <summary>Floor tiles + four walls. Shared by combat rooms and the Abyss Terminal hub.</summary>
        public static void BuildShell(Transform root, Rect a, Color floor, Color wall)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 6; j++)
                {
                    float shade = (i + j) % 2 == 0 ? 1f : 0.93f;
                    var c = new Color(floor.r * shade, floor.g * shade, floor.b * shade);
                    var t = SpriteFactory.Child(root, "Tile", SpriteFactory.Square, c, new Vector2(4f, 4f), -10000);
                    t.transform.position = new Vector3(a.xMin - 2f + 4f * i + 2f, a.yMin - 2f + 4f * j + 2f, 0f);
                }
            Wall(root, new Vector2(a.center.x, a.yMax + 1.5f), new Vector2(a.width + 6f, 3f), wall);
            Wall(root, new Vector2(a.center.x, a.yMin - 1.5f), new Vector2(a.width + 6f, 3f), wall);
            Wall(root, new Vector2(a.xMin - 1.5f, a.center.y), new Vector2(3f, a.height + 6f), wall);
            Wall(root, new Vector2(a.xMax + 1.5f, a.center.y), new Vector2(3f, a.height + 6f), wall);
        }

        public static Vector2 RandomInterior(RoomController room, System.Random rng, float margin)
        {
            Rect a = room.Arena;
            for (int tries = 0; tries < 30; tries++)
            {
                var p = new Vector2(Mathf.Lerp(a.xMin + margin, a.xMax - margin, (float)rng.NextDouble()), Mathf.Lerp(a.yMin + margin, a.yMax - margin, (float)rng.NextDouble()));
                if (Vector2.Distance(p, room.PlayerSpawn) > 5f && p.y < a.yMax - 3.5f) return p;
            }
            return new Vector2(a.center.x, a.center.y + 2f);
        }

        public static Transform Wall(Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            var sr = SpriteFactory.Child(parent, "Wall", SpriteFactory.Square, color, size, -9500);
            sr.transform.position = pos;
            sr.gameObject.layer = Layers.Obstacle;
            var col = sr.gameObject.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            sr.gameObject.AddComponent<Obstacle>();
            return sr.transform;
        }
    }
}
