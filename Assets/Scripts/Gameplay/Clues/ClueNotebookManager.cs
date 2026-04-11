using System;
using System.Collections.Generic;
using BS.Gameplay.Clues.Data;
using UnityEngine;

namespace BS.Gameplay.Clues
{
    /// <summary>
    /// 线索簿管理器。
    /// 负责记录已发现线索，并为后续案件簿、推理面板和剧情查询提供统一入口。
    /// </summary>
    public sealed class ClueNotebookManager : MonoBehaviour
    {
        private readonly Dictionary<string, ClueData> _cluesById = new();
        private readonly List<ClueData> _clues = new();

        public event Action<ClueData> ClueAdded;
        public event Action<ClueData> ClueRemoved;
        public event Action NotebookChanged;

        public IReadOnlyList<ClueData> Clues => _clues;

        public bool TryAddClue(ClueData clueData)
        {
            if (clueData == null || !clueData.IsValid)
            {
                return false;
            }

            if (_cluesById.ContainsKey(clueData.ClueId))
            {
                return false;
            }

            _cluesById.Add(clueData.ClueId, clueData);
            _clues.Add(clueData);

            ClueAdded?.Invoke(clueData);
            NotebookChanged?.Invoke();
            return true;
        }

        public bool RemoveClue(ClueData clueData)
        {
            return clueData != null && RemoveClue(clueData.ClueId);
        }

        public bool RemoveClue(string clueId)
        {
            if (string.IsNullOrWhiteSpace(clueId))
            {
                return false;
            }

            if (!_cluesById.TryGetValue(clueId, out var clueData))
            {
                return false;
            }

            _cluesById.Remove(clueId);
            _clues.Remove(clueData);

            ClueRemoved?.Invoke(clueData);
            NotebookChanged?.Invoke();
            return true;
        }

        public bool HasClue(ClueData clueData)
        {
            return clueData != null && HasClue(clueData.ClueId);
        }

        public bool HasClue(string clueId)
        {
            return !string.IsNullOrWhiteSpace(clueId) && _cluesById.ContainsKey(clueId);
        }

        public ClueData GetClue(string clueId)
        {
            if (string.IsNullOrWhiteSpace(clueId))
            {
                return null;
            }

            return _cluesById.TryGetValue(clueId, out var clueData) ? clueData : null;
        }

        public void ClearAll()
        {
            _cluesById.Clear();
            _clues.Clear();
            NotebookChanged?.Invoke();
        }
    }
}
