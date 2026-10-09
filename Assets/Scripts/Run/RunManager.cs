using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    /// <summary>Owns the current run: map progression, room loading, blessing offers, victory.</summary>
    public class RunManager : MonoBehaviour
    {
        public static RunManager I { get; private set; }

        public RunState State { get; private set; }
        public RunMap Map { get; } = new RunMap();

        void Awake() => I = this;

        public void BeginRun(Player player)
        {
            State = new RunState(System.Environment.TickCount);
            State.gold = 25 * MetaProgression.Rank(MatrixId.Dowry);
            State.defiancesLeft = MetaProgression.Rank(MatrixId.UnbrokenWill);
            State.rerollsLeft = MetaProgression.Rank(MatrixId.FatedReroll);
            player.Run = State;
            player.Sponsors.Clear();
            player.RefreshStats(true);
            GlobalTime.Reset();
            ChatPromptBoard.Clear();

            Map.EnterBiome(0, State.rng);
            LoadNode(Map.PeekNext()[0]);
        }

        public void Advance(RoomNode node)
        {
            if (node.type == RoomType.Transition)
            {
                Map.EnterBiome(node.biome, State.rng);
                LoadNode(Map.PeekNext()[0]);
                return;
            }
            LoadNode(node);
        }

        /// <summary>Doors offered after clearing <paramref name="current"/>.</summary>
        public RoomNode[] NextOptions(RoomNode current)
        {
            if (current.IsBoss)
                return current.biome < BiomeDatabase.All.Length - 1
                    ? new[] { new RoomNode { type = RoomType.Transition, biome = current.biome + 1, reward = RewardType.None } }
                    : new RoomNode[0];
            return Map.PeekNext();
        }

        void LoadNode(RoomNode node)
        {
            Map.Commit(node);
            State.biomeIndex = node.biome;
            State.layerIndex = node.layer;
            ClearTransients();
            if (RoomController.Current != null) Destroy(RoomController.Current.gameObject);
            RoomController.Build(node, BiomeDatabase.All[node.biome], State);
        }

        public static void ClearTransients()
        {
            for (int i = Projectile.Active.Count - 1; i >= 0; i--) if (Projectile.Active[i] != null) Destroy(Projectile.Active[i].gameObject);
            for (int i = Enemy.Active.Count - 1; i >= 0; i--) if (Enemy.Active[i] != null) Destroy(Enemy.Active[i].gameObject);
            Enemy.Active.Clear();
            SpectralThrall.DestroyAll();
        }

        // ---------- blessings ----------
        public void OfferBlessings()
        {
            var player = Player.Current;
            var choices = BlessingService.Roll(State, player.Sponsors, BlessingService.OfferSize);
            if (choices.Count == 0) { State.gold += 50; return; }
            BlessingView.Open(choices);
        }

        public void ChooseBlessing(BlessingChoice c)
        {
            Player.Current.Sponsors.Equip(c.def.id);
            State.picks.Add(c.def.id);
            MetaProgression.RecordPick(c.def.id);
            HudView.ShowBanner(c.def.sponsorName, c.IsNew ? "answers your call" : "grows stronger (Rank " + c.toRank + ")");
        }

        public bool TryReroll()
        {
            if (State.rerollsLeft <= 0) return false;
            State.rerollsLeft--;
            return true;
        }

        public void Victory() => GameManager.I.OnVictory();
    }
}
