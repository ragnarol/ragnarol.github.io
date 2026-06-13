using UnityEngine;
using FadingSuns.Core;
using FadingSuns.UI;

namespace FadingSuns.World
{
    /// <summary>
    /// Placed on a tile at the edge of a region. When the player steps on it,
    /// the travel menu opens showing available destinations.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RegionGate : MonoBehaviour
    {
        [Header("Gate Config")]
        public string sourceRegionId;
        public bool requiresConfirmation = true;

        private Region parentRegion;
        private RegionTravelUI travelUI;

        void Start()
        {
            parentRegion = GetComponentInParent<Region>();
            travelUI = FindObjectOfType<RegionTravelUI>();

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (GameManager.Instance.currentPhase != GamePhase.Exploration) return;

            var connections = parentRegion.GetAvailableConnections();
            if (connections.Count == 0)
            {
                Debug.Log("No available destinations from here.");
                return;
            }

            if (requiresConfirmation && travelUI != null)
                travelUI.Show(connections);
            else if (connections.Count == 1)
                GameManager.Instance.TravelToRegion(connections[0].targetRegionId);
        }
    }
}
