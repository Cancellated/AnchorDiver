using Logger;
using MyGame.Events;
using UnityEngine;

namespace MyGame.Control
{
    /// <summary>
    /// 危险区域触发器
    /// 挂载在有TilemapCollider2D（IsTrigger=true）的尖刺/陷阱Tilemap上
    /// 玩家进入时触发死亡
    /// </summary>
    public class HazardTrigger : MonoBehaviour
    {
        private const string LOG_MODULE = LogModules.PLAYER;

        /// <summary>
        /// 检测玩家进入触发区域
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Log.Info(LOG_MODULE, "玩家触碰尖刺，触发死亡");
                GameEvents.TriggerGameOver(false);
            }
        }
    }
}
