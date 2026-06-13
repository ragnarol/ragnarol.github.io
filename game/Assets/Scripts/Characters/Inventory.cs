using UnityEngine;
using System.Collections.Generic;

namespace FadingSuns.Characters
{
    public class Inventory : MonoBehaviour
    {
        private Dictionary<string, int> items = new();

        public event System.Action<string, int> OnItemChanged;

        public void AddItem(string itemId, int count = 1)
        {
            if (!items.ContainsKey(itemId)) items[itemId] = 0;
            items[itemId] += count;
            OnItemChanged?.Invoke(itemId, items[itemId]);
        }

        public bool RemoveItem(string itemId, int count = 1)
        {
            if (!items.TryGetValue(itemId, out var current) || current < count) return false;
            items[itemId] -= count;
            if (items[itemId] <= 0) items.Remove(itemId);
            OnItemChanged?.Invoke(itemId, items.GetValueOrDefault(itemId));
            return true;
        }

        public bool HasItem(string itemId, int count = 1) =>
            items.TryGetValue(itemId, out var c) && c >= count;

        public int GetItemCount(string itemId) =>
            items.TryGetValue(itemId, out var c) ? c : 0;

        public Dictionary<string, int> GetAll() => new Dictionary<string, int>(items);
    }
}
