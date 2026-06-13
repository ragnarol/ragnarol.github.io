using UnityEngine;
using FadingSuns.Data;

namespace FadingSuns.Characters
{
    public class PlayerCharacter : Character
    {
        [Header("Player Specific")]
        public bool isLeader = false;

        // The player's character unlocks additional background options
        // through milestones, not level-up menus
        protected override void Awake()
        {
            base.Awake();
        }

        public void OnMilestoneUnlocked(string milestoneId)
        {
            // Specific player character reactions to milestones
            // (e.g., unique dialogue options become available)
            Debug.Log($"Player character acknowledged milestone: {milestoneId}");
        }
    }
}
