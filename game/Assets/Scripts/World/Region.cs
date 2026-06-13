using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Core;
using FadingSuns.Characters;

namespace FadingSuns.World
{
    /// <summary>
    /// Base class for each game region (City, Spaceport, Palace, Wilderness).
    /// Each region is a separate Unity scene with its own tilemap and NPC roster.
    /// </summary>
    public abstract class Region : MonoBehaviour
    {
        [Header("Region Identity")]
        public string regionId;
        public string displayName;
        public string ambientMusicId;

        [Header("Entry Points")]
        public List<RegionEntryPoint> entryPoints = new();

        [Header("NPCs")]
        [SerializeField] protected List<NPC> regionNPCs = new();

        [Header("Interactive Objects")]
        [SerializeField] protected List<InteractableObject> interactables = new();

        [Header("Travel Connections")]
        public List<RegionConnection> connections = new();

        protected IsometricTileMap tileMap;

        protected virtual void Start()
        {
            tileMap = GetComponentInChildren<IsometricTileMap>();
            SpawnNPCs();
            OnRegionEntered();
        }

        protected virtual void SpawnNPCs()
        {
            // NPCs are placed via scene hierarchy, not runtime spawned
            // This registers them with the region
            regionNPCs = new List<NPC>(GetComponentsInChildren<NPC>());
        }

        protected virtual void OnRegionEntered() { }

        public Vector3 GetEntryPoint(string fromRegion)
        {
            var ep = entryPoints.Find(e => e.fromRegionId == fromRegion);
            return ep != null ? ep.transform.position : Vector3.zero;
        }

        public List<RegionConnection> GetAvailableConnections()
        {
            var available = new List<RegionConnection>();
            foreach (var conn in connections)
            {
                if (!conn.requiresUnlock ||
                    WorldState.Instance.IsRegionUnlocked(conn.targetRegionId))
                    available.Add(conn);
            }
            return available;
        }
    }

    [System.Serializable]
    public class RegionConnection
    {
        public string targetRegionId;
        public string displayName;    // e.g. "Head to the Spaceport"
        public bool requiresUnlock;
        public string unlockMilestoneId;
        public Vector3Int gateTile;   // Tile the player steps on to trigger travel
    }

    [System.Serializable]
    public class RegionEntryPoint : MonoBehaviour
    {
        public string fromRegionId;
    }

    // ── Region Implementations ──────────────────────────────────────────────

    public class CityRegion : Region
    {
        protected override void OnRegionEntered()
        {
            Debug.Log("Entered: City — the heart of noble intrigue and guild commerce.");
        }
    }

    public class SpaceportRegion : Region
    {
        protected override void OnRegionEntered()
        {
            Debug.Log("Entered: Spaceport / Agora — crossroads of the known worlds.");
        }
    }

    public class PalaceRegion : Region
    {
        [Header("Palace Specific")]
        public string rulingHouse;

        protected override void OnRegionEntered()
        {
            Debug.Log($"Entered: Palace of House {rulingHouse}. Mind your courtly manners.");
        }
    }

    public class WildernessRegion : Region
    {
        [Header("Wilderness")]
        public bool hasRandomEncounters = true;
        public float encounterChancePerStep = 0.05f;

        protected override void OnRegionEntered()
        {
            Debug.Log("Entered: Wilderness — the ancient, alien lands beyond city walls.");
        }
    }
}
