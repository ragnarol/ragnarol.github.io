using UnityEngine;
using TMPro;
using FadingSuns.Core;
using FadingSuns.World;

namespace FadingSuns.UI
{
    public class HUD : MonoBehaviour
    {
        [Header("Info")]
        public TextMeshProUGUI regionLabel;
        public TextMeshProUGUI clockLabel;
        public TextMeshProUGUI factionLabel;

        [Header("Milestone Toast")]
        public GameObject milestoneToast;
        public TextMeshProUGUI milestoneToastText;

        private GameClock clock;

        void Start()
        {
            clock = FindObjectOfType<GameClock>();

            GameManager.Instance.OnRegionChanged += UpdateRegionLabel;
            GameManager.Instance.milestoneManager.OnMilestoneCompleted += ShowMilestoneToast;
        }

        void Update()
        {
            if (clock != null)
                clockLabel.text = clock.GetTimeString();
        }

        private void UpdateRegionLabel(string regionId)
        {
            regionLabel.text = regionId.Replace("_", " ");
        }

        private void ShowMilestoneToast(Data.MilestoneData m)
        {
            milestoneToastText.text = $"Milestone: {m.displayName}";
            milestoneToast.SetActive(true);
            Invoke(nameof(HideToast), 4f);
        }

        private void HideToast() => milestoneToast.SetActive(false);

        void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRegionChanged -= UpdateRegionLabel;
                GameManager.Instance.milestoneManager.OnMilestoneCompleted -= ShowMilestoneToast;
            }
        }
    }
}
