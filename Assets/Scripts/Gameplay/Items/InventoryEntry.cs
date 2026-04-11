using System;
using BS.Gameplay.Items.Data;

namespace BS.Gameplay.Items
{
    /// <summary>
    /// 背包中的单条道具记录。
    /// 运行时保存道具定义和数量。
    /// </summary>
    [Serializable]
    public sealed class InventoryEntry
    {
        public InventoryEntry(ItemData itemData, int count)
        {
            ItemData = itemData;
            Count = count;
        }

        public ItemData ItemData { get; }
        public int Count { get; private set; }

        public void SetCount(int count)
        {
            Count = count;
        }
    }
}
