using BS.Foundation.Ids;
using UnityEngine;

namespace BS.Gameplay.Story.Data
{
    /// <summary>
    /// 剧情 Flag 定义资源。
    /// 通过资源定义稳定 ID，避免场景逻辑大量使用裸字符串。
    /// </summary>
    [CreateAssetMenu(menuName = "BS/Story/Flag Definition", fileName = "Flag_")]
    public sealed class FlagDefinition : ScriptableObject
    {
        [Header("基础信息")]
        [SerializeField] private string flagId = "story.sample_flag";
        [SerializeField] private string displayName = "新剧情标记";
        [SerializeField] [TextArea(2, 4)] private string description = "用于剧情判断";

        public string FlagIdValue => flagId;
        public string DisplayName => displayName;
        public string Description => description;
        public bool IsValid => !string.IsNullOrWhiteSpace(flagId);
        public FlagId FlagId => new(flagId);
    }
}
