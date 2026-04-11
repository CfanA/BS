using System;
using System.Collections.Generic;

namespace BS.Gameplay.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public SceneSaveData scene = new();
        public List<ItemSaveEntry> items = new();
        public List<string> clueIds = new();
        public List<FlagSaveEntry> flags = new();
        public List<string> solvedPuzzleIds = new();
    }

    [Serializable]
    public sealed class SceneSaveData
    {
        public string sceneName;
        public string spawnPointId;
    }

    [Serializable]
    public sealed class ItemSaveEntry
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public sealed class FlagSaveEntry
    {
        public string flagId;
        public bool value;
    }
}
