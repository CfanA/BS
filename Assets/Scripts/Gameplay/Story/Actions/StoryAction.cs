using System;
using BS.Core;
using BS.Foundation.Ids;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Items.Data;
using BS.Gameplay.SceneActors;
using UnityEngine;

namespace BS.Gameplay.Story.Actions
{
    /// <summary>
    /// 剧情动作。
    /// 用统一配置表达“条件满足后要做什么”。
    /// </summary>
    [Serializable]
    public sealed class StoryAction
    {
        private enum ActionType
        {
            SetFlag,
            SetGameObjectActive,
            PlayDialogue,
            GrantItem,
            OpenDoor,
            LoadScene,
            PlaySceneActorSequence
        }

        [SerializeField] private ActionType actionType;

        [Header("Set Flag")]
        [SerializeField] private FlagReference flag;
        [SerializeField] private bool flagValue = true;

        [Header("Set GameObject Active")]
        [SerializeField] private GameObject targetObject;
        [SerializeField] private bool activeState = true;

        [Header("Play Dialogue")]
        [SerializeField] private DialogueData dialogueData;

        [Header("Grant Item")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int itemCount = 1;

        [Header("Open Door")]
        [SerializeField] private DoorController doorController;

        [Header("Load Scene")]
        [SerializeField] private string sceneName;

        [Header("Play Scene Actor Sequence")]
        [SerializeField] private SceneActor sceneActor;
        [SerializeField] private SceneActorSequenceAsset sceneActorSequence;
        [SerializeField] private bool useActorDefaultSequence = true;

        public void Execute(GameManager gameManager)
        {
            if (gameManager == null)
            {
                return;
            }

            switch (actionType)
            {
                case ActionType.SetFlag:
                    if (flag.IsValid && gameManager.Flags != null)
                    {
                        gameManager.Flags.SetFlag(flag.FlagId, flagValue);
                    }
                    break;

                case ActionType.SetGameObjectActive:
                    if (targetObject != null)
                    {
                        targetObject.SetActive(activeState);
                    }
                    break;

                case ActionType.PlayDialogue:
                    if (dialogueData != null && gameManager.Dialogue != null && !gameManager.Dialogue.IsPlaying)
                    {
                        gameManager.Dialogue.StartDialogue(dialogueData);
                    }
                    break;

                case ActionType.GrantItem:
                    if (itemData != null && gameManager.Inventory != null)
                    {
                        gameManager.Inventory.TryAddItem(itemData, itemCount);
                    }
                    break;

                case ActionType.OpenDoor:
                    if (doorController != null)
                    {
                        doorController.Open();
                    }
                    break;

                case ActionType.LoadScene:
                    if (!string.IsNullOrWhiteSpace(sceneName) && gameManager.SceneLoader != null)
                    {
                        gameManager.SceneLoader.LoadScene(new SceneId(sceneName));
                    }
                    break;

                case ActionType.PlaySceneActorSequence:
                    if (sceneActor == null)
                    {
                        break;
                    }

                    if (sceneActorSequence != null)
                    {
                        sceneActor.PlaySequence(sceneActorSequence);
                    }
                    else if (useActorDefaultSequence)
                    {
                        sceneActor.PlayDefaultSequence();
                    }
                    break;
            }
        }
    }
}
