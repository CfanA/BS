using System;
using BS.Core;
using UnityEngine;

namespace BS.Gameplay.Story.Conditions
{
    /// <summary>
    /// 复合 Flag 条件组。
    /// allOf 全部满足，anyOf 任一满足，noneOf 全部不满足。
    /// </summary>
    [Serializable]
    public sealed class FlagConditionGroup
    {
        [SerializeField] private FlagRequirement[] allOf;
        [SerializeField] private FlagRequirement[] anyOf;
        [SerializeField] private FlagRequirement[] noneOf;

        public bool Evaluate(FlagManager flagManager)
        {
            if (flagManager == null)
            {
                return false;
            }

            if (!EvaluateAll(flagManager, allOf))
            {
                return false;
            }

            if (!EvaluateAny(flagManager, anyOf))
            {
                return false;
            }

            return EvaluateNone(flagManager, noneOf);
        }

        private static bool EvaluateAll(FlagManager flagManager, FlagRequirement[] requirements)
        {
            if (requirements == null || requirements.Length == 0)
            {
                return true;
            }

            for (var i = 0; i < requirements.Length; i++)
            {
                if (!requirements[i].Evaluate(flagManager))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool EvaluateAny(FlagManager flagManager, FlagRequirement[] requirements)
        {
            if (requirements == null || requirements.Length == 0)
            {
                return true;
            }

            for (var i = 0; i < requirements.Length; i++)
            {
                if (requirements[i].Evaluate(flagManager))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EvaluateNone(FlagManager flagManager, FlagRequirement[] requirements)
        {
            if (requirements == null || requirements.Length == 0)
            {
                return true;
            }

            for (var i = 0; i < requirements.Length; i++)
            {
                if (requirements[i].Evaluate(flagManager))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
