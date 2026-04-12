#if UNITY_EDITOR || DEVELOPMENT_BUILD
using BS.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BS.Debugging
{
    /// <summary>
    /// 开发环境下注入运行时调试面板。
    /// 优先挂到 GameManager，同步跟随全局入口生命周期。
    /// </summary>
    public static class RuntimeDebugPanelBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsurePanel()
        {
            TryAttachToGameManager();
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttachToGameManager();
        }

        private static void TryAttachToGameManager()
        {
            var gameManager = GameManager.Instance ?? Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
            if (gameManager == null)
            {
                return;
            }

            if (gameManager.GetComponent<RuntimeDebugPanel>() == null)
            {
                gameManager.gameObject.AddComponent<RuntimeDebugPanel>();
                Debug.Log("[DebugPanel] 已挂载到 GameManager。", gameManager);
            }
        }
    }
}
#endif
