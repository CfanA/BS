using System;
using System.Collections;
using BS.Foundation.Ids;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BS.Core
{
    /// <summary>
    /// 场景切换接口。
    /// 后续如果要加黑幕、淡入淡出、动画过场，可实现这个接口。
    /// </summary>
    public interface ISceneTransition
    {
        IEnumerator PlayEnter();
        IEnumerator PlayExit();
    }

    /// <summary>
    /// 默认空过场。
    /// MVP 阶段先保证接口存在，不强行绑定 UI。
    /// </summary>
    public sealed class NoSceneTransition : ISceneTransition
    {
        public IEnumerator PlayEnter()
        {
            yield break;
        }

        public IEnumerator PlayExit()
        {
            yield break;
        }
    }

    /// <summary>
    /// 统一场景加载器。
    /// 负责异步加载、重复请求保护，以及跨场景出生点上下文传递。
    /// </summary>
    public sealed class SceneLoader : MonoBehaviour
    {
        private static readonly ISceneTransition DefaultTransition = new NoSceneTransition();

        private SpawnPointId _pendingSpawnPointId;

        public bool IsLoading { get; private set; }

        public event Action<SceneId> BeforeSceneLoad;
        public event Action<SceneId> AfterSceneLoaded;

        public void LoadScene(SceneId sceneId)
        {
            LoadScene(sceneId, default, null);
        }

        public void LoadScene(SceneId sceneId, ISceneTransition transition)
        {
            LoadScene(sceneId, default, transition);
        }

        public void LoadScene(SceneId sceneId, string targetSpawnPointId)
        {
            LoadScene(sceneId, new SpawnPointId(targetSpawnPointId), null);
        }

        public void LoadScene(SceneId sceneId, string targetSpawnPointId, ISceneTransition transition)
        {
            LoadScene(sceneId, new SpawnPointId(targetSpawnPointId), transition);
        }

        public void LoadScene(SceneId sceneId, SpawnPointId targetSpawnPointId, ISceneTransition transition)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"Scene is already loading, ignore duplicate request: {sceneId}");
                return;
            }

            if (!sceneId.IsValid)
            {
                Debug.LogError("Trying to load an invalid SceneId.");
                return;
            }

            _pendingSpawnPointId = targetSpawnPointId;
            StartCoroutine(LoadSceneRoutine(sceneId, transition ?? DefaultTransition));
        }

        public bool TryConsumePendingSpawnPoint(out SpawnPointId spawnPointId)
        {
            spawnPointId = _pendingSpawnPointId;
            _pendingSpawnPointId = default;
            return spawnPointId.IsValid;
        }

        private IEnumerator LoadSceneRoutine(SceneId sceneId, ISceneTransition transition)
        {
            IsLoading = true;
            BeforeSceneLoad?.Invoke(sceneId);

            yield return transition.PlayExit();

            var asyncOperation = SceneManager.LoadSceneAsync(sceneId.Value);
            if (asyncOperation == null)
            {
                Debug.LogError($"Failed to load scene: {sceneId}");
                IsLoading = false;
                yield break;
            }

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            yield return transition.PlayEnter();

            IsLoading = false;
            AfterSceneLoaded?.Invoke(sceneId);
        }
    }
}
