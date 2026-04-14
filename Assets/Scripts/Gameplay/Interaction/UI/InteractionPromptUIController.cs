using BS.Gameplay.Interaction;
using BS.Core;
using TMPro;
using UnityEngine;

namespace BS.Gameplay.Interaction.UI
{
    /// <summary>
    /// 基础交互提示 UI 控制器。
    /// 只订阅 PlayerInteractor 的提示事件，不直接依赖具体交互对象实现。
    /// </summary>
    public sealed class InteractionPromptUIController : MonoBehaviour
    {
        [Header("依赖引用")]
        [SerializeField] private PlayerInteractor playerInteractor;
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptText;

        [Header("显示配置")]
        [SerializeField] private string promptFormat = "[E] {0} {1}";

        private void OnEnable()
        {
            if (playerInteractor == null)
            {
                return;
            }

            playerInteractor.PromptShown += ShowPrompt;
            playerInteractor.PromptHidden += HidePrompt;
        }

        private void OnDisable()
        {
            if (playerInteractor == null)
            {
                return;
            }

            playerInteractor.PromptShown -= ShowPrompt;
            playerInteractor.PromptHidden -= HidePrompt;
        }

        private void Start()
        {
            HidePrompt();
        }

        private void Update()
        {
            if (IsPromptBlocked())
            {
                HidePrompt();
            }
        }

        private void ShowPrompt(string actionText, string targetName)
        {
            if (IsPromptBlocked())
            {
                HidePrompt();
                return;
            }

            if (promptRoot != null)
            {
                promptRoot.SetActive(true);
            }

            if (promptText != null)
            {
                promptText.text = string.Format(promptFormat, actionText, targetName);
            }
        }

        private void HidePrompt()
        {
            if (promptRoot != null)
            {
                promptRoot.SetActive(false);
            }
        }

        private static bool IsPromptBlocked()
        {
            if (GameManager.Instance == null)
            {
                return false;
            }

            var isSceneLoading = GameManager.Instance.SceneLoader != null
                                 && GameManager.Instance.SceneLoader.IsLoading;

            var isDialoguePlaying = GameManager.Instance.Dialogue != null
                                    && GameManager.Instance.Dialogue.IsPlaying;

            return isSceneLoading || isDialoguePlaying;
        }
    }
}
