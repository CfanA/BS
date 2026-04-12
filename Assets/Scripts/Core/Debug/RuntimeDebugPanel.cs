#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Reflection;
using BS.Core;
using BS.Foundation.Catalogs;
using BS.Foundation.Ids;
using BS.Gameplay.Puzzles;
using BS.Gameplay.SceneFlow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BS.Debugging
{
    /// <summary>
    /// 开发环境运行时调试面板。
    /// 第一版使用 IMGUI，避免引入额外 UI 资源和复杂接线。
    /// </summary>
    public sealed class RuntimeDebugPanel : MonoBehaviour
    {
        private enum DebugTab
        {
            Overview,
            Flags,
            Inventory,
            Clues,
            Scene,
            Save,
            Puzzle
        }

        private const float WindowWidth = 560f;
        private const float WindowHeight = 460f;
        private const float MinWindowWidth = 420f;
        private const float MinWindowHeight = 320f;
        private const float Margin = 12f;
        private const string DefaultFlagHint = "story.intro_finished";
        private const string DefaultSpawnHint = "entry.default";

        private static readonly DebugTab[] Tabs =
        {
            DebugTab.Overview,
            DebugTab.Flags,
            DebugTab.Inventory,
            DebugTab.Clues,
            DebugTab.Scene,
            DebugTab.Save,
            DebugTab.Puzzle
        };

        private Rect _windowRect = new(16f, 16f, WindowWidth, WindowHeight);
        private Vector2 _contentScroll;
        private Vector2 _flagsScroll;
        private Vector2 _inventoryScroll;
        private Vector2 _cluesScroll;
        private Vector2 _puzzleScroll;

        private bool _isVisible = true;
        private DebugTab _currentTab;

        private string _flagSearch = string.Empty;
        private string _manualFlagInput = string.Empty;

        private string _itemIdInput = string.Empty;
        private string _itemAmountInput = "1";

        private string _clueIdInput = string.Empty;

        private string _sceneNameInput = string.Empty;
        private string _spawnPointInput = string.Empty;

        private readonly List<FlagId> _knownFlagIds = new();
        private readonly HashSet<string> _knownFlagValues = new(StringComparer.Ordinal);

        private GUIStyle _sectionStyle;
        private GUIStyle _boxStyle;

        private void Awake()
        {
            hideFlags = HideFlags.DontSave;
            CacheKnownFlags();
            _manualFlagInput = DefaultFlagHint;
            _sceneNameInput = SceneManager.GetActiveScene().name;
            _spawnPointInput = DefaultSpawnHint;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _isVisible = !_isVisible;
                Debug.Log($"[DebugPanel] 面板{(_isVisible ? "已打开" : "已关闭")}", this);
            }
        }

        private void OnGUI()
        {
            if (!_isVisible)
            {
                return;
            }

            EnsureStyles();
            ClampWindowRectToScreen();
            _windowRect = GUI.Window(GetInstanceID(), _windowRect, DrawWindow, "Runtime Debug Panel");
        }

        private void DrawWindow(int windowId)
        {
            using (new GUILayout.VerticalScope())
            {
                DrawToolbar();
                GUILayout.Space(6f);
                _contentScroll = GUILayout.BeginScrollView(_contentScroll);
                DrawCurrentTab();
                GUILayout.EndScrollView();
                GUILayout.Space(6f);
                GUILayout.Label("F1: 打开/关闭调试面板");
            }

            GUI.DragWindow(new Rect(0f, 0f, _windowRect.width, 24f));
        }

        private void ClampWindowRectToScreen()
        {
            var maxWidth = Mathf.Max(MinWindowWidth, Screen.width - 16f);
            var maxHeight = Mathf.Max(MinWindowHeight, Screen.height - 16f);

            _windowRect.width = Mathf.Min(_windowRect.width, maxWidth);
            _windowRect.height = Mathf.Min(_windowRect.height, maxHeight);

            _windowRect.x = Mathf.Clamp(_windowRect.x, 0f, Mathf.Max(0f, Screen.width - _windowRect.width));
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0f, Mathf.Max(0f, Screen.height - _windowRect.height));
        }

        private void DrawToolbar()
        {
            using (new GUILayout.HorizontalScope())
            {
                for (var i = 0; i < Tabs.Length; i++)
                {
                    var tab = Tabs[i];
                    var selected = tab == _currentTab;
                    var label = selected ? $"[{tab}]" : tab.ToString();
                    if (GUILayout.Button(label, GUILayout.Height(28f)))
                    {
                        _currentTab = tab;
                    }
                }
            }
        }

        private void DrawCurrentTab()
        {
            switch (_currentTab)
            {
                case DebugTab.Overview:
                    DrawOverviewTab();
                    break;
                case DebugTab.Flags:
                    DrawFlagsTab();
                    break;
                case DebugTab.Inventory:
                    DrawInventoryTab();
                    break;
                case DebugTab.Clues:
                    DrawCluesTab();
                    break;
                case DebugTab.Scene:
                    DrawSceneTab();
                    break;
                case DebugTab.Save:
                    DrawSaveTab();
                    break;
                case DebugTab.Puzzle:
                    DrawPuzzleTab();
                    break;
            }
        }

        private void DrawOverviewTab()
        {
            var gameManager = GameManager.Instance;
            var player = FindFirstObjectByType<PlayerSpawnReceiver>(FindObjectsInactive.Exclude);
            var puzzles = FindObjectsByType<PuzzleBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var solvedCount = 0;
            for (var i = 0; i < puzzles.Length; i++)
            {
                if (puzzles[i] != null && puzzles[i].IsSolved)
                {
                    solvedCount++;
                }
            }

            DrawSection("运行概览", () =>
            {
                DrawKeyValue("当前场景", SceneManager.GetActiveScene().name);
                DrawKeyValue("GameManager", gameManager != null ? "已找到" : "未找到");
                DrawKeyValue("玩家对象", player != null ? player.name : "未找到");
                DrawKeyValue("玩家坐标", player != null ? player.transform.position.ToString("F2") : "-");
                DrawKeyValue("激活 Flag 数量", GetActiveFlagCount(gameManager).ToString());
                DrawKeyValue("当前道具条目数", GetInventoryEntryCount(gameManager).ToString());
                DrawKeyValue("当前线索数量", GetClueCount(gameManager).ToString());
                DrawKeyValue("当前已解谜题数量", solvedCount.ToString());
                DrawKeyValue("是否存在存档", GetHasSave(gameManager) ? "是" : "否");
            });
        }

        private void DrawFlagsTab()
        {
            var flagManager = GameManager.Instance != null ? GameManager.Instance.Flags : null;
            if (flagManager == null)
            {
                DrawMissingSystem("FlagManager 未找到。");
                return;
            }

            CacheKnownFlags();
            DrawSection("Flag 调试", () =>
            {
                GUILayout.Label("搜索关键词");
                _flagSearch = GUILayout.TextField(_flagSearch ?? string.Empty);

                GUILayout.Space(6f);
                GUILayout.Label("手动输入 FlagId");
                _manualFlagInput = GUILayout.TextField(_manualFlagInput ?? string.Empty);

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("设为 True", GUILayout.Height(28f)))
                    {
                        SetFlagFromInput(true);
                    }

                    if (GUILayout.Button("设为 False", GUILayout.Height(28f)))
                    {
                        SetFlagFromInput(false);
                    }

                    if (GUILayout.Button("查询当前值", GUILayout.Height(28f)))
                    {
                        QueryFlagFromInput();
                    }
                }
            });

            GUILayout.Space(8f);
            DrawSection("已知 Flag 列表", () =>
            {
                var snapshot = flagManager.ExportSnapshot();
                var flagIds = BuildVisibleFlagList(snapshot);
                _flagsScroll = GUILayout.BeginScrollView(_flagsScroll, GUILayout.Height(360f));

                if (flagIds.Count == 0)
                {
                    GUILayout.Label("没有匹配的 Flag。");
                }

                for (var i = 0; i < flagIds.Count; i++)
                {
                    var flagId = flagIds[i];
                    var current = flagManager.GetFlag(flagId);
                    using (new GUILayout.HorizontalScope(_boxStyle))
                    {
                        GUILayout.Label(flagId.Value, GUILayout.Width(360f));
                        GUILayout.Label(current ? "True" : "False", GUILayout.Width(70f));

                        if (GUILayout.Button("True", GUILayout.Width(70f)))
                        {
                            flagManager.SetFlag(flagId, true);
                            Debug.Log($"[DebugPanel] Flag 设为 True: {flagId.Value}", this);
                        }

                        if (GUILayout.Button("False", GUILayout.Width(70f)))
                        {
                            flagManager.SetFlag(flagId, false);
                            Debug.Log($"[DebugPanel] Flag 设为 False: {flagId.Value}", this);
                        }
                    }
                }

                GUILayout.EndScrollView();
            });
        }

        private void DrawInventoryTab()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Inventory == null || gameManager.DataRegistry == null)
            {
                DrawMissingSystem("InventoryManager 或 GameDataRegistry 未找到。");
                return;
            }

            DrawSection("道具操作", () =>
            {
                GUILayout.Label("itemId");
                _itemIdInput = GUILayout.TextField(_itemIdInput ?? string.Empty);
                GUILayout.Label("数量");
                _itemAmountInput = GUILayout.TextField(_itemAmountInput ?? "1");

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("添加道具", GUILayout.Height(28f)))
                    {
                        TryChangeItem(true);
                    }

                    if (GUILayout.Button("移除道具", GUILayout.Height(28f)))
                    {
                        TryChangeItem(false);
                    }
                }
            });

            GUILayout.Space(8f);
            DrawSection("当前背包", () =>
            {
                _inventoryScroll = GUILayout.BeginScrollView(_inventoryScroll, GUILayout.Height(360f));
                var entries = gameManager.Inventory.Entries;
                if (entries.Count == 0)
                {
                    GUILayout.Label("当前背包为空。");
                }

                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (entry == null || entry.ItemData == null)
                    {
                        continue;
                    }

                    using (new GUILayout.HorizontalScope(_boxStyle))
                    {
                        GUILayout.Label(entry.ItemData.ItemId, GUILayout.Width(260f));
                        GUILayout.Label(entry.ItemData.DisplayName, GUILayout.Width(220f));
                        GUILayout.Label($"x{entry.Count}", GUILayout.Width(70f));
                    }
                }

                GUILayout.EndScrollView();
            });
        }

        private void DrawCluesTab()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.ClueNotebook == null || gameManager.DataRegistry == null)
            {
                DrawMissingSystem("ClueNotebookManager 或 GameDataRegistry 未找到。");
                return;
            }

            DrawSection("线索操作", () =>
            {
                GUILayout.Label("clueId");
                _clueIdInput = GUILayout.TextField(_clueIdInput ?? string.Empty);

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("添加线索", GUILayout.Height(28f)))
                    {
                        TryChangeClue(true);
                    }

                    if (GUILayout.Button("移除线索", GUILayout.Height(28f)))
                    {
                        TryChangeClue(false);
                    }
                }
            });

            GUILayout.Space(8f);
            DrawSection("当前线索", () =>
            {
                _cluesScroll = GUILayout.BeginScrollView(_cluesScroll, GUILayout.Height(360f));
                var clues = gameManager.ClueNotebook.Clues;
                if (clues.Count == 0)
                {
                    GUILayout.Label("当前没有线索。");
                }

                for (var i = 0; i < clues.Count; i++)
                {
                    var clue = clues[i];
                    if (clue == null)
                    {
                        continue;
                    }

                    using (new GUILayout.HorizontalScope(_boxStyle))
                    {
                        GUILayout.Label(clue.ClueId, GUILayout.Width(260f));
                        GUILayout.Label(clue.DisplayName, GUILayout.Width(220f));
                        GUILayout.Label(clue.Category, GUILayout.Width(140f));
                    }
                }

                GUILayout.EndScrollView();
            });
        }

        private void DrawSceneTab()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.SceneLoader == null)
            {
                DrawMissingSystem("SceneLoader 未找到。");
                return;
            }

            DrawSection("场景操作", () =>
            {
                GUILayout.Label("sceneName");
                _sceneNameInput = GUILayout.TextField(string.IsNullOrWhiteSpace(_sceneNameInput) ? SceneManager.GetActiveScene().name : _sceneNameInput);
                GUILayout.Label("spawnPointId（可选）");
                _spawnPointInput = GUILayout.TextField(_spawnPointInput ?? string.Empty);

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("切换场景", GUILayout.Height(28f)))
                    {
                        LoadTargetScene();
                    }

                    if (GUILayout.Button("重载当前场景", GUILayout.Height(28f)))
                    {
                        ReloadCurrentScene();
                    }
                }
            });

            GUILayout.Space(8f);
            DrawSection("最近一次切场景参数", () =>
            {
                DrawKeyValue("当前场景", SceneManager.GetActiveScene().name);
                DrawKeyValue("最近请求 SceneId", gameManager.SceneLoader.LastRequestedSceneId.IsValid ? gameManager.SceneLoader.LastRequestedSceneId.Value : "-");
                DrawKeyValue("最近请求 SpawnPointId", gameManager.SceneLoader.LastRequestedSpawnPointId.IsValid ? gameManager.SceneLoader.LastRequestedSpawnPointId.Value : "-");
                DrawKeyValue("当前玩家出生点", GetCurrentSpawnPointId());
                DrawKeyValue("是否正在加载", gameManager.SceneLoader.IsLoading ? "是" : "否");
            });
        }

        private void DrawSaveTab()
        {
            var saveManager = GameManager.Instance != null ? GameManager.Instance.SaveManager : null;
            if (saveManager == null)
            {
                DrawMissingSystem("SaveManager 未找到。");
                return;
            }

            DrawSection("存档操作", () =>
            {
                DrawKeyValue("HasSaveFile", saveManager.HasSaveFile ? "True" : "False");

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("保存", GUILayout.Height(32f)))
                    {
                        var result = saveManager.SaveGame();
                        Debug.Log($"[DebugPanel] SaveGame 结果: {result}", this);
                    }

                    if (GUILayout.Button("读取", GUILayout.Height(32f)))
                    {
                        var result = saveManager.LoadGame();
                        Debug.Log($"[DebugPanel] LoadGame 结果: {result}", this);
                    }

                    if (GUILayout.Button("删档", GUILayout.Height(32f)))
                    {
                        saveManager.DeleteSave();
                        Debug.Log("[DebugPanel] 已调用 DeleteSave。", this);
                    }
                }
            });
        }

        private void DrawPuzzleTab()
        {
            DrawSection("当前场景谜题", () =>
            {
                var puzzles = FindObjectsByType<PuzzleBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                _puzzleScroll = GUILayout.BeginScrollView(_puzzleScroll, GUILayout.Height(470f));

                if (puzzles.Length == 0)
                {
                    GUILayout.Label("当前场景没有找到 PuzzleBase。");
                }

                for (var i = 0; i < puzzles.Length; i++)
                {
                    var puzzle = puzzles[i];
                    if (puzzle == null)
                    {
                        continue;
                    }

                    var hasSaveId = puzzle.TryGetSaveId(out var saveId);
                    using (new GUILayout.VerticalScope(_boxStyle))
                    {
                        DrawKeyValue("对象名", puzzle.name);
                        DrawKeyValue("状态", puzzle.State.ToString());
                        DrawKeyValue("IsSolved", puzzle.IsSolved ? "True" : "False");
                        DrawKeyValue("SaveId", hasSaveId ? saveId : "-");

                        using (new GUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("标记完成", GUILayout.Height(26f)))
                            {
                                puzzle.DebugMarkSolved();
                            }

                            if (GUILayout.Button("重置状态", GUILayout.Height(26f)))
                            {
                                puzzle.DebugResetToInactive();
                            }

                            if (GUILayout.Button("刷新可用性", GUILayout.Height(26f)))
                            {
                                puzzle.DebugRefreshAvailability();
                            }
                        }
                    }
                }

                GUILayout.EndScrollView();
            });
        }

        private void DrawSection(string title, Action drawContent)
        {
            GUILayout.Label(title, _sectionStyle);
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                drawContent?.Invoke();
            }
        }

        private void DrawMissingSystem(string message)
        {
            DrawSection("系统状态", () =>
            {
                GUILayout.Label(message);
            });
        }

        private void DrawKeyValue(string key, string value)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"{key}:", GUILayout.Width(180f));
                GUILayout.Label(value ?? "-");
            }
        }

        private void SetFlagFromInput(bool value)
        {
            var flagManager = GameManager.Instance != null ? GameManager.Instance.Flags : null;
            var flagId = new FlagId(_manualFlagInput);
            if (flagManager == null || !flagId.IsValid)
            {
                Debug.LogWarning("[DebugPanel] Flag 设置失败：FlagManager 不可用或 FlagId 无效。", this);
                return;
            }

            flagManager.SetFlag(flagId, value);
            RememberFlag(flagId);
            Debug.Log($"[DebugPanel] Flag 已设置: {flagId.Value} = {value}", this);
        }

        private void QueryFlagFromInput()
        {
            var flagManager = GameManager.Instance != null ? GameManager.Instance.Flags : null;
            var flagId = new FlagId(_manualFlagInput);
            if (flagManager == null || !flagId.IsValid)
            {
                Debug.LogWarning("[DebugPanel] Flag 查询失败：FlagManager 不可用或 FlagId 无效。", this);
                return;
            }

            RememberFlag(flagId);
            var value = flagManager.GetFlag(flagId);
            Debug.Log($"[DebugPanel] Flag 当前值: {flagId.Value} = {value}", this);
        }

        private void TryChangeItem(bool isAdd)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Inventory == null || gameManager.DataRegistry == null)
            {
                Debug.LogWarning("[DebugPanel] 道具操作失败：系统未就绪。", this);
                return;
            }

            if (!TryParsePositiveInt(_itemAmountInput, out var amount))
            {
                Debug.LogWarning("[DebugPanel] 道具数量必须是大于 0 的整数。", this);
                return;
            }

            var itemData = gameManager.DataRegistry.GetItem(_itemIdInput);
            if (itemData == null)
            {
                Debug.LogWarning($"[DebugPanel] 未找到 ItemData: {_itemIdInput}", this);
                return;
            }

            var result = isAdd
                ? gameManager.Inventory.TryAddItem(itemData, amount)
                : gameManager.Inventory.RemoveItem(itemData, amount);

            Debug.Log($"[DebugPanel] {(isAdd ? "添加" : "移除")}道具结果: {itemData.ItemId}, amount={amount}, success={result}", this);
        }

        private void TryChangeClue(bool isAdd)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.ClueNotebook == null || gameManager.DataRegistry == null)
            {
                Debug.LogWarning("[DebugPanel] 线索操作失败：系统未就绪。", this);
                return;
            }

            var clueData = gameManager.DataRegistry.GetClue(_clueIdInput);
            if (clueData == null)
            {
                Debug.LogWarning($"[DebugPanel] 未找到 ClueData: {_clueIdInput}", this);
                return;
            }

            var result = isAdd
                ? gameManager.ClueNotebook.TryAddClue(clueData)
                : gameManager.ClueNotebook.RemoveClue(clueData.ClueId);

            Debug.Log($"[DebugPanel] {(isAdd ? "添加" : "移除")}线索结果: {clueData.ClueId}, success={result}", this);
        }

        private void LoadTargetScene()
        {
            var sceneLoader = GameManager.Instance != null ? GameManager.Instance.SceneLoader : null;
            var sceneId = new SceneId(_sceneNameInput);
            if (sceneLoader == null || !sceneId.IsValid)
            {
                Debug.LogWarning("[DebugPanel] 切场景失败：SceneLoader 不可用或 SceneId 无效。", this);
                return;
            }

            if (string.IsNullOrWhiteSpace(_spawnPointInput))
            {
                sceneLoader.LoadScene(sceneId);
                Debug.Log($"[DebugPanel] 已请求切换场景: {sceneId.Value}", this);
                return;
            }

            sceneLoader.LoadScene(sceneId, _spawnPointInput);
            Debug.Log($"[DebugPanel] 已请求切换场景: {sceneId.Value}, spawn={_spawnPointInput}", this);
        }

        private void ReloadCurrentScene()
        {
            _sceneNameInput = SceneManager.GetActiveScene().name;
            LoadTargetScene();
        }

        private void CacheKnownFlags()
        {
            var flagManager = GameManager.Instance != null ? GameManager.Instance.Flags : null;
            if (flagManager != null)
            {
                var snapshot = flagManager.ExportSnapshot();
                foreach (var pair in snapshot)
                {
                    RememberFlag(pair.Key);
                }
            }

            var fields = typeof(GameFlags).GetFields(BindingFlags.Public | BindingFlags.Static);
            for (var i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType != typeof(FlagId))
                {
                    continue;
                }

                var value = (FlagId)fields[i].GetValue(null);
                RememberFlag(value);
            }
        }

        private List<FlagId> BuildVisibleFlagList(IReadOnlyDictionary<FlagId, bool> snapshot)
        {
            var result = new List<FlagId>();
            for (var i = 0; i < _knownFlagIds.Count; i++)
            {
                var flagId = _knownFlagIds[i];
                if (!IsMatch(flagId.Value, _flagSearch))
                {
                    continue;
                }

                result.Add(flagId);
            }

            foreach (var pair in snapshot)
            {
                if (_knownFlagValues.Contains(pair.Key.Value) || !IsMatch(pair.Key.Value, _flagSearch))
                {
                    continue;
                }

                result.Add(pair.Key);
            }

            result.Sort((left, right) => string.Compare(left.Value, right.Value, StringComparison.Ordinal));
            return result;
        }

        private void RememberFlag(FlagId flagId)
        {
            if (!flagId.IsValid || !_knownFlagValues.Add(flagId.Value))
            {
                return;
            }

            _knownFlagIds.Add(flagId);
        }

        private static bool IsMatch(string value, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(value)
                   && value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool TryParsePositiveInt(string input, out int value)
        {
            return int.TryParse(input, out value) && value > 0;
        }

        private static int GetActiveFlagCount(GameManager gameManager)
        {
            if (gameManager == null || gameManager.Flags == null)
            {
                return 0;
            }

            var snapshot = gameManager.Flags.ExportSnapshot();
            var count = 0;
            foreach (var pair in snapshot)
            {
                if (pair.Value)
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetInventoryEntryCount(GameManager gameManager)
        {
            return gameManager != null && gameManager.Inventory != null ? gameManager.Inventory.Entries.Count : 0;
        }

        private static int GetClueCount(GameManager gameManager)
        {
            return gameManager != null && gameManager.ClueNotebook != null ? gameManager.ClueNotebook.Clues.Count : 0;
        }

        private static bool GetHasSave(GameManager gameManager)
        {
            return gameManager != null && gameManager.SaveManager != null && gameManager.SaveManager.HasSaveFile;
        }

        private static string GetCurrentSpawnPointId()
        {
            var player = FindFirstObjectByType<PlayerSpawnReceiver>(FindObjectsInactive.Exclude);
            return player != null && player.CurrentSpawnPointId.IsValid ? player.CurrentSpawnPointId.Value : "-";
        }

        private void EnsureStyles()
        {
            if (_sectionStyle != null)
            {
                return;
            }

            _sectionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset((int)Margin, (int)Margin, 8, 8),
                margin = new RectOffset(0, 0, 4, 4)
            };
        }
    }
}
#endif
