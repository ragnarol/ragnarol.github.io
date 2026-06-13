using UnityEngine;
using System.Collections.Generic;

namespace FadingSuns.Data
{
    [CreateAssetMenu(fileName = "NPCData", menuName = "FadingSuns/NPC Data")]
    public class NPCData : ScriptableObject
    {
        [Header("Identity")]
        public string npcId;
        public string displayName;
        public string title;          // "Baron", "Brother", "Guildmaster"
        public string occupation;
        public CharacterOrigin origin;
        public Sprite portrait;
        public RuntimeAnimatorController animatorController;

        [Header("Faction & Role")]
        public string faction;
        public NPCRole role;
        public bool isRecruitable;    // Can join the party?
        public bool isEssential;      // Cannot be killed

        [Header("Location")]
        public string homeRegion;     // "City", "Spaceport", "Palace", "Wilderness"
        public string homeNodeId;     // Specific tile/node in the region

        [Header("Dialogue")]
        public DialogueProfile dialogueProfile;

        [Header("Stats (for combat or skill checks)")]
        public int body = 4;
        public int mind = 4;
        public int spirit = 4;
        public int vitality = 15;

        [Header("Schedule")]
        public List<NPCScheduleEntry> dailySchedule = new();
    }

    public enum NPCRole
    {
        Merchant,
        Guard,
        Noble,
        Priest,
        Scholar,
        Criminal,
        Informant,
        QuestGiver,
        Companion,   // Recruitable
        Enemy,
        Neutral,
        Alien
    }

    [System.Serializable]
    public class NPCScheduleEntry
    {
        public int hourStart;  // 0-23
        public int hourEnd;
        public string locationNodeId;
        public string activity; // "idle", "trade", "pray", "patrol"
    }
}
