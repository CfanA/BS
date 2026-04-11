using UnityEngine;

namespace BS.Gameplay.Items.Data
{
    /// <summary>
    /// 道具数据。
    /// 表示可使用、可消耗、可作为解谜条件的物品定义。
    /// </summary>
    [CreateAssetMenu(menuName = "BS/Items/Item Data", fileName = "ItemData_")]
    public sealed class ItemData : ScriptableObject
    {
        [Header("基础信息")]
        [SerializeField] private string itemId = "item.sample";
        [SerializeField] private string displayName = "新道具";
        [SerializeField] [TextArea(3, 6)] private string description = "道具说明";
        [SerializeField] private Sprite icon;

        [Header("规则配置")]
        [SerializeField] private bool isUnique = true;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public bool IsUnique => isUnique;

        public bool IsValid => !string.IsNullOrWhiteSpace(itemId);
    }
}
