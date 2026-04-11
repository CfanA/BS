using System;
using UnityEngine;

namespace BS.Gameplay.Puzzles
{
    /// <summary>
    /// 数字密码锁谜题。
    /// 只负责保存当前输入并判断密码是否正确。
    /// </summary>
    public sealed class NumberCodePuzzle : PuzzleBase
    {
        [Header("密码配置")]
        [SerializeField] private string correctCode = "0426";
        [SerializeField] private int maxLength = 4;
        [SerializeField] private bool clearInputOnWrongAnswer = true;

        private string _currentInput = string.Empty;

        public string CurrentInput => _currentInput;

        public event Action<string> InputChanged;
        public event Action<bool> SubmitFinished;

        public void AppendDigit(string digit)
        {
            if (!IsAvailable || IsSolved || string.IsNullOrEmpty(digit))
            {
                return;
            }

            if (_currentInput.Length >= maxLength)
            {
                return;
            }

            _currentInput += digit;
            InputChanged?.Invoke(_currentInput);
        }

        public void Backspace()
        {
            if (!IsAvailable || IsSolved || _currentInput.Length == 0)
            {
                return;
            }

            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
            InputChanged?.Invoke(_currentInput);
        }

        public void ClearInput()
        {
            if (_currentInput.Length == 0)
            {
                return;
            }

            _currentInput = string.Empty;
            InputChanged?.Invoke(_currentInput);
        }

        public bool Submit()
        {
            if (!IsAvailable || IsSolved)
            {
                return false;
            }

            var isCorrect = string.Equals(_currentInput, correctCode, StringComparison.Ordinal);
            SubmitFinished?.Invoke(isCorrect);

            if (isCorrect)
            {
                SolvePuzzle();
                return true;
            }

            if (clearInputOnWrongAnswer)
            {
                ClearInput();
            }

            return false;
        }
    }
}
