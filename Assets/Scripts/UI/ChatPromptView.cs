using UnityEngine;

namespace Ascension
{
    /// <summary>On-screen prompts spawned by the Over-Enthusiastic Chat Moderator.</summary>
    public class ChatPromptView : MonoBehaviour
    {
        void Update()
        {
            if (GameManager.State == GameFlow.Run) ChatPromptBoard.Tick(Time.deltaTime);
        }

        void OnGUI()
        {
            var list = ChatPromptBoard.Prompts;
            if (list.Count == 0 || GameManager.State != GameFlow.Run) return;
            GuiKit.Begin();
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                var r = new Rect(GuiKit.W - 330, 230 + i * 96, 310, 86);
                GuiKit.Fill(r, new Color(0.35f, 0.2f, 0.65f, 0.92f));
                GuiKit.Fill(new Rect(r.x, r.yMax - 5, r.width * Mathf.Clamp01(p.remaining / 4.5f), 5), new Color(1f, 0.9f, 0.4f));
                GuiKit.Label(new Rect(r.x + 10, r.y + 6, r.width - 20, 26), p.title, 20, Color.white, TextAnchor.UpperLeft, true);
                GuiKit.Label(new Rect(r.x + 10, r.y + 34, r.width - 20, 22), p.subtitle, 14, new Color(1, 1, 1, 0.85f));
                GuiKit.Label(new Rect(r.x + 10, r.y + 58, r.width - 20, 22), i == 0 ? "[F] or click to accept" : "click to accept", 12, new Color(1f, 0.9f, 0.4f));
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { ChatPromptBoard.Accept(i); break; }
            }
            GuiKit.End();
        }
    }
}
