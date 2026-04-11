using BS.Foundation.Ids;
using BS.Gameplay.Clues;
using BS.Gameplay.Data;
using BS.Gameplay.Dialogue;
using BS.Gameplay.Items;
using BS.Gameplay.SceneFlow;
using BS.Gameplay.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BS.Core
{
    /// <summary>
    /// 游戏总入口。
    /// 负责跨场景常驻，并统一持有当前阶段最核心的全局系统引用。
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public FlagManager Flags { get; private set; }
        public SceneLoader SceneLoader { get; private set; }
        public InventoryManager Inventory { get; private set; }
        public ClueNotebookManager ClueNotebook { get; private set; }
        public DialogueManager Dialogue { get; private set; }
        public PlayerSpawnService PlayerSpawnService { get; private set; }
        public GameDataRegistry DataRegistry { get; private set; }
        public SaveManager SaveManager { get; private set; }

        [Header("启动配置")]
        [SerializeField] private bool loadStartupSceneOnPlay;
        [SerializeField] private string startupSceneName = "SampleScene";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Flags = GetComponent<FlagManager>();
            if (Flags == null)
            {
                Flags = gameObject.AddComponent<FlagManager>();
            }

            SceneLoader = GetComponent<SceneLoader>();
            if (SceneLoader == null)
            {
                SceneLoader = gameObject.AddComponent<SceneLoader>();
            }

            Inventory = GetComponent<InventoryManager>();
            if (Inventory == null)
            {
                Inventory = gameObject.AddComponent<InventoryManager>();
            }

            ClueNotebook = GetComponent<ClueNotebookManager>();
            if (ClueNotebook == null)
            {
                ClueNotebook = gameObject.AddComponent<ClueNotebookManager>();
            }

            Dialogue = GetComponent<DialogueManager>();
            if (Dialogue == null)
            {
                Dialogue = gameObject.AddComponent<DialogueManager>();
            }

            PlayerSpawnService = GetComponent<PlayerSpawnService>();
            if (PlayerSpawnService == null)
            {
                PlayerSpawnService = gameObject.AddComponent<PlayerSpawnService>();
            }

            DataRegistry = GetComponent<GameDataRegistry>();
            if (DataRegistry == null)
            {
                DataRegistry = gameObject.AddComponent<GameDataRegistry>();
            }

            SaveManager = GetComponent<SaveManager>();
            if (SaveManager == null)
            {
                SaveManager = gameObject.AddComponent<SaveManager>();
            }
        }

        private void Start()
        {
            if (!loadStartupSceneOnPlay)
            {
                return;
            }

            var startupScene = new SceneId(startupSceneName);
            if (!startupScene.IsValid)
            {
                return;
            }

            if (SceneManager.GetActiveScene().name == startupScene.Value)
            {
                return;
            }

            SceneLoader.LoadScene(startupScene);
        }
    }
}
