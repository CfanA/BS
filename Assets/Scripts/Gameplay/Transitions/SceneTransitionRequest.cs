using System;
using BS.Foundation.Ids;

namespace BS.Gameplay.Transitions
{
    /// <summary>
    /// 一次过场请求。
    /// 运行时可附带场景目标和钩子回调。
    /// </summary>
    public sealed class SceneTransitionRequest
    {
        public SceneTransitionRequest()
        {
        }

        public SceneTransitionRequest(string sceneName, string spawnPointId = null)
        {
            TargetSceneId = new SceneId(sceneName);
            TargetSpawnPointId = string.IsNullOrWhiteSpace(spawnPointId)
                ? default
                : new SpawnPointId(spawnPointId);
        }

        public SceneId TargetSceneId { get; set; }
        public SpawnPointId TargetSpawnPointId { get; set; }
        public bool UnlockPlayerOnComplete { get; set; } = true;
        public Action BeforeSceneLoad { get; set; }
        public Action AfterSceneLoaded { get; set; }
        public Action Completed { get; set; }

        public bool HasTargetScene => TargetSceneId.IsValid;
    }
}
