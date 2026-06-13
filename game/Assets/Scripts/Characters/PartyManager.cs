using UnityEngine;
using System.Collections.Generic;

namespace FadingSuns.Characters
{
    /// <summary>
    /// Manages the player's party. Starts with 1 member; additional slots
    /// unlock through narrative milestones (max 4).
    /// </summary>
    public class PartyManager : MonoBehaviour
    {
        public static PartyManager Instance { get; private set; }

        [Header("Party")]
        public PlayerCharacter leader;
        private List<Character> members = new();
        private int unlockedSlots = 1; // Starts with 1, max 4

        public IReadOnlyList<Character> Members => members.AsReadOnly();
        public int MaxPartySize => 4;
        public int CurrentSize => members.Count;

        public event System.Action<Character> OnMemberAdded;
        public event System.Action<Character> OnMemberRemoved;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool TryAddMember(Character character)
        {
            if (members.Count >= unlockedSlots)
            {
                Debug.Log("Party is full. Complete milestones to unlock more slots.");
                return false;
            }
            if (members.Contains(character)) return false;

            members.Add(character);
            OnMemberAdded?.Invoke(character);
            return true;
        }

        public void RemoveMember(Character character)
        {
            if (character == leader) return; // Leader cannot be removed
            members.Remove(character);
            OnMemberRemoved?.Invoke(character);
        }

        public void UnlockPartySlot()
        {
            unlockedSlots = Mathf.Min(unlockedSlots + 1, MaxPartySize);
            Debug.Log($"Party slot unlocked. Slots available: {unlockedSlots}/{MaxPartySize}");
        }

        public List<Character> GetActiveMembers() => members.FindAll(m => !m.isIncapacitated);

        public bool AllIncapacitated() => members.TrueForAll(m => m.isIncapacitated);

        /// <summary>Returns best skill value among all party members for party checks.</summary>
        public int GetBestSkill(string skillName)
        {
            int best = 0;
            foreach (var m in members)
                best = Mathf.Max(best, m.GetSkillValue(skillName));
            return best;
        }

        public void HealAll(int amount)
        {
            foreach (var m in members) m.Heal(amount);
        }
    }
}
