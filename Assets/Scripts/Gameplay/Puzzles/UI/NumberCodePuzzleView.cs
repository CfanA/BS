using TMPro;
using UnityEngine;

namespace BS.Gameplay.Puzzles.UI
{
    /// <summary>
    /// 数字密码锁 UI。
    /// 只负责显示和按钮转发，不参与判题逻辑。
    /// </summary>
    public sealed class NumberCodePuzzleView : PuzzleViewBase
    {
        [Header("UI")]
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text inputText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private string maskedCharacter = "*";
        [SerializeField] private string wrongAnswerMessage = "密码错误";
        [SerializeField] private string solvedMessage = "已解锁";

        private NumberCodePuzzle _numberPuzzle;

        private void Start()
        {
            if (!IsOpen)
            {
                SetVisible(false);
            }
        }

        public override void Open(PuzzleBase puzzle, Gameplay.Interaction.PlayerInteractor interactor)
        {
            base.Open(puzzle, interactor);

            _numberPuzzle = puzzle as NumberCodePuzzle;
            if (_numberPuzzle == null)
            {
                Debug.LogError("NumberCodePuzzleView 需要绑定 NumberCodePuzzle。", this);
                Close();
                return;
            }

            _numberPuzzle.InputChanged += HandleInputChanged;
            _numberPuzzle.SubmitFinished += HandleSubmitFinished;
            _numberPuzzle.Solved += HandleSolved;

            SetVisible(true);
            RefreshInput(_numberPuzzle.CurrentInput);
            SetStatus(string.Empty);
        }

        public override void Close()
        {
            if (_numberPuzzle != null)
            {
                _numberPuzzle.InputChanged -= HandleInputChanged;
                _numberPuzzle.SubmitFinished -= HandleSubmitFinished;
                _numberPuzzle.Solved -= HandleSolved;
                _numberPuzzle = null;
            }

            SetVisible(false);
            base.Close();
        }

        public void InputDigit(string digit)
        {
            _numberPuzzle?.AppendDigit(digit);
        }

        public void Backspace()
        {
            _numberPuzzle?.Backspace();
        }

        public void ClearInput()
        {
            _numberPuzzle?.ClearInput();
        }

        public void Submit()
        {
            _numberPuzzle?.Submit();
        }

        public void CloseView()
        {
            Close();
        }

        private void HandleInputChanged(string input)
        {
            RefreshInput(input);
        }

        private void HandleSubmitFinished(bool success)
        {
            if (!success)
            {
                SetStatus(wrongAnswerMessage);
            }
        }

        private void HandleSolved()
        {
            SetStatus(solvedMessage);
        }

        private void RefreshInput(string input)
        {
            if (inputText == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(maskedCharacter))
            {
                inputText.text = input;
                return;
            }

            inputText.text = new string(maskedCharacter[0], input.Length);
        }

        private void SetStatus(string value)
        {
            if (statusText != null)
            {
                statusText.text = value;
            }
        }

        private void SetVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
