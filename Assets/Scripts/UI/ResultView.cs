using UnityEngine;

namespace Ascension
{
    /// <summary>Death / victory summary shown before returning to the Abyss Terminal.</summary>
    public class ResultView : MonoBehaviour
    {
        void Update()
        {
            if ((GameManager.State == GameFlow.Dead || GameManager.State == GameFlow.Victory) && InputReader.InteractPressed)
                GameManager.I.GoHub();
        }

        void OnGUI()
        {
            bool dead = GameManager.State == GameFlow.Dead, won = GameManager.State == GameFlow.Victory;
            if (!dead && !won) return;
            var rec = MetaProgression.Data.lastDeath;
            GuiKit.Begin();
            GuiKit.Fill(new Rect(0, 0, GuiKit.W, GuiKit.H), new Color(0, 0, 0, 0.78f));
            GuiKit.Label(new Rect(0, 150, GuiKit.W, 70), won ? "ASCENSION" : "YOU HAVE FALLEN", 56, won ? new Color(1f, 0.95f, 0.7f) : new Color(0.9f, 0.2f, 0.25f), TextAnchor.MiddleCenter, true);
            string sub = won ? "The Sovereign of Judgment is no more." : "Slain by " + rec.killer + (rec.byHazard ? " (environment)" : "");
            GuiKit.Label(new Rect(0, 225, GuiKit.W, 30), sub, 22, Color.white, TextAnchor.MiddleCenter);
            string biome = BiomeDatabase.All[Mathf.Clamp(rec.biome, 0, BiomeDatabase.All.Length - 1)].name;
            GuiKit.Label(new Rect(0, 275, GuiKit.W, 100),
                biome + "\nTime: " + Mathf.RoundToInt(rec.seconds) + "s     Foes slain: " + rec.enemiesKilled + "     Gold: " + rec.gold +
                "\nStardust banked: +" + rec.stardust,
                20, new Color(1f, 0.9f, 0.6f), TextAnchor.UpperCenter);
            GuiKit.Label(new Rect(0, 460, GuiKit.W, 30), "[E]  Return to the Abyss Terminal", 22, Color.white, TextAnchor.MiddleCenter, true);
            GuiKit.End();
        }
    }
}
