using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>One chamber: builds itself, runs waves, drops its reward, then opens previewed doors.</summary>
    public class RoomController : MonoBehaviour
    {
        public static RoomController Current { get; private set; }

        public RoomNode Node { get; private set; }
        public BiomeDef Biome { get; private set; }
        public readonly Rect Arena = new Rect(-16f, -9f, 32f, 18f);
        public Vector2 PlayerSpawn => new Vector2(0f, -7.2f);
        public readonly List<Door> Doors = new List<Door>();
        public bool Cleared { get; private set; }

        RunState run;
        System.Random rng;
        int wavesTotal, waveIndex, pendingSpawns;
        bool started;
        float startDelay = 1.0f;

        public static RoomController Build(RoomNode node, BiomeDef biome, RunState run)
        {
            var go = new GameObject("Room");
            var r = go.AddComponent<RoomController>();
            r.Node = node; r.Biome = biome; r.run = run;
            r.rng = new System.Random(run.seed * 31 + node.biome * 1009 + node.layer * 97 + run.roomsCleared);
            RoomBuilder.Build(r, biome, r.rng, node.type != RoomType.Sanctuary);
            Current = r;
            r.wavesTotal = node.type == RoomType.Boss ? 1 : node.type == RoomType.Elite ? 3 : node.type == RoomType.Sanctuary ? 0 : 2;
            return r;
        }

        void Start()
        {
            var p = Player.Current;
            p.Body.position = PlayerSpawn;
            p.transform.position = PlayerSpawn;
            CameraRig.SetBounds(Arena);
            GameEvents.RaiseRoomEntered(this);
            if (Node.IsBoss) HudView.ShowBanner(Node.biome == 3 ? "THE SOVEREIGN OF JUDGMENT" : Biome.boss.name.ToUpper(), Biome.boss.title);
            else if (Node.layer == 0) HudView.ShowBanner(Biome.name, Biome.subtitle);
        }

        void OnDestroy() { if (Current == this) Current = null; }

        public Vector2 ClampToArena(Vector2 p, float margin) =>
            new Vector2(Mathf.Clamp(p.x, Arena.xMin + margin, Arena.xMax - margin), Mathf.Clamp(p.y, Arena.yMin + margin, Arena.yMax - margin));

        void Update()
        {
            if (Cleared || GameManager.State != GameFlow.Run) return;
            if (!started)
            {
                startDelay -= Time.deltaTime;
                if (startDelay > 0f) return;
                started = true;
                if (wavesTotal == 0) { Clear(); return; }
                NextWave();
                return;
            }
            if (pendingSpawns > 0) return;
            for (int i = 0; i < Enemy.Active.Count; i++) if (Enemy.Active[i] != null && !Enemy.Active[i].Health.IsDead) return;
            if (waveIndex < wavesTotal) NextWave(); else Clear();
        }

        void NextWave()
        {
            waveIndex++;
            if (Node.IsBoss) { StartCoroutine(SpawnBoss()); return; }
            int depth = run.Depth;
            int count = Mathf.Clamp(2 + depth / 2 + waveIndex, 3, 12);
            if (Node.type == RoomType.Elite) count = Mathf.CeilToInt(count * 0.7f);
            StartCoroutine(SpawnWave(count));
        }

        IEnumerator SpawnBoss()
        {
            pendingSpawns++;
            Fx.Pulse(new Vector2(0, 4), 5f, Biome.accent, 0.8f);
            yield return new WaitForSeconds(0.9f);
            Enemy.Spawn(Biome.boss, new Vector2(0, 4f), 1f + 0.02f * run.Depth, 1f);
            pendingSpawns--;
        }

        IEnumerator SpawnWave(int count)
        {
            pendingSpawns += count;
            bool elite = Node.type == RoomType.Elite;
            float hp = 1f + 0.10f * run.Depth + (elite ? 0.8f : 0f);
            float dmg = 1f + 0.05f * run.Depth + (elite ? 0.25f : 0f);
            var positions = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = RoomBuilder.RandomInterior(this, rng, 2.5f);
                positions.Add(pos);
                Fx.Pulse(pos, 1.0f, Biome.accent, 0.8f);
            }
            yield return new WaitForSeconds(0.8f);
            for (int i = 0; i < count; i++)
            {
                var def = PickEnemy(elite);
                var e = Enemy.Spawn(def, positions[i], hp, dmg);
                if (elite) e.transform.localScale *= 1.25f;
                pendingSpawns--;
                yield return new WaitForSeconds(0.1f);
            }
        }

        EnemyDef PickEnemy(bool elite)
        {
            int maxTier = (Node.layer >= 1 || elite) ? 1 : 0;
            var candidates = new List<EnemyDef>();
            foreach (var d in Biome.pool) if (d.tier <= maxTier) candidates.Add(d);
            return candidates[rng.Next(candidates.Count)];
        }

        void Clear()
        {
            Cleared = true;
            run.roomsCleared++;
            GameEvents.RaiseRoomCleared(this);
            CameraRig.Shake(0.15f);

            Vector2 center = new Vector2(0f, 0f);
            switch (Node.reward)
            {
                case RewardType.Gold: RewardPickup.Spawn(transform, center, RewardType.Gold, 25 + 6 * run.Depth); break;
                case RewardType.Stardust: RewardPickup.Spawn(transform, center, RewardType.Stardust, 2 + run.Depth / 3); break;
                case RewardType.None: break;
                default: RewardPickup.Spawn(transform, center, Node.reward, 0); break;
            }
            if (Node.IsBoss)
            {
                RunDrops.AddStardust(run, 8 + 6 * Node.biome);
                RewardPickup.Spawn(transform, new Vector2(3f, 0f), RewardType.Vial, 0);
                if (Node.biome == 3) { RunManager.I.Victory(); return; }
            }
            else if (Node.type == RoomType.Elite) RunDrops.AddStardust(run, 2);

            OpenDoors();
        }

        void OpenDoors()
        {
            var options = RunManager.I.NextOptions(Node);
            if (options == null || options.Length == 0) return;
            for (int i = 0; i < options.Length; i++)
            {
                float x = options.Length == 1 ? 0f : Mathf.Lerp(-8f, 8f, i / (float)(options.Length - 1));
                var d = Door.Create(transform, new Vector2(x, Arena.yMax - 0.1f), options[i], Biome.accent);
                d.Open();
                Doors.Add(d);
            }
        }
    }
}
