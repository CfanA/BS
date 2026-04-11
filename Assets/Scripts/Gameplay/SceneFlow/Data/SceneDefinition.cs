using BS.Foundation.Ids;
using UnityEngine;

namespace BS.Gameplay.SceneFlow.Data
{
    /// <summary>
    /// 场景定义资源。
    /// 通过资源持有稳定场景名，减少门和剧情逻辑里散落的字符串。
    /// </summary>
    [CreateAssetMenu(menuName = "BS/Scene Flow/Scene Definition", fileName = "Scene_")]
    public sealed class SceneDefinition : ScriptableObject
    {
        [Header("场景信息")]
        [SerializeField] private string sceneName = "SampleScene";
        [SerializeField] private string displayName = "样例场景";

        public string SceneName => sceneName;
        public string DisplayName => displayName;
        public bool IsValid => !string.IsNullOrWhiteSpace(sceneName);
        public SceneId SceneId => new(sceneName);
    }
}
