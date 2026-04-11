using System;
using BS.Foundation.Ids;
using BS.Gameplay.SceneFlow.Data;
using UnityEngine;

namespace BS.Gameplay.SceneFlow
{
    /// <summary>
    /// 场景引用。
    /// 优先拖 SceneDefinition，保留后备场景名兼容快速原型。
    /// </summary>
    [Serializable]
    public struct SceneReference
    {
        [SerializeField] private SceneDefinition definition;
        [SerializeField] private string fallbackSceneName;

        public SceneId SceneId
        {
            get
            {
                if (definition != null && definition.IsValid)
                {
                    return definition.SceneId;
                }

                return new SceneId(fallbackSceneName);
            }
        }

        public string DisplayName
        {
            get
            {
                if (definition != null)
                {
                    return definition.DisplayName;
                }

                return fallbackSceneName;
            }
        }

        public bool IsValid => SceneId.IsValid;
    }
}
