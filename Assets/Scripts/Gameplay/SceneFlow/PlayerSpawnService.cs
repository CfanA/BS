using System.Collections;
using BS.Core;
using BS.Foundation.Ids;
using UnityEngine;

namespace BS.Gameplay.SceneFlow
{
    /// <summary>
    /// Resolves the target spawn point after a scene load and moves the player there.
    /// </summary>
    public sealed class PlayerSpawnService : MonoBehaviour
    {
        private Coroutine _spawnRoutine;

        private void Start()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
            }

            _spawnRoutine = StartCoroutine(ResolveInitialSpawnRoutine());
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null && GameManager.Instance.SceneLoader != null)
            {
                GameManager.Instance.SceneLoader.SceneLoaded += HandleSceneLoaded;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null && GameManager.Instance.SceneLoader != null)
            {
                GameManager.Instance.SceneLoader.SceneLoaded -= HandleSceneLoaded;
            }
        }

        private void HandleSceneLoaded(SceneId sceneId)
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
            }

            _spawnRoutine = StartCoroutine(ResolveSpawnRoutine(false));
        }

        private IEnumerator ResolveSpawnRoutine(bool waitOneFrame)
        {
            if (waitOneFrame)
            {
                yield return null;
            }

            if (GameManager.Instance == null || GameManager.Instance.SceneLoader == null)
            {
                yield break;
            }

            if (!GameManager.Instance.SceneLoader.TryConsumePendingSpawnPoint(out var spawnPointId))
            {
                yield break;
            }

            var spawnPoint = FindSpawnPoint(spawnPointId) ?? FindDefaultSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("No spawn point found in the loaded scene.", this);
                yield break;
            }

            var player = FindFirstObjectByType<PlayerSpawnReceiver>(FindObjectsInactive.Exclude);
            if (player == null)
            {
                Debug.LogWarning("No PlayerSpawnReceiver found in the loaded scene.", this);
                yield break;
            }

            player.ApplySpawn(spawnPoint);
        }

        private IEnumerator ResolveInitialSpawnRoutine()
        {
            yield return null;

            var player = FindFirstObjectByType<PlayerSpawnReceiver>(FindObjectsInactive.Exclude);
            if (player == null || player.CurrentSpawnPointId.IsValid)
            {
                yield break;
            }

            var spawnPoint = FindDefaultSpawnPoint();
            if (spawnPoint == null)
            {
                yield break;
            }

            player.ApplySpawn(spawnPoint);
        }

        private static SceneSpawnPoint FindSpawnPoint(SpawnPointId spawnPointId)
        {
            if (!spawnPointId.IsValid)
            {
                return null;
            }

            var spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].SpawnPointId.Equals(spawnPointId))
                {
                    return spawnPoints[i];
                }
            }

            return null;
        }

        private static SceneSpawnPoint FindDefaultSpawnPoint()
        {
            var spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].IsDefaultSpawn)
                {
                    return spawnPoints[i];
                }
            }

            return spawnPoints.Length > 0 ? spawnPoints[0] : null;
        }
    }
}
