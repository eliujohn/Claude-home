using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    public enum MatrixId { IronWill, DefiantEdge, SwiftSoul, PhantomStep, UnbrokenWill, SponsorFavor, FatedReroll, Dowry, StardustSiphon }

    public class MatrixDef
    {
        public MatrixId id;
        public string name, description;
        public int maxRank, baseCost, costStep;
        public int CostFor(int currentRank) => baseCost + costStep * currentRank;
    }

    [Serializable]
    public class SaveData
    {
        public int stardust, runs, deaths, victories, bestBiome;
        public List<int> matrixRanks = new List<int>();
        public List<int> sponsorPicks = new List<int>();
        public List<string> seenDialogue = new List<string>();
        public List<string> killers = new List<string>();
        public bool hasLastDeath;
        public DeathRecord lastDeath = new DeathRecord();
    }

    /// <summary>Permanent progression: the Stellar Mirror Matrix, run history and dialogue memory. JSON in PlayerPrefs.</summary>
    public static class MetaProgression
    {
        const string Key = "ascension.save.v1";
        public static SaveData Data { get; private set; } = new SaveData();

        public static readonly MatrixDef[] Defs =
        {
            new MatrixDef { id = MatrixId.IronWill, name = "Iron Will", description = "+10 max health per rank.", maxRank = 10, baseCost = 12, costStep = 6 },
            new MatrixDef { id = MatrixId.DefiantEdge, name = "Defiant Edge", description = "+4% damage per rank.", maxRank = 10, baseCost = 15, costStep = 8 },
            new MatrixDef { id = MatrixId.SwiftSoul, name = "Swift Soul", description = "+2.5% move speed per rank.", maxRank = 8, baseCost = 10, costStep = 6 },
            new MatrixDef { id = MatrixId.PhantomStep, name = "Phantom Step", description = "-4% dash cooldown per rank.", maxRank = 6, baseCost = 12, costStep = 8 },
            new MatrixDef { id = MatrixId.UnbrokenWill, name = "Unbroken Will", description = "Survive a fatal blow at half health, once per rank each run.", maxRank = 3, baseCost = 60, costStep = 50 },
            new MatrixDef { id = MatrixId.SponsorFavor, name = "Sponsor Favor", description = "Constellations offer +1 Stellar Blessing choice.", maxRank = 1, baseCost = 90, costStep = 0 },
            new MatrixDef { id = MatrixId.FatedReroll, name = "Fated Reroll", description = "Reroll a Blessing offer, once per rank each run.", maxRank = 3, baseCost = 35, costStep = 25 },
            new MatrixDef { id = MatrixId.Dowry, name = "Hell's Dowry", description = "Begin each run with +25 gold per rank.", maxRank = 6, baseCost = 8, costStep = 5 },
            new MatrixDef { id = MatrixId.StardustSiphon, name = "Stardust Siphon", description = "+10% Stardust from every source per rank.", maxRank = 5, baseCost = 25, costStep = 20 },
        };

        public static int Stardust => Data.stardust;

        public static void Load()
        {
            try
            {
                if (PlayerPrefs.HasKey(Key)) Data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key)) ?? new SaveData();
            }
            catch (Exception) { Data = new SaveData(); }
            while (Data.matrixRanks.Count < Defs.Length) Data.matrixRanks.Add(0);
            while (Data.sponsorPicks.Count < Enum.GetValues(typeof(ConstellationId)).Length) Data.sponsorPicks.Add(0);
        }

        public static void Save()
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public static void ResetAll() { PlayerPrefs.DeleteKey(Key); Data = new SaveData(); Load(); }

        public static int Rank(MatrixId id) => Data.matrixRanks.Count > (int)id ? Data.matrixRanks[(int)id] : 0;
        public static MatrixDef DefOf(MatrixId id) => Defs[(int)id];

        public static bool CanBuy(MatrixId id)
        {
            var d = DefOf(id);
            int r = Rank(id);
            return r < d.maxRank && Data.stardust >= d.CostFor(r);
        }

        public static bool TryBuy(MatrixId id)
        {
            if (!CanBuy(id)) return false;
            var d = DefOf(id);
            Data.stardust -= d.CostFor(Rank(id));
            Data.matrixRanks[(int)id]++;
            Save();
            return true;
        }

        public static void RecordPick(ConstellationId id) { Data.sponsorPicks[(int)id]++; }

        public static void RecordRun(DeathRecord rec)
        {
            Data.runs++;
            if (rec.victory) { Data.victories++; rec.stardust += 25; } else { Data.deaths++; Data.killers.Add(rec.killer); }
            Data.stardust += rec.stardust;
            Data.bestBiome = Mathf.Max(Data.bestBiome, rec.biome);
            Data.lastDeath = rec;
            Data.hasLastDeath = true;
            Save();
        }

        public static bool Seen(string dialogueId) => Data.seenDialogue.Contains(dialogueId);
        public static void MarkSeen(string dialogueId) { if (!Data.seenDialogue.Contains(dialogueId)) { Data.seenDialogue.Add(dialogueId); Save(); } }

        public static int KilledByCount(string killer)
        {
            int n = 0;
            foreach (var k in Data.killers) if (k == killer) n++;
            return n;
        }
    }
}
