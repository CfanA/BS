using BS.Foundation.Ids;
using UnityEngine;

namespace BS.Gameplay.SceneFlow
{
    /// <summary>
    /// 场景出生点。
    /// 每个场景可配置多个入口，玩家切入后按 SpawnPointId 定位。
    /// </summary>
    public sealed class SceneSpawnPoint : MonoBehaviour
    {
        [Header("出生点配置")]
        [SerializeField] private string spawnPointId = "entry.default";
        [SerializeField] private bool isDefaultSpawn;

        public SpawnPointId SpawnPointId => new(spawnPointId);
        public bool IsDefaultSpawn => isDefaultSpawn;

        private void OnDrawGizmos()
        {
            Gizmos.color = isDefaultSpawn ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.75f);
        }
    }
}
