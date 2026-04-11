using UnityEngine;

namespace BS.Gameplay.Clues.Data
{
    /// <summary>
    /// 线索数据。
    /// 表示仅用于叙事记录、推理、案件整理的发现内容。
    /// </summary>
    [CreateAssetMenu(menuName = "BS/Clues/Clue Data", fileName = "ClueData_")]
    public sealed class ClueData : ScriptableObject
    {
        [Header("基础信息")]
        [SerializeField] private string clueId = "clue.sample";
        [SerializeField] private string displayName = "新线索";
        [SerializeField] [TextArea(3, 6)] private string description = "线索说明";
        [SerializeField] private Sprite icon;

        [Header("案件簿信息")]
        [SerializeField] private string category = "默认分类";

        public string ClueId => clueId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public string Category => category;

        public bool IsValid => !string.IsNullOrWhiteSpace(clueId);
    }
}
