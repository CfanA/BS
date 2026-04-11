using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using UnityEngine;

namespace BS.Gameplay.Puzzles
{
    /// <summary>
    /// 谜题统一交互入口。
    /// 负责激活谜题并打开对应视图，不把具体谜题逻辑写在交互层。
    /// </summary>
    public sealed class PuzzleInteractable : InteractableBase
    {
        [Header("谜题配置")]
        [SerializeField] private PuzzleBase puzzle;
        [SerializeField] private PuzzleViewBase puzzleView;
        [SerializeField] private DialogueData unavailableDialogue;
        [SerializeField] private string unavailableMessage = "现在还不能解这个谜题。";

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return base.CanInteract(interactor) && puzzle != null && !puzzle.IsSolved;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (puzzle == null)
            {
                Debug.LogWarning("PuzzleInteractable 缺少 PuzzleBase。", this);
                return;
            }

            if (!puzzle.TryActivate())
            {
                if (unavailableDialogue != null && Core.GameManager.Instance != null && Core.GameManager.Instance.Dialogue != null)
                {
                    Core.GameManager.Instance.Dialogue.StartDialogue(unavailableDialogue, interactor);
                }
                else if (!string.IsNullOrWhiteSpace(unavailableMessage))
                {
                    Debug.Log(unavailableMessage, this);
                }

                return;
            }

            if (puzzleView == null)
            {
                Debug.LogWarning("PuzzleInteractable 缺少 PuzzleViewBase。", this);
                return;
            }

            puzzleView.Open(puzzle, interactor);
        }
    }
}
