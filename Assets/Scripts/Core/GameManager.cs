using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    public enum GameFlow { Boot, Hub, Run, Choosing, Dialogue, Matrix, Dead, Victory }

    /// <summary>
    /// Top-level flow: Hub (Abyss Terminal) -> Run -> Dead/Victory -> Hub. Owns scene bootstrapping so the
    /// project works from an empty scene. Overlay states (Choosing/Dialogue/Matrix) are pushed on a stack.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }
        public static GameFlow State { get; private set; } = GameFlow.Boot;
        public static bool InputLocked => State != GameFlow.Hub && State != GameFlow.Run;

        static readonly Stack<GameFlow> stack = new Stack<GameFlow>();
        HubController hub;

        public static void Push(GameFlow s)
        {
            stack.Push(State);
            State = s;
            if (s == GameFlow.Choosing) Time.timeScale = 0f;
        }

        public static void Pop()
        {
            State = stack.Count > 0 ? stack.Pop() : GameFlow.Hub;
            Time.timeScale = 1f;
        }

        void Awake()
        {
            I = this;
            Application.targetFrameRate = 60;
            Layers.ConfigurePhysics();
            MetaProgression.Load();
            SetupCamera();

            gameObject.AddComponent<RunManager>();
            gameObject.AddComponent<HudView>();
            gameObject.AddComponent<BlessingView>();
            gameObject.AddComponent<MatrixView>();
            gameObject.AddComponent<DialogueView>();
            gameObject.AddComponent<ChatPromptView>();
            gameObject.AddComponent<ResultView>();

            Player.Create(HubController.Spawn);
        }

        void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;
        void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

        void Start()
        {
            Player.Current.RefreshStats(true);
            GoHub();
        }

        void Update()
        {
            if (State == GameFlow.Run && Player.Current != null) Player.Current.Run.elapsed += Time.deltaTime;
        }

        static void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.04f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0, 0, -10f);
            if (cam.GetComponent<CameraRig>() == null) cam.gameObject.AddComponent<CameraRig>();
        }

        // ---------- flow ----------
        public void GoHub()
        {
            stack.Clear();
            Time.timeScale = 1f;
            if (RoomController.Current != null) Destroy(RoomController.Current.gameObject);
            if (hub != null) Destroy(hub.gameObject);
            RunManager.ClearTransients();
            ChatPromptBoard.Clear();

            var p = Player.Current;
            p.StopAllCoroutines();
            p.Sponsors.Clear();
            p.Run = new RunState(0);
            p.RefreshStats(true);
            p.SetInvisible(0f);
            p.Body.linearVelocity = Vector2.zero;
            hub = HubController.Create();
            State = GameFlow.Hub;
        }

        public void StartRun()
        {
            if (State != GameFlow.Hub) return;
            if (hub != null) { Destroy(hub.gameObject); hub = null; }
            State = GameFlow.Run;
            RunManager.I.BeginRun(Player.Current);
        }

        void OnPlayerDied(DamageInfo info)
        {
            if (State != GameFlow.Run) return;
            var rec = BuildRecord(info, false);
            MetaProgression.RecordRun(rec);
            stack.Clear();
            State = GameFlow.Dead;
            Time.timeScale = 1f;
            Player.Current.Body.linearVelocity = Vector2.zero;
        }

        public void OnVictory()
        {
            if (State != GameFlow.Run) return;
            var rec = BuildRecord(null, true);
            rec.killer = "";
            MetaProgression.RecordRun(rec);
            State = GameFlow.Victory;
        }

        static DeathRecord BuildRecord(DamageInfo info, bool victory)
        {
            var p = Player.Current;
            var run = p.Run;
            var rec = new DeathRecord
            {
                victory = victory,
                killer = info != null ? info.sourceName : "",
                byHazard = info != null && info.isHazard,
                biome = run.biomeIndex, layer = run.layerIndex,
                gold = run.gold, stardust = run.stardustEarned, enemiesKilled = run.enemiesKilled, seconds = run.elapsed
            };
            foreach (var biome in BiomeDatabase.All) if (info != null && biome.boss.name == info.sourceName) rec.byBoss = true;
            foreach (var s in p.Sponsors.Active) rec.sponsors.Add((int)s.Id);
            return rec;
        }
    }
}
