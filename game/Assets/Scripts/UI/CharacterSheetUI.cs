using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FadingSuns.Characters;
using FadingSuns.Data;
using FadingSuns.Core;

namespace FadingSuns.UI
{
    /// <summary>
    /// Full character sheet: portrait, attributes, skills, abilities, inventory.
    /// Toggled with a hotkey (Tab). Shows the selected party member.
    /// </summary>
    public class CharacterSheetUI : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panel;

        [Header("Identity")]
        public Image portrait;
        public TextMeshProUGUI nameLabel;
        public TextMeshProUGUI originLabel;
        public TextMeshProUGUI factionLabel;

        [Header("Attributes")]
        public TextMeshProUGUI bodyLabel;
        public TextMeshProUGUI mindLabel;
        public TextMeshProUGUI spiritLabel;
        public TextMeshProUGUI vitalityLabel;
        public TextMeshProUGUI wyrdLabel;

        [Header("Skills")]
        public Transform skillList;
        public GameObject skillRowPrefab;

        [Header("Abilities")]
        public Transform abilityList;
        public GameObject abilityButtonPrefab;

        [Header("Inventory")]
        public Transform inventoryGrid;
        public GameObject inventorySlotPrefab;

        [Header("Party Tabs")]
        public Transform partyTabContainer;
        public GameObject partyTabPrefab;

        private Character displayedCharacter;

        void Start()
        {
            panel.SetActive(false);
            BuildPartyTabs();
            PartyManager.Instance.OnMemberAdded += _ => BuildPartyTabs();
        }

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
                Toggle();
        }

        public void Toggle()
        {
            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf && displayedCharacter == null && PartyManager.Instance.Members.Count > 0)
                ShowCharacter(PartyManager.Instance.Members[0]);
        }

        private void BuildPartyTabs()
        {
            foreach (Transform child in partyTabContainer) Destroy(child.gameObject);
            foreach (var member in PartyManager.Instance.Members)
            {
                var go = Instantiate(partyTabPrefab, partyTabContainer);
                go.GetComponentInChildren<Image>().sprite = member.data.portrait;
                var cap = member;
                go.GetComponent<Button>().onClick.AddListener(() => ShowCharacter(cap));
            }
        }

        public void ShowCharacter(Character c)
        {
            displayedCharacter = c;
            var d = c.data;

            portrait.sprite = d.portrait;
            nameLabel.text = d.displayName;
            originLabel.text = d.origin.ToString().Replace("_", " ");
            factionLabel.text = d.primaryFaction;

            bodyLabel.text = $"Body: {d.body}";
            mindLabel.text = $"Mind: {d.mind}";
            spiritLabel.text = $"Spirit: {d.spirit}";
            vitalityLabel.text = $"Vitality: {c.currentVitality}/{c.maxVitality}";
            wyrdLabel.text = $"Wyrd: {c.currentWyrd}/{c.maxWyrd}";

            PopulateSkills(d.skills);
            PopulateAbilities(c.abilities);
            PopulateInventory(c.inventory);
        }

        private void PopulateSkills(SkillSet skills)
        {
            foreach (Transform child in skillList) Destroy(child.gameObject);

            AddSkillRow("Melee", skills.melee);
            AddSkillRow("Ranged", skills.ranged);
            AddSkillRow("Charm", skills.charm);
            AddSkillRow("Knavery", skills.knavery);
            AddSkillRow("Intimidate", skills.intimidate);
            AddSkillRow("Lore", skills.lore);
            AddSkillRow("Tech Redemption", skills.techRedemption);
            AddSkillRow("Occultism", skills.occultism);
            AddSkillRow("Athletics", skills.athletics);
            AddSkillRow("Stealth", skills.stealth);
            AddSkillRow("Survival", skills.survival);
            AddSkillRow("Medicine", skills.medicine);
        }

        private void AddSkillRow(string name, int value)
        {
            if (value <= 0) return; // Only show trained skills
            var go = Instantiate(skillRowPrefab, skillList);
            var labels = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (labels.Length >= 2) { labels[0].text = name; labels[1].text = value.ToString(); }
        }

        private void PopulateAbilities(AbilitySet abilities)
        {
            foreach (Transform child in abilityList) Destroy(child.gameObject);
            foreach (var ability in abilities.Unlocked)
            {
                var go = Instantiate(abilityButtonPrefab, abilityList);
                go.GetComponentInChildren<Image>().sprite = ability.icon;
                var tooltip = go.GetComponentInChildren<TextMeshProUGUI>();
                if (tooltip != null) tooltip.text = ability.displayName;
            }
        }

        private void PopulateInventory(Inventory inventory)
        {
            foreach (Transform child in inventoryGrid) Destroy(child.gameObject);
            foreach (var kvp in inventory.GetAll())
            {
                var go = Instantiate(inventorySlotPrefab, inventoryGrid);
                var labels = go.GetComponentsInChildren<TextMeshProUGUI>();
                if (labels.Length >= 2)
                {
                    var itemData = ItemManager.Instance?.GetItem(kvp.Key);
                    labels[0].text = itemData?.displayName ?? kvp.Key;
                    labels[1].text = $"x{kvp.Value}";
                }
            }
        }
    }
}
