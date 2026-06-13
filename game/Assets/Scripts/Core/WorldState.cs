using UnityEngine;
using System.Collections.Generic;

namespace FadingSuns.Core
{
    /// <summary>
    /// Tracks global flags, faction standings, NPC relationship states,
    /// and region availability across the entire game world.
    /// </summary>
    public class WorldState : MonoBehaviour
    {
        public static WorldState Instance { get; private set; }

        private Dictionary<string, bool> flags = new();
        private Dictionary<string, int> factionReputation = new();
        private HashSet<string> unlockedRegions = new();
        private Dictionary<string, NPCRelationshipState> npcRelationships = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Starting region only
            unlockedRegions.Add("City");

            // Default faction reputations (range -100 to 100)
            factionReputation["NoblesHouse"] = 0;
            factionReputation["UniversalChurch"] = 0;
            factionReputation["MerchantLeague"] = 0;
            factionReputation["Aliens"] = 0;
            factionReputation["Underworld"] = 0;
        }

        public void SetFlag(string key, bool value) => flags[key] = value;
        public bool GetFlag(string key) => flags.TryGetValue(key, out var v) && v;

        public void UnlockRegion(string regionId) => unlockedRegions.Add(regionId);
        public bool IsRegionUnlocked(string regionId) => unlockedRegions.Contains(regionId);

        public void ModifyReputation(string faction, int delta)
        {
            if (!factionReputation.ContainsKey(faction)) factionReputation[faction] = 0;
            factionReputation[faction] = Mathf.Clamp(factionReputation[faction] + delta, -100, 100);
        }
        public int GetReputation(string faction) =>
            factionReputation.TryGetValue(faction, out var v) ? v : 0;

        public FactionStanding GetStanding(string faction)
        {
            int rep = GetReputation(faction);
            if (rep >= 60) return FactionStanding.Allied;
            if (rep >= 20) return FactionStanding.Friendly;
            if (rep >= -20) return FactionStanding.Neutral;
            if (rep >= -60) return FactionStanding.Hostile;
            return FactionStanding.Enemy;
        }

        public NPCRelationshipState GetNPCRelationship(string npcId)
        {
            if (!npcRelationships.ContainsKey(npcId))
                npcRelationships[npcId] = new NPCRelationshipState();
            return npcRelationships[npcId];
        }
    }

    public enum FactionStanding { Allied, Friendly, Neutral, Hostile, Enemy }

    [System.Serializable]
    public class NPCRelationshipState
    {
        public int trust = 0;           // -100 to 100
        public bool hasSpoken = false;
        public List<string> knownFacts = new();
        public List<string> completedQuests = new();
    }
}
