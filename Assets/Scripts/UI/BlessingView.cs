using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>Stellar Blessing selection cards. Pauses the world while open.</summary>
    public class BlessingView : MonoBehaviour
    {
        static List<BlessingChoice> choices;
        public static bool IsOpen => choices != null;

        public static void Open(List<BlessingChoice> offer)
        {
            choices = offer;
            GameManager.Push(GameFlow.Choosing);
        }

        static void Close()
        {
            choices = null;
            GameManager.Pop();
        }

        void Update()
        {
            if (!IsOpen) return;
            for (int i = 0; i < choices.Count; i++)
                if (InputReader.NumberPressed(i + 1)) { Pick(choices[i]); return; }
            if (InputReader.KeyDown(KeyCode.R)) Reroll();
        }

        static void Pick(BlessingChoice c)
        {
            RunManager.I.ChooseBlessing(c);
            Close();
        }

        static void Reroll()
        {
            if (!RunManager.I.TryReroll()) return;
            choices = BlessingService.Roll(RunManager.I.State, Player.Current.Sponsors, BlessingService.OfferSize);
        }

        void OnGUI()
        {
            if (!IsOpen) return;
            GuiKit.Begin();
            GuiKit.Fill(new Rect(0, 0, GuiKit.W, GuiKit.H), new Color(0, 0, 0, 0.75f));
            GuiKit.Label(new Rect(0, 40, GuiKit.W, 50), "THE CONSTELLATIONS STIR", 34, new Color(1f, 0.95f, 0.75f), TextAnchor.MiddleCenter, true);
            GuiKit.Label(new Rect(0, 88, GuiKit.W, 28), "Choose a Stellar Blessing  (click or press 1-" + choices.Count + ")", 17, new Color(1, 1, 1, 0.7f), TextAnchor.MiddleCenter);

            float cardW = 260, gap = 24, total = choices.Count * cardW + (choices.Count - 1) * gap;
            float x0 = (GuiKit.W - total) / 2f;
            for (int i = 0; i < choices.Count; i++)
            {
                var c = choices[i];
                var r = new Rect(x0 + i * (cardW + gap), 150, cardW, 430);
                bool hover = r.Contains(Event.current.mousePosition);
                GuiKit.Fill(r, hover ? new Color(0.16f, 0.14f, 0.22f, 1f) : new Color(0.1f, 0.09f, 0.15f, 1f));
                GuiKit.Fill(new Rect(r.x, r.y, r.width, 8), c.def.constellationColor);
                GuiKit.Label(new Rect(r.x + 12, r.y + 20, r.width - 24, 70), c.def.sponsorName, 17, c.def.constellationColor, TextAnchor.UpperLeft, true);
                GuiKit.Label(new Rect(r.x + 12, r.y + 92, r.width - 24, 24), c.def.sponsorTitle, 15, new Color(1, 1, 1, 0.65f));
                GuiKit.Label(new Rect(r.x + 12, r.y + 130, r.width - 24, 190), c.def.blessingDescription, 15, Color.white);
                string tag = c.IsNew ? "NEW" : "RANK " + c.fromRank + " > " + c.toRank;
                if (c.def.altersAttackBehavior) tag += "   (alters attacks)";
                GuiKit.Label(new Rect(r.x + 12, r.y + 340, r.width - 24, 24), tag, 14, new Color(1f, 0.85f, 0.3f), TextAnchor.UpperLeft, true);
                GuiKit.Label(new Rect(r.x + 12, r.y + 395, r.width - 24, 24), "[" + (i + 1) + "]", 16, new Color(1, 1, 1, 0.5f));
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) { Pick(c); GuiKit.End(); return; }
            }

            if (RunManager.I.State.rerollsLeft > 0)
            {
                if (GuiKit.Button(new Rect((GuiKit.W - 240) / 2, 620, 240, 40), "Fated Reroll [R]  x" + RunManager.I.State.rerollsLeft, new Color(0.3f, 0.25f, 0.5f)))
                    Reroll();
            }
            GuiKit.End();
        }
    }
}
