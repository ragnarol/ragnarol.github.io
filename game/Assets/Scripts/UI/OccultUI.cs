using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FadingSuns.Occult;
using FadingSuns.Characters;
using FadingSuns.Data;

namespace FadingSuns.UI
{
    /// <summary>
    /// Power selection menu for Psi and Theurgy.
    /// Only shown when the active character has occult abilities.
    /// </summary>
    public class OccultUI : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panel;
        public TextMeshProUGUI wyrdLabel;

        [Header("Power List")]
        public Transform powerListContainer;
        public GameObject powerButtonPrefab;

        [Header("Target Selection")]
        public TextMeshProUGUI instructionLabel;

        private Character caster;
        private string pendingPowerId;
        private OccultType pendingType;

        public void Open(Character character)
        {
            caster = character;
            panel.SetActive(true);
            wyrdLabel.text = $"Wyrd: {character.currentWyrd}/{character.maxWyrd}";
            PopulatePowers();
        }

        private void PopulatePowers()
        {
            foreach (Transform child in powerListContainer) Destroy(child.gameObject);

            if (caster.data.occultType == OccultType.Psi)
                PopulatePsiPowers();
            else if (caster.data.occultType == OccultType.Theurgy)
                PopulateTheurgyRites();

            instructionLabel.text = "Select a power, then click a target.";
        }

        private void PopulatePsiPowers()
        {
            // Powers are listed via OccultSystem — here we read from data
            var powers = OccultSystem.Instance;
            // In a full project OccultSystem exposes a GetPsiPowers() method;
            // we demonstrate with stub entries for the key Fading Suns disciplines.
            string[] ids = { "telepathy", "telekinesis", "psi_shield", "empathy", "farsight" };
            string[] names = { "Telepathy", "Telekinesis", "Psychic Shield", "Empathy", "Farsight" };
            for (int i = 0; i < ids.Length; i++)
                AddPowerButton(ids[i], names[i], OccultType.Psi);
        }

        private void PopulateTheurgyRites()
        {
            string[] ids = { "rite_heal", "rite_ward", "rite_bless", "rite_sanctify", "rite_reveal" };
            string[] names = { "Laying on Hands", "Ward of Pancreator", "Blessing", "Sanctify", "Revelation" };
            for (int i = 0; i < ids.Length; i++)
                AddPowerButton(ids[i], names[i], OccultType.Theurgy);
        }

        private void AddPowerButton(string id, string label, OccultType type)
        {
            var go = Instantiate(powerButtonPrefab, powerListContainer);
            go.GetComponentInChildren<TextMeshProUGUI>().text = label;
            string capturedId = id;
            OccultType capturedType = type;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                pendingPowerId = capturedId;
                pendingType = capturedType;
                instructionLabel.text = $"Click a target to use {label}.";
            });
        }

        public void ConfirmOnTarget(Character target)
        {
            if (string.IsNullOrEmpty(pendingPowerId)) return;

            OccultResult result = pendingType == OccultType.Psi
                ? OccultSystem.Instance.ActivatePsi(caster, target, pendingPowerId)
                : OccultSystem.Instance.PerformTheurgy(caster, target, pendingPowerId);

            instructionLabel.text = result.message;
            wyrdLabel.text = $"Wyrd: {caster.currentWyrd}/{caster.maxWyrd}";
            pendingPowerId = null;
        }

        public void Close()
        {
            panel.SetActive(false);
            caster = null;
            pendingPowerId = null;
        }
    }
}
