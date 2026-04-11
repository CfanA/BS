using System;
using BS.Core;
using UnityEngine;

namespace BS.Gameplay.Story.Conditions
{
    /// <summary>
    /// 单个 Flag 条件。
    /// 例如：某 Flag 必须为 true，或必须为 false。
    /// </summary>
    [Serializable]
    public struct FlagRequirement
    {
        [SerializeField] private FlagReference flag;
        [SerializeField] private bool expectedValue;

        public bool Evaluate(FlagManager flagManager)
        {
            return flagManager != null && flag.IsValid && flagManager.GetFlag(flag.FlagId) == expectedValue;
        }
    }
}
