using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FadingSuns.Core;
using FadingSuns.World;

namespace FadingSuns.UI
{
    /// <summary>
    /// Shown when the player approaches a region boundary.
    /// Lists unlocked destinations with lore-appropriate names.
    /// </summary>
    public class RegionTravelUI : MonoBehaviour
    {
        public GameObject travelPanel;
        public Transform buttonContainer;
        public GameObject destinationButtonPrefab;
        public TextMeshProUGUI titleLabel;

        public void Show(List<RegionConnection> connections)
        {
            travelPanel.SetActive(true);
            titleLabel.text = "Where do you wish to go?";

            foreach (Transform child in buttonContainer)
                Destroy(child.gameObject);

            foreach (var conn in connections)
            {
                var go = Instantiate(destinationButtonPrefab, buttonContainer);
                go.GetComponentInChildren<TextMeshProUGUI>().text = conn.displayName;
                string targetId = conn.targetRegionId;
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Hide();
                    GameManager.Instance.TravelToRegion(targetId);
                });
            }

            // Cancel button
            var cancelGo = Instantiate(destinationButtonPrefab, buttonContainer);
            cancelGo.GetComponentInChildren<TextMeshProUGUI>().text = "Stay here";
            cancelGo.GetComponent<Button>().onClick.AddListener(Hide);
        }

        public void Hide() => travelPanel.SetActive(false);
    }
}
