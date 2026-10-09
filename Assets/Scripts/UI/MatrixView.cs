using UnityEngine;

namespace Ascension
{
    /// <summary>Stellar Mirror Matrix: permanent upgrades (left) and the Constellation codex (right).</summary>
    public class MatrixView : MonoBehaviour
    {
        static bool open;
        static int openedFrame;
        public static void Open() { if (open) return; open = true; openedFrame = Time.frameCount; GameManager.Push(GameFlow.Matrix); }
        static void Close() { open = false; GameManager.Pop(); }

        void Update()
        {
            if (open && Time.frameCount != openedFrame && (InputReader.CancelPressed || InputReader.InteractPressed)) Close();
        }

        void OnGUI()
        {
            if (!open) return;
            GuiKit.Begin();
            GuiKit.Fill(new Rect(0, 0, GuiKit.W, GuiKit.H), new Color(0.02f, 0.02f, 0.06f, 0.92f));
            GuiKit.Label(new Rect(40, 20, 700, 40), "STELLAR MIRROR MATRIX", 30, new Color(0.6f, 0.9f, 1f), TextAnchor.UpperLeft, true);
            GuiKit.Label(new Rect(40, 62, 700, 26), "Stardust: " + MetaProgression.Stardust + "      (Esc / E to close)", 17, new Color(1f, 0.9f, 0.5f));

            float y = 100;
            foreach (var d in MetaProgression.Defs)
            {
                int rank = MetaProgression.Rank(d.id);
                bool maxed = rank >= d.maxRank;
                var row = new Rect(40, y, 640, 52);
                GuiKit.Fill(row, new Color(1, 1, 1, 0.06f));
                GuiKit.Label(new Rect(52, y + 4, 400, 22), d.name + "   <color=#ffd966>" + rank + "/" + d.maxRank + "</color>", 17, Color.white, TextAnchor.UpperLeft, true);
                GuiKit.Label(new Rect(52, y + 26, 460, 22), d.description, 13, new Color(1, 1, 1, 0.65f));
                if (!maxed)
                {
                    int cost = d.CostFor(rank);
                    bool can = MetaProgression.CanBuy(d.id);
                    if (GuiKit.Button(new Rect(560, y + 8, 110, 36), cost + " dust", can ? new Color(0.2f, 0.45f, 0.6f) : new Color(0.25f, 0.25f, 0.28f), 15) && can)
                        MetaProgression.TryBuy(d.id);
                }
                else GuiKit.Label(new Rect(560, y + 8, 110, 36), "MAX", 16, new Color(0.5f, 1f, 0.6f), TextAnchor.MiddleCenter, true);
                y += 58;
            }

            GuiKit.Label(new Rect(720, 20, 500, 30), "THE FIFTEEN CONSTELLATIONS", 20, new Color(1f, 0.95f, 0.75f), TextAnchor.UpperLeft, true);
            float cy = 56;
            foreach (var def in ConstellationDatabase.All)
            {
                GuiKit.Fill(new Rect(720, cy + 2, 5, 36), def.constellationColor);
                GuiKit.Label(new Rect(732, cy, 520, 20), def.sponsorTitle + "  -  " + def.sponsorName + "  <color=#ffd966>x" + MetaProgression.Data.sponsorPicks[(int)def.id] + "</color>", 12, Color.white, TextAnchor.UpperLeft, true);
                GuiKit.Label(new Rect(732, cy + 16, 520, 24), def.blessingDescription, 10, new Color(1, 1, 1, 0.55f));
                cy += 42;
            }
            GuiKit.End();
        }
    }
}
