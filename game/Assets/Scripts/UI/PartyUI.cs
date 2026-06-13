using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FadingSuns.Characters;

namespace FadingSuns.UI
{
    /// <summary>
    /// Shows up to 4 party member portraits along the bottom of the screen.
    /// Locked slots appear as padlocked frames — unlocked by milestones.
    /// </summary>
    public class PartyUI : MonoBehaviour
    {
        [Header("Slot Prefab")]
        public GameObject partySlotPrefab;
        public Transform slotContainer;

        private List<PartySlotWidget> slots = new();

        void Start()
        {
            var pm = PartyManager.Instance;
            pm.OnMemberAdded += RefreshParty;
            pm.OnMemberRemoved += RefreshParty;

            // Create 4 slot widgets
            for (int i = 0; i < 4; i++)
            {
                var go = Instantiate(partySlotPrefab, slotContainer);
                slots.Add(go.GetComponent<PartySlotWidget>());
            }

            RefreshParty(null);
        }

        private void RefreshParty(Characters.Character _)
        {
            var pm = PartyManager.Instance;
            var members = pm.Members;

            for (int i = 0; i < 4; i++)
            {
                bool hasSlot = i < pm.Members.Count + (4 - pm.MaxPartySize);
                // Unlock status is reflected by milestone progression
                bool unlocked = i == 0; // First slot always unlocked

                if (i < members.Count)
                    slots[i].SetCharacter(members[i], true);
                else
                    slots[i].SetEmpty(unlocked);
            }
        }

        void OnDestroy()
        {
            var pm = PartyManager.Instance;
            if (pm == null) return;
            pm.OnMemberAdded -= RefreshParty;
            pm.OnMemberRemoved -= RefreshParty;
        }
    }

    public class PartySlotWidget : MonoBehaviour
    {
        public Image portrait;
        public Image healthBar;
        public Image wyrdBar;
        public TextMeshProUGUI nameLabel;
        public GameObject lockedOverlay;
        public GameObject incapacitatedOverlay;

        private Characters.Character tracked;

        public void SetCharacter(Characters.Character c, bool unlocked)
        {
            tracked = c;
            gameObject.SetActive(true);
            lockedOverlay.SetActive(false);
            portrait.sprite = c.data.portrait;
            nameLabel.text = c.data.displayName;
            UpdateBars();
        }

        public void SetEmpty(bool unlocked)
        {
            tracked = null;
            portrait.sprite = null;
            nameLabel.text = unlocked ? "Empty" : "???";
            lockedOverlay.SetActive(!unlocked);
            incapacitatedOverlay.SetActive(false);
            healthBar.fillAmount = 0;
            wyrdBar.fillAmount = 0;
        }

        private void Update()
        {
            if (tracked != null) UpdateBars();
        }

        private void UpdateBars()
        {
            if (tracked == null) return;
            healthBar.fillAmount = (float)tracked.currentVitality / tracked.maxVitality;
            wyrdBar.fillAmount = (float)tracked.currentWyrd / tracked.maxWyrd;
            incapacitatedOverlay.SetActive(tracked.isIncapacitated);
        }
    }
}
