using UnityEngine;

namespace Ascension
{
    public class Door : MonoBehaviour
    {
        public RoomNode Target;
        public bool IsOpen { get; private set; }
        SpriteRenderer sr;
        Color openColor;
        bool used;

        public static Door Create(Transform parent, Vector2 pos, RoomNode target, Color accent)
        {
            var go = new GameObject("Door");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(2.6f, 2.2f, 1f);
            var d = go.AddComponent<Door>();
            d.Target = target; d.openColor = accent;
            d.sr = SpriteFactory.Attach(go, SpriteFactory.Square, new Color(0.08f, 0.08f, 0.1f), -9400);
            return d;
        }

        public void Open() { IsOpen = true; sr.color = openColor; }

        void Update()
        {
            if (!IsOpen) return;
            sr.color = Color.Lerp(openColor, Color.white, 0.25f * (1f + Mathf.Sin(Time.time * 4f)) * 0.5f);
            var p = Player.Current;
            if (used || p == null || GameManager.State != GameFlow.Run) return;
            if (((Vector2)transform.position - p.Position).magnitude < 1.8f)
            {
                used = true;
                RunManager.I.Advance(Target);
            }
        }
    }
}
