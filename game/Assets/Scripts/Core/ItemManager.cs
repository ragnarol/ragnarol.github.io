using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Data;
using FadingSuns.Characters;

namespace FadingSuns.Core
{
    /// <summary>
    /// Resolves item IDs to ItemData assets and applies consumable effects.
    /// Also handles the Church's tech-heresy detection mechanic.
    /// </summary>
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }

        private Dictionary<string, ItemData> itemRegistry = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Load all items from Resources at startup
            var all = Resources.LoadAll<ItemData>("Items");
            foreach (var item in all)
                itemRegistry[item.itemId] = item;
        }

        public ItemData GetItem(string itemId) =>
            itemRegistry.TryGetValue(itemId, out var d) ? d : null;

        public bool UseItem(string itemId, Character user)
        {
            var item = GetItem(itemId);
            if (item == null) return false;
            if (!item.isConsumable) return false;
            if (!user.inventory.HasItem(itemId)) return false;

            ApplyConsumableEffect(item, user);
            user.inventory.RemoveItem(itemId);

            // Tech heresy check: energy weapons used in public draw Church attention
            if (item.isTechHeresy)
                CheckTechHeresy(user);

            return true;
        }

        private void ApplyConsumableEffect(ItemData item, Character user)
        {
            switch (item.consumeEffectId)
            {
                case "heal":
                    user.Heal(item.consumeValue);
                    break;
                case "restore_wyrd":
                    user.currentWyrd = Mathf.Min(user.maxWyrd, user.currentWyrd + item.consumeValue);
                    break;
                case "remove_status":
                    user.activeEffects.Clear();
                    break;
                default:
                    Debug.LogWarning($"Unknown consumable effect: {item.consumeEffectId}");
                    break;
            }
        }

        /// <summary>
        /// Using tech-heresy items in populated regions risks Church investigation.
        /// </summary>
        private void CheckTechHeresy(Character user)
        {
            string region = GameManager.Instance.currentRegion;
            bool isPublic = region == "City" || region == "Palace" || region == "Spaceport";
            if (!isPublic) return;

            float roll = Random.value;
            if (roll < 0.3f)
            {
                FactionManager.Instance.ModifyReputationWithConsequences("UniversalChurch", -5);
                Debug.Log("A Church inquisitor takes note of your tech heresy...");
                WorldState.Instance.SetFlag("church_suspicious", true);
            }
        }

        public List<ItemData> GetLootTable(string tableId)
        {
            // In a real project, loot tables are ScriptableObjects or JSON.
            // Here we return a stub.
            Debug.Log($"Resolving loot table: {tableId}");
            return new List<ItemData>();
        }
    }
}
