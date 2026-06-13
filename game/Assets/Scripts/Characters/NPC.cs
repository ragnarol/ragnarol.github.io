using UnityEngine;
using FadingSuns.Data;
using FadingSuns.Core;

namespace FadingSuns.Characters
{
    public class NPC : Character
    {
        [Header("NPC Data")]
        public NPCData npcData;

        [Header("AI State")]
        public NPCState currentState = NPCState.Idle;
        private Vector3Int currentTile;
        private int currentScheduleIndex = 0;

        // Reference to world clock (game time)
        private GameClock clock;

        protected override void Awake()
        {
            base.Awake();
            clock = FindObjectOfType<GameClock>();
        }

        void Update()
        {
            UpdateSchedule();
        }

        private void UpdateSchedule()
        {
            if (npcData == null || npcData.dailySchedule.Count == 0) return;
            if (clock == null) return;

            var entry = npcData.dailySchedule[currentScheduleIndex];
            int hour = clock.CurrentHour;

            if (hour >= entry.hourStart && hour < entry.hourEnd)
            {
                ExecuteScheduleEntry(entry);
            }
            else
            {
                // Advance to next schedule entry
                currentScheduleIndex = (currentScheduleIndex + 1) % npcData.dailySchedule.Count;
            }
        }

        private void ExecuteScheduleEntry(NPCScheduleEntry entry)
        {
            currentState = entry.activity switch
            {
                "patrol" => NPCState.Patrolling,
                "trade" => NPCState.Trading,
                "pray" => NPCState.Praying,
                "idle" => NPCState.Idle,
                _ => NPCState.Idle
            };
        }

        public bool CanBeRecruited()
        {
            if (!npcData.isRecruitable) return false;
            var rel = WorldState.Instance.GetNPCRelationship(npcData.npcId);
            return rel.trust >= 50;
        }

        protected override void OnIncapacitated()
        {
            base.OnIncapacitated();
            if (npcData.isEssential)
            {
                // Essential NPCs fall unconscious but don't die
                currentVitality = 1;
                isIncapacitated = false;
                Debug.Log($"{npcData.displayName} is essential and cannot be killed.");
            }
        }
    }

    public enum NPCState { Idle, Walking, Patrolling, Trading, Praying, Fleeing, Hostile }
}
