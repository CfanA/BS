using BS.Gameplay.Player;
using UnityEngine;

namespace BS.Gameplay.SceneFlow
{
    /// <summary>
    /// 玩家出生点接收器。
    /// 由全局出生点服务在场景加载完成后调用。
    /// </summary>
    public sealed class PlayerSpawnReceiver : MonoBehaviour
    {
        [Header("依赖引用")]
        [SerializeField] private PlayerMotor2D motor;

        public Foundation.Ids.SpawnPointId CurrentSpawnPointId { get; private set; }

        private void Awake()
        {
            if (motor == null)
            {
                motor = GetComponent<PlayerMotor2D>();
            }
        }

        public void ApplySpawn(SceneSpawnPoint spawnPoint)
        {
            if (spawnPoint == null)
            {
                return;
            }

            var targetPosition = spawnPoint.transform.position;
            targetPosition.z = transform.position.z;
            transform.position = targetPosition;
            CurrentSpawnPointId = spawnPoint.SpawnPointId;

            if (motor != null)
            {
                motor.StopImmediately();
            }
        }
    }
}
