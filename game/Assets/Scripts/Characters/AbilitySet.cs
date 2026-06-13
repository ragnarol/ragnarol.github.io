using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Data;

namespace FadingSuns.Characters
{
    public class AbilitySet : MonoBehaviour
    {
        private List<AbilityData> unlocked = new();

        public IReadOnlyList<AbilityData> Unlocked => unlocked.AsReadOnly();

        public void UnlockAbility(string abilityId)
        {
            var data = Resources.Load<AbilityData>($"Abilities/{abilityId}");
            if (data == null) { Debug.LogWarning($"Ability not found: {abilityId}"); return; }
            if (!unlocked.Exists(a => a.abilityId == abilityId))
                unlocked.Add(data);
        }

        public bool HasAbility(string abilityId) =>
            unlocked.Exists(a => a.abilityId == abilityId);

        public AbilityData GetAbility(string abilityId) =>
            unlocked.Find(a => a.abilityId == abilityId);
    }
}
