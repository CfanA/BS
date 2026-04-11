using System;
using System.Collections.Generic;
using BS.Gameplay.Items.Data;
using UnityEngine;

namespace BS.Gameplay.Items
{
    /// <summary>
    /// 道具背包管理器。
    /// 负责拾取、去重、查询、移除，以及为后续 UI 和存档提供统一入口。
    /// </summary>
    public sealed class InventoryManager : MonoBehaviour
    {
        private readonly Dictionary<string, InventoryEntry> _entriesById = new();
        private readonly List<InventoryEntry> _entries = new();

        public event Action<ItemData, int> ItemAdded;
        public event Action<ItemData, int> ItemRemoved;
        public event Action InventoryChanged;

        public IReadOnlyList<InventoryEntry> Entries => _entries;

        public bool TryAddItem(ItemData itemData, int amount = 1)
        {
            if (!IsValidRequest(itemData, amount))
            {
                return false;
            }

            if (_entriesById.TryGetValue(itemData.ItemId, out var existingEntry))
            {
                if (itemData.IsUnique)
                {
                    return false;
                }

                existingEntry.SetCount(existingEntry.Count + amount);
                ItemAdded?.Invoke(itemData, amount);
                InventoryChanged?.Invoke();
                return true;
            }

            var entry = new InventoryEntry(itemData, itemData.IsUnique ? 1 : amount);
            _entriesById.Add(itemData.ItemId, entry);
            _entries.Add(entry);

            ItemAdded?.Invoke(itemData, entry.Count);
            InventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(ItemData itemData, int amount = 1)
        {
            if (!IsValidRequest(itemData, amount))
            {
                return false;
            }

            if (!_entriesById.TryGetValue(itemData.ItemId, out var entry))
            {
                return false;
            }

            if (entry.Count < amount)
            {
                return false;
            }

            var newCount = entry.Count - amount;
            if (newCount <= 0)
            {
                _entriesById.Remove(itemData.ItemId);
                _entries.Remove(entry);
            }
            else
            {
                entry.SetCount(newCount);
            }

            ItemRemoved?.Invoke(itemData, amount);
            InventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(ItemData itemData, int amount = 1)
        {
            return itemData != null && HasItem(itemData.ItemId, amount);
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            return _entriesById.TryGetValue(itemId, out var entry) && entry.Count >= amount;
        }

        public int GetItemCount(ItemData itemData)
        {
            return itemData == null ? 0 : GetItemCount(itemData.ItemId);
        }

        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            return _entriesById.TryGetValue(itemId, out var entry) ? entry.Count : 0;
        }

        public ItemData GetItemData(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            return _entriesById.TryGetValue(itemId, out var entry) ? entry.ItemData : null;
        }

        public void ClearAll()
        {
            _entriesById.Clear();
            _entries.Clear();
            InventoryChanged?.Invoke();
        }

        private static bool IsValidRequest(ItemData itemData, int amount)
        {
            return itemData != null && itemData.IsValid && amount > 0;
        }
    }
}
