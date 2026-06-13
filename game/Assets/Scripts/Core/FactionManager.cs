using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Data;

namespace FadingSuns.Core
{
    public class FactionManager : MonoBehaviour
    {
        public static FactionManager Instance { get; private set; }

        [SerializeField] private List<FactionData> factions = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public FactionData GetFaction(string id) => factions.Find(f => f.id == id);

        // When rep changes with one faction, it may affect others (e.g., helping nobles hurts rebels)
        public void ModifyReputationWithConsequences(string factionId, int delta)
        {
            WorldState.Instance.ModifyReputation(factionId, delta);

            var faction = GetFaction(factionId);
            if (faction == null) return;

            foreach (var rivalry in faction.rivalFactions)
                WorldState.Instance.ModifyReputation(rivalry, -Mathf.RoundToInt(delta * 0.5f));
            foreach (var ally in faction.alliedFactions)
                WorldState.Instance.ModifyReputation(ally, Mathf.RoundToInt(delta * 0.25f));
        }
    }
}
