using UnityEngine;
using System.Collections.Generic;

namespace FadingSuns.Data
{
    [CreateAssetMenu(fileName = "DialogueProfile", menuName = "FadingSuns/Dialogue Profile")]
    public class DialogueProfile : ScriptableObject
    {
        [Header("Greeting")]
        public string greetingDefault;
        public string greetingFriendly;
        public string greetingHostile;

        [Header("Farewell")]
        public string farewellDefault;

        [Header("Unknown Keyword")]
        public string unknownKeywordResponse;
        // e.g. "I know not of what you speak."

        [Header("Keywords")]
        public List<DialogueKeyword> keywords = new();
    }

    /// <summary>
    /// An Ultima-style keyword entry. The player types (or clicks) a word,
    /// the NPC matches it against their keyword list and responds.
    /// </summary>
    [System.Serializable]
    public class DialogueKeyword
    {
        public string keyword;          // Canonical form, lowercase
        public List<string> aliases;    // Alternate forms ("job" == "work" == "occupation")

        [TextArea(2, 5)]
        public string response;

        // Conditions
        public string requiredFlag;             // World flag that must be set
        public string requiredMilestone;        // Milestone that must be complete
        public int requiredMinReputation = -100; // Min faction rep to trigger
        public string reputationFaction;

        // Consequences
        public List<string> grantsKeywords;     // New keywords NPC mentions in response
        public string setsFlag;
        public string completeMilestoneId;
        public int reputationDelta;
        public string reputationFactionTarget;

        // Follow-up / branching
        public bool endsConversation = false;
    }

    [System.Serializable]
    public class MilestoneData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea(1, 3)]
        public string description;
        public List<string> prerequisites = new();
        public List<MilestoneReward> rewards = new();
    }

    [System.Serializable]
    public class MilestoneReward
    {
        public RewardType type;
        public string stringValue;
        public int intValue;
    }

    public enum RewardType
    {
        UnlockPartySlot,
        UnlockRegion,
        GrantAbility,
        GrantItem,
        SetFlag
    }
}
