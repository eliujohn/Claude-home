using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    public class Interactable : MonoBehaviour
    {
        public static readonly List<Interactable> All = new List<Interactable>();
        public string label = "Interact";
        public float range = 2.2f;
        public Action onInteract;

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        public static Interactable Nearest(Vector2 from)
        {
            Interactable best = null; float bd = float.MaxValue;
            foreach (var i in All)
            {
                float d = ((Vector2)i.transform.position - from).magnitude;
                if (d <= i.range && d < bd) { bd = d; best = i; }
            }
            return best;
        }
    }

    /// <summary>The Abyss Terminal: quiet liminal sanctuary where Alistair returns after every death.</summary>
    public class HubController : MonoBehaviour
    {
        public static readonly Rect Arena = new Rect(-12f, -7f, 24f, 14f);
        public static readonly Vector2 Spawn = new Vector2(0f, -4f);

        public static HubController Create()
        {
            var go = new GameObject("Abyss Terminal");
            return go.AddComponent<HubController>();
        }

        void Start()
        {
            RoomBuilder.BuildShell(transform, Arena, new Color(0.08f, 0.07f, 0.12f), new Color(0.03f, 0.03f, 0.06f));
            CameraRig.SetBounds(Arena);

            Npc("Vesper", DialogueDirector.Vesper, new Vector2(-7f, 2f), new Color(0.6f, 0.75f, 0.95f));
            Npc("Orrin", DialogueDirector.Orrin, new Vector2(0f, 4.5f), new Color(0.95f, 0.8f, 0.4f));
            Npc("Cassian", DialogueDirector.Cassian, new Vector2(7f, 2f), new Color(0.75f, 0.95f, 0.7f));

            // Stellar Mirror Matrix
            var mirror = Prop("Stellar Mirror Matrix", new Vector2(-3.5f, 4.5f), new Vector2(1.6f, 2.4f), new Color(0.5f, 0.9f, 1f), SpriteFactory.Square);
            mirror.label = "Stellar Mirror Matrix";
            mirror.onInteract = () => MatrixView.Open();

            // Ascent gate
            var gate = Prop("Gate of Ascent", new Vector2(3.5f, 4.5f), new Vector2(2.6f, 2.6f), new Color(1f, 0.55f, 0.2f), SpriteFactory.Circle);
            gate.label = "Gate of Ascent";
            gate.onInteract = () => GameManager.I.StartRun();

            var p = Player.Current;
            p.Body.position = Spawn;
            p.transform.position = Spawn;
        }

        void Npc(string shortName, string fullName, Vector2 pos, Color color)
        {
            var i = Prop(shortName, pos, new Vector2(0.9f, 0.9f), color, SpriteFactory.Circle);
            i.label = fullName;
            i.onInteract = () => DialogueView.Open(fullName, DialogueDirector.Pick(fullName));
        }

        Interactable Prop(string name, Vector2 pos, Vector2 size, Color color, Sprite sprite)
        {
            var sr = SpriteFactory.Child(transform, name, sprite, color, size, 2);
            sr.transform.position = pos;
            sr.gameObject.AddComponent<YSort>();
            return sr.gameObject.AddComponent<Interactable>();
        }

        void Update()
        {
            var p = Player.Current;
            if (p == null || GameManager.State != GameFlow.Hub) return;
            if (!InputReader.InteractPressed) return;
            var near = Interactable.Nearest(p.Position);
            if (near != null) near.onInteract?.Invoke();
        }
    }
}
