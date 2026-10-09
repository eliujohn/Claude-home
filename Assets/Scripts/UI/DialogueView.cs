using UnityEngine;

namespace Ascension
{
    public class DialogueView : MonoBehaviour
    {
        static string speaker;
        static string[] lines;
        static int index, openedFrame;

        public static void Open(string who, string[] text)
        {
            speaker = who; lines = text; index = 0; openedFrame = Time.frameCount;
            GameManager.Push(GameFlow.Dialogue);
        }

        static void Advance()
        {
            index++;
            if (lines == null || index >= lines.Length) { lines = null; GameManager.Pop(); }
        }

        void Update()
        {
            if (lines == null || Time.frameCount == openedFrame) return;
            if (InputReader.InteractPressed || InputReader.DashPressed) Advance();
        }

        void OnGUI()
        {
            if (lines == null) return;
            GuiKit.Begin();
            var r = new Rect(140, GuiKit.H - 220, GuiKit.W - 280, 170);
            GuiKit.Fill(r, new Color(0.04f, 0.03f, 0.08f, 0.94f));
            GuiKit.Fill(new Rect(r.x, r.y, r.width, 4), new Color(1f, 0.85f, 0.5f));
            GuiKit.Label(new Rect(r.x + 20, r.y + 14, r.width - 40, 28), speaker, 20, new Color(1f, 0.85f, 0.5f), TextAnchor.UpperLeft, true);
            GuiKit.Label(new Rect(r.x + 20, r.y + 50, r.width - 40, 90), lines[Mathf.Min(index, lines.Length - 1)], 20, Color.white);
            GuiKit.Label(new Rect(r.x + 20, r.y + 138, r.width - 40, 24), "[E] continue", 13, new Color(1, 1, 1, 0.5f), TextAnchor.UpperRight);
            if (GUI.Button(r, GUIContent.none, GUIStyle.none)) Advance();
            GuiKit.End();
        }
    }
}
