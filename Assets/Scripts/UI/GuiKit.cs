using UnityEngine;

namespace Ascension
{
    /// <summary>Tiny IMGUI helper: resolution-independent 1280x720 logical canvas, flat rects, centred labels.</summary>
    public static class GuiKit
    {
        public const float LogicalH = 720f;
        static Texture2D white;
        static GUIStyle labelStyle;

        public static float Scale => Screen.height / LogicalH;
        public static float W => Screen.width / Scale;
        public static float H => LogicalH;

        static Texture2D White
        {
            get
            {
                if (white == null) { white = new Texture2D(1, 1); white.SetPixel(0, 0, Color.white); white.Apply(); }
                return white;
            }
        }

        public static void Begin() => GUI.matrix = Matrix4x4.Scale(new Vector3(Scale, Scale, 1f));
        public static void End() => GUI.matrix = Matrix4x4.identity;

        public static void Fill(Rect r, Color c)
        {
            var old = GUI.color; GUI.color = c;
            GUI.DrawTexture(r, White);
            GUI.color = old;
        }

        public static void Label(Rect r, string text, int size = 16, Color? color = null, TextAnchor anchor = TextAnchor.UpperLeft, bool bold = false)
        {
            if (labelStyle == null) labelStyle = new GUIStyle(GUI.skin.label) { wordWrap = true, richText = true };
            labelStyle.fontSize = size;
            labelStyle.alignment = anchor;
            labelStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            var old = GUI.color;
            // soft shadow keeps text readable over any floor colour
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), text, labelStyle);
            GUI.color = color ?? Color.white;
            GUI.Label(r, text, labelStyle);
            GUI.color = old;
        }

        public static bool Button(Rect r, string text, Color bg, int size = 16)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            Fill(r, hover ? Color.Lerp(bg, Color.white, 0.2f) : bg);
            Label(r, text, size, Color.white, TextAnchor.MiddleCenter, true);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        public static Vector2 WorldToGui(Vector3 world)
        {
            var cam = Camera.main;
            if (cam == null) return Vector2.zero;
            Vector3 s = cam.WorldToScreenPoint(world);
            return new Vector2(s.x / Scale, (Screen.height - s.y) / Scale);
        }
    }
}
