using System;
using BS.Foundation.Ids;
using BS.Gameplay.Story.Data;
using UnityEngine;

namespace BS.Gameplay.Story
{
    /// <summary>
    /// 轻量 Flag 引用。
    /// 优先引用 FlagDefinition，必要时可回退到直接 ID。
    /// </summary>
    [Serializable]
    public struct FlagReference
    {
        [SerializeField] private FlagDefinition definition;
        [SerializeField] private string fallbackFlagId;

        public FlagId FlagId
        {
            get
            {
                if (definition != null && definition.IsValid)
                {
                    return definition.FlagId;
                }

                return new FlagId(fallbackFlagId);
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

                return fallbackFlagId;
            }
        }

        public bool IsValid => FlagId.IsValid;
    }
}
