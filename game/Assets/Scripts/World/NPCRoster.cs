using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Characters;
using FadingSuns.Data;
using FadingSuns.Core;

namespace FadingSuns.World
{
    /// <summary>
    /// Manages all NPCs in a region: spawning, despawning, scheduling.
    /// NPCs are defined in ScriptableObjects; this instantiates their prefabs
    /// and positions them according to their home node and daily schedule.
    /// </summary>
    public class NPCRoster : MonoBehaviour
    {
        [Header("NPC Definitions")]
        [SerializeField] private List<NPCEntry> npcEntries = new();

        [Header("NPC Prefab")]
        public GameObject npcPrefab;

        private Dictionary<string, NPC> spawnedNPCs = new();
        private GameClock clock;

        void Start()
        {
            clock = FindObjectOfType<GameClock>();
            if (clock != null) clock.OnHourChanged += OnHourChanged;
            SpawnAll();
        }

        private void SpawnAll()
        {
            foreach (var entry in npcEntries)
            {
                if (entry.data == null) continue;
                if (!MeetsSpawnCondition(entry)) continue;

                var go = Instantiate(npcPrefab, entry.startPosition, Quaternion.identity, transform);
                go.name = entry.data.displayName;
                var npc = go.GetComponent<NPC>();
                npc.npcData = entry.data;
                npc.data = ScriptableObject.CreateInstance<CharacterData>();
                CopyStatsFromNPCData(entry.data, npc.data);

                if (entry.data.animatorController != null)
                    go.GetComponent<Animator>().runtimeAnimatorController = entry.data.animatorController;

                spawnedNPCs[entry.data.npcId] = npc;
            }
        }

        private bool MeetsSpawnCondition(NPCEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.requiredFlag) &&
                !WorldState.Instance.GetFlag(entry.requiredFlag))
                return false;
            if (!string.IsNullOrEmpty(entry.requiredMilestone) &&
                !GameManager.Instance.milestoneManager.IsMilestoneComplete(entry.requiredMilestone))
                return false;
            if (!string.IsNullOrEmpty(entry.blockedByFlag) &&
                WorldState.Instance.GetFlag(entry.blockedByFlag))
                return false;
            return true;
        }

        private void CopyStatsFromNPCData(NPCData src, CharacterData dst)
        {
            dst.displayName = src.displayName;
            dst.origin = src.origin;
            dst.body = src.body;
            dst.mind = src.mind;
            dst.spirit = src.spirit;
            dst.vitality = src.vitality;
            dst.portrait = src.portrait;
        }

        private void OnHourChanged(int hour)
        {
            // NPCs may move to new positions based on their schedule
            foreach (var kvp in spawnedNPCs)
            {
                var npc = kvp.Value;
                if (npc == null || npc.npcData == null) continue;

                foreach (var entry in npc.npcData.dailySchedule)
                {
                    if (hour >= entry.hourStart && hour < entry.hourEnd)
                    {
                        // In a full implementation, NPC pathfinds to entry.locationNodeId
                        break;
                    }
                }
            }
        }

        public NPC GetNPC(string npcId) =>
            spawnedNPCs.TryGetValue(npcId, out var npc) ? npc : null;

        void OnDestroy()
        {
            if (clock != null) clock.OnHourChanged -= OnHourChanged;
        }
    }

    [System.Serializable]
    public class NPCEntry
    {
        public NPCData data;
        public Vector3 startPosition;
        public string requiredFlag;
        public string requiredMilestone;
        public string blockedByFlag;    // NPC absent if this flag is set (e.g. they were killed)
    }
}
