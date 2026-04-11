using System.Collections.Generic;
using BS.Gameplay.Clues.Data;
using BS.Gameplay.Items.Data;
using UnityEngine;

namespace BS.Gameplay.Data
{
    /// <summary>
    /// 运行时数据注册表。
    /// 负责按稳定 ID 找回 ScriptableObject 资源，供存档恢复使用。
    /// 当前最小方案基于 Resources.LoadAll，因此需要把可恢复的数据资源放在 Resources 下。
    /// </summary>
    public sealed class GameDataRegistry : MonoBehaviour
    {
        private readonly Dictionary<string, ItemData> _itemsById = new();
        private readonly Dictionary<string, ClueData> _cluesById = new();

        private bool _isInitialized;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            RegisterItems(Resources.LoadAll<ItemData>(string.Empty));
            RegisterClues(Resources.LoadAll<ClueData>(string.Empty));
            _isInitialized = true;
        }

        public ItemData GetItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            Initialize();
            return _itemsById.TryGetValue(itemId, out var itemData) ? itemData : null;
        }

        public ClueData GetClue(string clueId)
        {
            if (string.IsNullOrWhiteSpace(clueId))
            {
                return null;
            }

            Initialize();
            return _cluesById.TryGetValue(clueId, out var clueData) ? clueData : null;
        }

        private void RegisterItems(ItemData[] items)
        {
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null || !item.IsValid || _itemsById.ContainsKey(item.ItemId))
                {
                    continue;
                }

                _itemsById.Add(item.ItemId, item);
            }
        }

        private void RegisterClues(ClueData[] clues)
        {
            for (var i = 0; i < clues.Length; i++)
            {
                var clue = clues[i];
                if (clue == null || !clue.IsValid || _cluesById.ContainsKey(clue.ClueId))
                {
                    continue;
                }

                _cluesById.Add(clue.ClueId, clue);
            }
        }
    }
}
