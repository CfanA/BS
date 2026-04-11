using BS.Core;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using BS.Gameplay.Story.Conditions;
using UnityEngine;

namespace BS.Gameplay.SceneFlow
{
    /// <summary>
    /// Scene door for inter-scene travel.
    /// Checks optional story conditions, then requests a scene load plus target spawn point.
    /// </summary>
    public sealed class SceneDoorInteractable : InteractableBase
    {
        [Header("Travel")]
        [SerializeField] private SceneReference targetScene;
        [SerializeField] private string targetSpawnPointId = "entry.default";

        [Header("Conditions")]
        [SerializeField] private bool requireConditionToOpen;
        [SerializeField] private FlagConditionGroup openConditions = new();

        [Header("Locked Feedback")]
        [SerializeField] private string lockedMessageText = "Door is locked.";
        [SerializeField] private DialogueData lockedDialogue;

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return base.CanInteract(interactor) && targetScene.IsValid;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null || GameManager.Instance.SceneLoader == null)
            {
                Debug.LogError("Missing SceneLoader.", this);
                return;
            }

            if (!targetScene.IsValid)
            {
                Debug.LogWarning("Target scene is not configured.", this);
                return;
            }

            if (requireConditionToOpen)
            {
                var flags = GameManager.Instance.Flags;
                if (flags == null || !openConditions.Evaluate(flags))
                {
                    HandleLocked(interactor);
                    return;
                }
            }

            GameManager.Instance.SceneLoader.LoadScene(targetScene.SceneId, targetSpawnPointId);
        }

        private void HandleLocked(PlayerInteractor interactor)
        {
            if (lockedDialogue != null && GameManager.Instance != null && GameManager.Instance.Dialogue != null)
            {
                GameManager.Instance.Dialogue.StartDialogue(lockedDialogue, interactor);
                return;
            }

            if (!string.IsNullOrWhiteSpace(lockedMessageText))
            {
                Debug.Log(lockedMessageText, this);
            }
        }
    }
}
