using UnityEngine;

namespace Ascension
{
    public class HudView : MonoBehaviour
    {
        static string bannerTitle = "", bannerSub = "";
        static float bannerEnd;

        public static void ShowBanner(string title, string sub = "")
        {
            bannerTitle = title; bannerSub = sub; bannerEnd = Time.unscaledTime + 3.2f;
        }

        void OnGUI()
        {
            var p = Player.Current;
            if (p == null || GameManager.State == GameFlow.Boot) return;
            GuiKit.Begin();
            DrawVitals(p);
            DrawSponsors(p);
            DrawLocation(p);
            DrawBoss();
            DrawWorldLabels(p);
            DrawBanner();
            DrawHints();
            GuiKit.End();
        }

        void DrawVitals(Player p)
        {
            var h = p.Health;
            var r = new Rect(20, GuiKit.H - 52, 300, 24);
            GuiKit.Fill(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), new Color(0, 0, 0, 0.7f));
            GuiKit.Fill(new Rect(r.x, r.y, r.width * h.Fraction, r.height), new Color(0.8f, 0.12f, 0.2f));
            GuiKit.Label(r, Mathf.CeilToInt(h.current) + " / " + Mathf.CeilToInt(h.max), 15, Color.white, TextAnchor.MiddleCenter, true);
            var dash = new Rect(r.x, r.y + 30, 300 * 0.45f, 8);
            GuiKit.Fill(dash, new Color(0, 0, 0, 0.6f));
            GuiKit.Fill(new Rect(dash.x, dash.y, dash.width * p.Controller.DashReady, dash.height), new Color(0.6f, 0.85f, 1f));
        }

        void DrawLocation(Player p)
        {
            string loc;
            if (GameManager.State == GameFlow.Hub) loc = "The Abyss Terminal";
            else
            {
                var room = RoomController.Current;
                var map = RunManager.I != null ? RunManager.I.Map : null;
                if (room == null || map == null) return;
                string chamber = room.Node.IsBoss ? "BOSS" : "Chamber " + (room.Node.layer + 1) + "/" + (map.Layers.Count - 1);
                loc = room.Biome.name + "  -  " + chamber;
            }
            GuiKit.Label(new Rect(GuiKit.W - 520, 14, 500, 26), loc, 18, Color.white, TextAnchor.UpperRight, true);
            string wallet = "Stardust banked: " + MetaProgression.Stardust;
            if (GameManager.State != GameFlow.Hub)
                wallet = "Gold: " + p.Run.gold + "    Stardust this run: " + p.Run.stardustEarned + "    Defiances: " + p.Run.defiancesLeft;
            GuiKit.Label(new Rect(GuiKit.W - 520, 40, 500, 24), wallet, 15, new Color(1f, 0.9f, 0.5f), TextAnchor.UpperRight);

            // Endless Horizon: the whole realm layout is revealed
            if (GameManager.State == GameFlow.Run && p.Sponsors.RevealsMap && RunManager.I != null)
            {
                var map = RunManager.I.Map;
                float y = 70;
                GuiKit.Label(new Rect(GuiKit.W - 520, y, 500, 20), "-- Map Reveal --", 13, new Color(0.5f, 0.85f, 1f), TextAnchor.UpperRight, true);
                for (int l = map.LayerIndex + 1; l < map.Layers.Count; l++)
                {
                    string row = "";
                    foreach (var n in map.Layers[l]) row += RunMap.Describe(n, true).Replace("\n", ": ") + "  |  ";
                    y += 18;
                    GuiKit.Label(new Rect(GuiKit.W - 620, y, 600, 20), row, 12, Color.white, TextAnchor.UpperRight);
                }
            }
        }

        void DrawSponsors(Player p)
        {
            float y = 14;
            foreach (var b in p.Sponsors.Active)
            {
                GuiKit.Fill(new Rect(14, y + 3, 6, 34), b.Def.constellationColor);
                GuiKit.Label(new Rect(26, y, 360, 20), b.Def.sponsorTitle + "  <color=#ffd966>R" + b.Rank + "</color>", 15, Color.white, TextAnchor.UpperLeft, true);
                string status = b.HudStatus;
                GuiKit.Label(new Rect(26, y + 18, 360, 20), status ?? b.Def.sponsorName, 11, status != null ? b.Def.constellationColor : new Color(1, 1, 1, 0.5f));
                y += 40;
            }
        }

        void DrawBoss()
        {
            var boss = Boss.Current;
            if (boss == null || boss.Health.IsDead || GameManager.State != GameFlow.Run) return;
            float w = 560, x = (GuiKit.W - w) / 2;
            GuiKit.Label(new Rect(x, 8, w, 24), boss.Def.name, 18, Color.white, TextAnchor.MiddleCenter, true);
            GuiKit.Fill(new Rect(x - 2, 32, w + 4, 16), new Color(0, 0, 0, 0.75f));
            GuiKit.Fill(new Rect(x, 34, w * boss.Health.Fraction, 12), boss.ShieldActive ? new Color(0.8f, 0.8f, 1f) : new Color(0.9f, 0.2f, 0.15f));
            if (boss.ShieldActive) GuiKit.Label(new Rect(x, 50, w, 20), "SHIELDED - destroy the summoned allies", 13, new Color(0.8f, 0.8f, 1f), TextAnchor.MiddleCenter);
        }

        void DrawWorldLabels(Player p)
        {
            if (GameManager.State == GameFlow.Run && RoomController.Current != null)
            {
                bool reveal = p.Sponsors.RevealsMap;
                foreach (var d in RoomController.Current.Doors)
                {
                    if (d == null) continue;
                    var pos = GuiKit.WorldToGui(d.transform.position + Vector3.down * 2.3f);
                    string text = RunMap.Describe(d.Target, reveal);
                    var rect = new Rect(pos.x - 90, pos.y - 20, 180, reveal ? 44 : 26);
                    GuiKit.Fill(rect, new Color(0, 0, 0, 0.65f));
                    Color tint = d.Target.type == RoomType.Elite ? new Color(1f, 0.5f, 0.4f) : RewardPickup.ColorFor(d.Target.reward);
                    GuiKit.Label(rect, text, 14, tint, TextAnchor.MiddleCenter, true);
                }
            }
            if (GameManager.State == GameFlow.Hub)
            {
                foreach (var i in Interactable.All)
                {
                    var pos = GuiKit.WorldToGui(i.transform.position + Vector3.up * 1.6f);
                    GuiKit.Label(new Rect(pos.x - 160, pos.y - 12, 320, 24), i.label, 13, new Color(1, 1, 1, 0.85f), TextAnchor.MiddleCenter);
                }
                var near = Interactable.Nearest(p.Position);
                if (near != null && !GameManager.InputLocked)
                    GuiKit.Label(new Rect(0, GuiKit.H - 120, GuiKit.W, 30), "[E]  " + near.label, 22, new Color(1f, 0.95f, 0.6f), TextAnchor.MiddleCenter, true);
            }
        }

        void DrawBanner()
        {
            if (Time.unscaledTime > bannerEnd) return;
            float a = Mathf.Clamp01(bannerEnd - Time.unscaledTime);
            GuiKit.Label(new Rect(0, 150, GuiKit.W, 50), bannerTitle, 36, new Color(1, 1, 1, a), TextAnchor.MiddleCenter, true);
            GuiKit.Label(new Rect(0, 198, GuiKit.W, 30), bannerSub, 18, new Color(1, 0.9f, 0.7f, a), TextAnchor.MiddleCenter);
        }

        void DrawHints()
        {
            string hint = GameManager.State == GameFlow.Run
                ? "WASD move   LMB melee   RMB shot   SPACE dash   Q special   F chat prompt"
                : "WASD move   E interact";
            GuiKit.Label(new Rect(GuiKit.W - 620, GuiKit.H - 28, 600, 22), hint, 12, new Color(1, 1, 1, 0.55f), TextAnchor.UpperRight);
        }
    }
}
