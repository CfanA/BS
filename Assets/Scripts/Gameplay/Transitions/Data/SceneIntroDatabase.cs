using System.Collections.Generic;
using UnityEngine;

namespace BS.Gameplay.Transitions.Data
{
    /// <summary>
    /// 场景开场演出配置表。
    /// 按场景名查找对应的开场参数。
    /// </summary>
    [CreateAssetMenu(menuName = "BS/Transitions/Scene Intro Database", fileName = "SceneIntroDatabase_")]
    public sealed class SceneIntroDatabase : ScriptableObject
    {
        [SerializeField] private List<SceneIntroProfile> profiles = new();

        public bool TryGetProfile(string sceneName, out SceneIntroProfile profile)
        {
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                var normalizedSceneName = sceneName.Trim();
                for (var i = 0; i < profiles.Count; i++)
                {
                    var candidate = profiles[i];
                    if (candidate != null
                        && !string.IsNullOrWhiteSpace(candidate.SceneName)
                        && string.Equals(candidate.SceneName.Trim(), normalizedSceneName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        profile = candidate;
                        return true;
                    }
                }
            }

            Debug.LogWarning($"[SceneIntroDatabase] 未找到场景开场配置: {sceneName}", this);
            profile = null;
            return false;
        }
    }
}
