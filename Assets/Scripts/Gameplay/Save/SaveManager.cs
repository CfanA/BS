using System.Collections;
using System.Collections.Generic;
using System.IO;
using BS.Core;
using BS.Foundation.Ids;
using BS.Gameplay.Data;
using BS.Gameplay.Puzzles;
using BS.Gameplay.SceneFlow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BS.Gameplay.Save
{
    /// <summary>
    /// 最小单槽位存档管理器。
    /// 负责采集各系统快照、写入 JSON，并在读取后分发给各运行时系统恢复。
    /// </summary>
    public sealed class SaveManager : MonoBehaviour
    {
        private const string SaveFileName = "save_slot_0.json";

        private SaveData _pendingPuzzleRestoreData;
        private Coroutine _restoreRoutine;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public bool HasSaveFile => File.Exists(SaveFilePath);

        private void OnEnable()
        {
            if (GameManager.Instance != null && GameManager.Instance.SceneLoader != null)
            {
                GameManager.Instance.SceneLoader.AfterSceneLoaded += HandleAfterSceneLoaded;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null && GameManager.Instance.SceneLoader != null)
            {
                GameManager.Instance.SceneLoader.AfterSceneLoaded -= HandleAfterSceneLoaded;
            }
        }

        public bool SaveGame()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return false;
            }

            var saveData = BuildSaveData(gameManager);
            var json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"Game saved: {SaveFilePath}", this);
            return true;
        }

        public bool LoadGame()
        {
            if (!HasSaveFile)
            {
                Debug.LogWarning("No save file found.", this);
                return false;
            }

            var json = File.ReadAllText(SaveFilePath);
            var saveData = JsonUtility.FromJson<SaveData>(json);
            if (saveData == null)
            {
                Debug.LogError("Failed to parse save file.", this);
                return false;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return false;
            }

            ApplyGlobalData(saveData, gameManager);
            _pendingPuzzleRestoreData = saveData;

            var currentSceneName = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrWhiteSpace(saveData.scene.sceneName) && saveData.scene.sceneName != currentSceneName)
            {
                gameManager.SceneLoader.LoadScene(new SceneId(saveData.scene.sceneName), saveData.scene.spawnPointId);
                return true;
            }

            if (_restoreRoutine != null)
            {
                StopCoroutine(_restoreRoutine);
            }

            _restoreRoutine = StartCoroutine(RestoreSceneStateNextFrame());
            return true;
        }

        public void DeleteSave()
        {
            if (HasSaveFile)
            {
                File.Delete(SaveFilePath);
            }
        }

        private SaveData BuildSaveData(GameManager gameManager)
        {
            var saveData = new SaveData();

            saveData.scene.sceneName = SceneManager.GetActiveScene().name;
            saveData.scene.spawnPointId = GetCurrentSpawnPointId();

            if (gameManager.Inventory != null)
            {
                for (var i = 0; i < gameManager.Inventory.Entries.Count; i++)
                {
                    var entry = gameManager.Inventory.Entries[i];
                    if (entry?.ItemData == null || !entry.ItemData.IsValid)
                    {
                        continue;
                    }

                    saveData.items.Add(new ItemSaveEntry
                    {
                        itemId = entry.ItemData.ItemId,
                        count = entry.Count
                    });
                }
            }

            if (gameManager.ClueNotebook != null)
            {
                for (var i = 0; i < gameManager.ClueNotebook.Clues.Count; i++)
                {
                    var clue = gameManager.ClueNotebook.Clues[i];
                    if (clue != null && clue.IsValid)
                    {
                        saveData.clueIds.Add(clue.ClueId);
                    }
                }
            }

            if (gameManager.Flags != null)
            {
                var snapshot = gameManager.Flags.ExportSnapshot();
                foreach (var pair in snapshot)
                {
                    saveData.flags.Add(new FlagSaveEntry
                    {
                        flagId = pair.Key.Value,
                        value = pair.Value
                    });
                }
            }

            var puzzles = FindObjectsByType<PuzzleBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < puzzles.Length; i++)
            {
                if (puzzles[i].IsSolved && puzzles[i].TryGetSaveId(out var puzzleId))
                {
                    saveData.solvedPuzzleIds.Add(puzzleId);
                }
            }

            return saveData;
        }

        private void ApplyGlobalData(SaveData saveData, GameManager gameManager)
        {
            RestoreFlags(saveData, gameManager);
            RestoreInventory(saveData, gameManager);
            RestoreClues(saveData, gameManager);
        }

        private void RestoreFlags(SaveData saveData, GameManager gameManager)
        {
            if (gameManager.Flags == null)
            {
                return;
            }

            var snapshot = new Dictionary<FlagId, bool>();
            for (var i = 0; i < saveData.flags.Count; i++)
            {
                var entry = saveData.flags[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.flagId))
                {
                    continue;
                }

                snapshot[new FlagId(entry.flagId)] = entry.value;
            }

            gameManager.Flags.ImportSnapshot(snapshot);
        }

        private void RestoreInventory(SaveData saveData, GameManager gameManager)
        {
            if (gameManager.Inventory == null || gameManager.DataRegistry == null)
            {
                return;
            }

            gameManager.Inventory.ClearAll();

            for (var i = 0; i < saveData.items.Count; i++)
            {
                var entry = saveData.items[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.itemId) || entry.count <= 0)
                {
                    continue;
                }

                var itemData = gameManager.DataRegistry.GetItem(entry.itemId);
                if (itemData == null)
                {
                    Debug.LogWarning($"Missing ItemData for save restore: {entry.itemId}", this);
                    continue;
                }

                gameManager.Inventory.TryAddItem(itemData, entry.count);
            }
        }

        private void RestoreClues(SaveData saveData, GameManager gameManager)
        {
            if (gameManager.ClueNotebook == null || gameManager.DataRegistry == null)
            {
                return;
            }

            gameManager.ClueNotebook.ClearAll();

            for (var i = 0; i < saveData.clueIds.Count; i++)
            {
                var clueId = saveData.clueIds[i];
                if (string.IsNullOrWhiteSpace(clueId))
                {
                    continue;
                }

                var clueData = gameManager.DataRegistry.GetClue(clueId);
                if (clueData == null)
                {
                    Debug.LogWarning($"Missing ClueData for save restore: {clueId}", this);
                    continue;
                }

                gameManager.ClueNotebook.TryAddClue(clueData);
            }
        }

        private void HandleAfterSceneLoaded(SceneId sceneId)
        {
            if (_pendingPuzzleRestoreData == null)
            {
                return;
            }

            if (_restoreRoutine != null)
            {
                StopCoroutine(_restoreRoutine);
            }

            _restoreRoutine = StartCoroutine(RestoreSceneStateNextFrame());
        }

        private IEnumerator RestoreSceneStateNextFrame()
        {
            yield return null;

            if (_pendingPuzzleRestoreData == null)
            {
                yield break;
            }

            RestorePlayerSpawnInCurrentScene(_pendingPuzzleRestoreData.scene.spawnPointId);
            RestoreSolvedPuzzles(_pendingPuzzleRestoreData);
            _pendingPuzzleRestoreData = null;
        }

        private void RestoreSolvedPuzzles(SaveData saveData)
        {
            var solvedIds = new HashSet<string>(saveData.solvedPuzzleIds);
            var puzzles = FindObjectsByType<PuzzleBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < puzzles.Length; i++)
            {
                if (puzzles[i].TryGetSaveId(out var puzzleId) && solvedIds.Contains(puzzleId))
                {
                    puzzles[i].RestoreSolvedStateFromSave();
                }
            }
        }

        private static string GetCurrentSpawnPointId()
        {
            var receiver = FindFirstObjectByType<PlayerSpawnReceiver>(FindObjectsInactive.Exclude);
            return receiver != null && receiver.CurrentSpawnPointId.IsValid ? receiver.CurrentSpawnPointId.Value : string.Empty;
        }

        private static void RestorePlayerSpawnInCurrentScene(string spawnPointIdValue)
        {
            if (string.IsNullOrWhiteSpace(spawnPointIdValue))
            {
                return;
            }

            var receiver = FindFirstObjectByType<PlayerSpawnReceiver>(FindObjectsInactive.Exclude);
            if (receiver == null)
            {
                return;
            }

            var targetId = new SpawnPointId(spawnPointIdValue);
            var spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].SpawnPointId.Equals(targetId))
                {
                    receiver.ApplySpawn(spawnPoints[i]);
                    return;
                }
            }
        }
    }
}
