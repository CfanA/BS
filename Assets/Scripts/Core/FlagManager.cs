using System;
using System.Collections.Generic;
using BS.Foundation.Ids;
using BS.Gameplay.Story.Data;
using UnityEngine;

namespace BS.Core
{
    /// <summary>
    /// 全局剧情条件管理器。
    /// 负责保存布尔型剧情标记，供对话、交互、谜题、事件系统查询。
    /// </summary>
    public sealed class FlagManager : MonoBehaviour
    {
        private readonly Dictionary<FlagId, bool> _flags = new();

        public event Action<FlagId, bool> FlagChanged;

        public bool GetFlag(FlagId flagId)
        {
            if (!flagId.IsValid)
            {
                Debug.LogWarning("尝试读取无效的 FlagId。");
                return false;
            }

            return _flags.TryGetValue(flagId, out var value) && value;
        }

        public bool GetFlag(FlagDefinition flagDefinition)
        {
            return flagDefinition != null && GetFlag(flagDefinition.FlagId);
        }

        public void SetFlag(FlagId flagId, bool value = true)
        {
            if (!flagId.IsValid)
            {
                Debug.LogWarning("尝试设置无效的 FlagId。");
                return;
            }

            if (_flags.TryGetValue(flagId, out var currentValue) && currentValue == value)
            {
                return;
            }

            _flags[flagId] = value;
            FlagChanged?.Invoke(flagId, value);
        }

        public void SetFlag(FlagDefinition flagDefinition, bool value = true)
        {
            if (flagDefinition == null)
            {
                Debug.LogWarning("尝试设置空的 FlagDefinition。");
                return;
            }

            SetFlag(flagDefinition.FlagId, value);
        }

        public bool HasAll(params FlagId[] flagIds)
        {
            for (var i = 0; i < flagIds.Length; i++)
            {
                if (!GetFlag(flagIds[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasAny(params FlagId[] flagIds)
        {
            for (var i = 0; i < flagIds.Length; i++)
            {
                if (GetFlag(flagIds[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public void ClearAll()
        {
            _flags.Clear();
        }

        public IReadOnlyDictionary<FlagId, bool> ExportSnapshot()
        {
            return new Dictionary<FlagId, bool>(_flags);
        }

        public void ImportSnapshot(IReadOnlyDictionary<FlagId, bool> snapshot)
        {
            _flags.Clear();

            if (snapshot == null)
            {
                return;
            }

            foreach (var pair in snapshot)
            {
                _flags[pair.Key] = pair.Value;
                FlagChanged?.Invoke(pair.Key, pair.Value);
            }
        }
    }
}
