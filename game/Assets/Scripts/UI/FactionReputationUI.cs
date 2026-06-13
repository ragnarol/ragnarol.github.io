using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FadingSuns.Core;

namespace FadingSuns.UI
{
    /// <summary>
    /// Displays the party's standing with all major factions.
    /// Accessible from the main menu (F key).
    /// </summary>
    public class FactionReputationUI : MonoBehaviour
    {
        [System.Serializable]
        public class FactionRow
        {
            public string factionId;
            public string displayName;
            public Image emblemImage;
            public Image reputationBar;
            public TextMeshProUGUI standingLabel;
            public TextMeshProUGUI valueLabel;
        }

        public GameObject panel;
        public FactionRow[] rows;

        // Standing bar color gradient: red (enemy) → yellow (neutral) → green (allied)
        private static readonly Color colorEnemy = new(0.8f, 0.1f, 0.1f);
        private static readonly Color colorHostile = new(0.9f, 0.4f, 0.1f);
        private static readonly Color colorNeutral = new(0.8f, 0.8f, 0.2f);
        private static readonly Color colorFriendly = new(0.4f, 0.8f, 0.3f);
        private static readonly Color colorAllied = new(0.1f, 0.9f, 0.4f);

        void Start()
        {
            panel.SetActive(false);
        }

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame)
                Toggle();
        }

        public void Toggle()
        {
            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf) Refresh();
        }

        private void Refresh()
        {
            foreach (var row in rows)
            {
                int rep = WorldState.Instance.GetReputation(row.factionId);
                var standing = WorldState.Instance.GetStanding(row.factionId);

                // Bar goes 0–1 where 50% = 0 rep
                row.reputationBar.fillAmount = (rep + 100) / 200f;
                row.reputationBar.color = StandingColor(standing);
                row.standingLabel.text = standing.ToString();
                row.valueLabel.text = rep.ToString("+#;-#;0");
            }
        }

        private Color StandingColor(FactionStanding s) => s switch
        {
            FactionStanding.Allied => colorAllied,
            FactionStanding.Friendly => colorFriendly,
            FactionStanding.Neutral => colorNeutral,
            FactionStanding.Hostile => colorHostile,
            FactionStanding.Enemy => colorEnemy,
            _ => colorNeutral
        };
    }
}
