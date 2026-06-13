using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FadingSuns.Data;

namespace FadingSuns.UI
{
    /// <summary>
    /// New Game screen. Shows three archetypes side by side.
    /// Player reads the flavour text, then confirms their choice.
    /// Shown before the bootstrap loads the City scene.
    /// </summary>
    public class CharacterCreationUI : MonoBehaviour
    {
        [System.Serializable]
        public class ArchetypeCard
        {
            public GameObject cardRoot;
            public Image portrait;
            public TextMeshProUGUI nameLabel;
            public TextMeshProUGUI descriptionLabel;
            public TextMeshProUGUI statsLabel;
            public Button selectButton;
        }

        public ArchetypeCard[] cards;
        public Button confirmButton;
        public TextMeshProUGUI confirmLabel;

        private Core.CharacterCreation creation;
        private int selectedIndex = -1;

        void Start()
        {
            creation = FindObjectOfType<Core.CharacterCreation>();
            confirmButton.interactable = false;

            for (int i = 0; i < cards.Length; i++)
            {
                int idx = i;
                cards[i].selectButton.onClick.AddListener(() => SelectCard(idx));

                if (i < creation.Archetypes.Count)
                    PopulateCard(cards[i], creation.Archetypes[i]);
            }

            confirmButton.onClick.AddListener(Confirm);
        }

        private void PopulateCard(ArchetypeCard card, CharacterData data)
        {
            card.portrait.sprite = data.portrait;
            card.nameLabel.text = data.displayName;
            card.descriptionLabel.text = data.background;
            card.statsLabel.text =
                $"Body {data.body}  Mind {data.mind}  Spirit {data.spirit}\n" +
                $"Faction: {data.primaryFaction}\n" +
                (data.occultType != OccultType.None ? $"Occult: {data.occultType}" : "No occult talent");
        }

        private void SelectCard(int index)
        {
            selectedIndex = index;
            for (int i = 0; i < cards.Length; i++)
            {
                var outline = cards[i].cardRoot.GetComponent<Outline>();
                if (outline != null) outline.enabled = (i == index);
            }
            confirmButton.interactable = true;
            confirmLabel.text = $"Begin as {creation.Archetypes[index].displayName}?";
        }

        private void Confirm()
        {
            if (selectedIndex < 0) return;
            creation.ChooseArchetype(selectedIndex);
            gameObject.SetActive(false);
        }
    }
}
