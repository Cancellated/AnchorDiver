using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyGame.Item
{
    /// <summary>
    /// 开关触发器中转 — 挂载在Switch_Tilemap上，管理自身tile视觉 + 转发事件给GateManager
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(TilemapCollider2D))]
    public class GateSwitch : MonoBehaviour
    {
        [Header("目标")]
        [Tooltip("本开关控制的GateManager")]
        [SerializeField] private GateManager m_gateManager;

        [Header("开关tile")]
        [Tooltip("关闭状态的tile")]
        [SerializeField] private TileBase m_offTile;

        [Tooltip("开启状态的tile")]
        [SerializeField] private TileBase m_onTile;

        [Header("配置")]
        [Tooltip("两次触发的最小间隔（秒）")]
        [SerializeField] private float m_triggerCooldown = 0.2f;

        [Tooltip("关卡开始时开关是否处于开启状态")]
        [SerializeField] private bool m_startOn = false;

        private Tilemap m_tilemap;
        private bool m_isOn;
        private float m_lastTriggerTime = -99f;

        private void Awake()
        {
            m_tilemap = GetComponent<Tilemap>();
        }

        /// <summary>
        /// 根据初始配置设置开关tile显示
        /// </summary>
        private void Start()
        {
            if (m_startOn)
            {
                m_isOn = true;
                SetAllSwitchTiles(m_onTile);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") && !other.CompareTag("Anchor")) return;
            TryTrigger(other.transform.position);
        }

        /// <summary>
        /// 供外部直接调用（如锚的CircleCast检测到开关触发器时）
        /// </summary>
        public void TryTrigger(Vector3 position)
        {
            if (Time.time - m_lastTriggerTime < m_triggerCooldown) return;
            m_lastTriggerTime = Time.time;

            Vector3Int cell = FindNearestSwitchCell(position);
            if (m_tilemap.GetTile(cell) == null) return;

            m_isOn = !m_isOn;
            m_tilemap.SetTile(cell, m_isOn ? m_onTile : m_offTile);

            if (m_gateManager != null)
            {
                m_gateManager.OnSwitchTriggered(cell);
            }
        }

        /// <summary>
        /// 搜索离玩家最近的开关cell
        /// </summary>
        private Vector3Int FindNearestSwitchCell(Vector3 playerPos)
        {
            Vector3Int nearest = default;
            float minDist = float.MaxValue;

            foreach (Vector3Int cell in m_tilemap.cellBounds.allPositionsWithin)
            {
                if (m_tilemap.GetTile(cell) == null) continue;

                float dist = Vector3.Distance(playerPos, m_tilemap.GetCellCenterWorld(cell));
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = cell;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 将Tilemap上所有开关cell设置为指定tile
        /// </summary>
        private void SetAllSwitchTiles(TileBase tile)
        {
            foreach (Vector3Int cell in m_tilemap.cellBounds.allPositionsWithin)
            {
                if (m_tilemap.GetTile(cell) != null)
                {
                    m_tilemap.SetTile(cell, tile);
                }
            }
        }
    }
}
