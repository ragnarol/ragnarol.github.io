using UnityEngine;
using System.Collections.Generic;

namespace FadingSuns.Core
{
    /// <summary>
    /// Progress is driven by narrative milestones, not XP or levels.
    /// Milestones unlock abilities, party slots, regions, and story beats.
    /// </summary>
    public class MilestoneManager : MonoBehaviour
    {
        [SerializeField] private List<MilestoneData> allMilestones = new();
        private HashSet<string> completedMilestones = new();

        public event System.Action<MilestoneData> OnMilestoneCompleted;

        public bool IsMilestoneComplete(string id) => completedMilestones.Contains(id);

        public void CompleteAMilestone(string id)
        {
            if (completedMilestones.Contains(id)) return;

            MilestoneData m = allMilestones.Find(x => x.id == id);
            if (m == null) { Debug.LogWarning($"Milestone not found: {id}"); return; }

            completedMilestones.Add(id);
            ApplyMilestoneRewards(m);
            OnMilestoneCompleted?.Invoke(m);
            Debug.Log($"Milestone completed: {m.displayName}");
        }

        private void ApplyMilestoneRewards(MilestoneData m)
        {
            var party = PartyManager.Instance;
            foreach (var reward in m.rewards)
            {
                switch (reward.type)
                {
                    case RewardType.UnlockPartySlot:
                        party.UnlockPartySlot();
                        break;
                    case RewardType.UnlockRegion:
                        WorldState.Instance.UnlockRegion(reward.stringValue);
                        break;
                    case RewardType.GrantAbility:
                        party.Leader?.abilities.UnlockAbility(reward.stringValue);
                        break;
                    case RewardType.GrantItem:
                        party.Leader?.inventory.AddItem(reward.stringValue, 1);
                        break;
                    case RewardType.SetFlag:
                        WorldState.Instance.SetFlag(reward.stringValue, true);
                        break;
                }
            }
        }

        public List<MilestoneData> GetAvailableMilestones()
        {
            var available = new List<MilestoneData>();
            foreach (var m in allMilestones)
            {
                if (completedMilestones.Contains(m.id)) continue;
                if (ArePrerequistitesMet(m)) available.Add(m);
            }
            return available;
        }

        private bool ArePrerequistitesMet(MilestoneData m)
        {
            foreach (var prereq in m.prerequisites)
                if (!completedMilestones.Contains(prereq)) return false;
            return true;
        }

        // Save/Load
        public MilestoneSaveData GetSaveData() =>
            new MilestoneSaveData { completed = new List<string>(completedMilestones) };

        public void LoadSaveData(MilestoneSaveData data)
        {
            completedMilestones = new HashSet<string>(data.completed);
        }
    }

    [System.Serializable]
    public class MilestoneSaveData
    {
        public List<string> completed = new();
    }
}
