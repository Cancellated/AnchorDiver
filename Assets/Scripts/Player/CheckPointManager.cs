using Logger;
using MyGame.Events;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyGame.Managers
{
    /// <summary>
    /// 关卡通重生管理器（挂载在CheckPoint Tilemap上）
    /// 使用Tilemap层管理存档点：直接在关卡编辑器中绘制存档点tile即可
    /// 玩家经过存档点tile时自动激活，按R重生时传送到最近激活的存档点
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(TilemapCollider2D))]
    public class CheckPointManager : MonoBehaviour
    {
        [Header("玩家引用")]
        [Tooltip("玩家Transform（用于传送位置）")]
        [SerializeField] private Transform m_playerTransform;

        [Header("视觉反馈")]
        [Tooltip("是否高亮当前激活的存档点tile")]
        [SerializeField] private bool m_highlightActive = true;

        [Tooltip("激活存档点的tile颜色")]
        [SerializeField] private Color m_activeColor = Color.green;

        private Tilemap m_tilemap;
        private Vector3Int m_currentCheckPointCell;
        private Color m_defaultTileColor = Color.white;
        private const string LOG_MODULE = "CheckPoint";

        #region 属性

        /// <summary>
        /// 当前存档点世界坐标
        /// </summary>
        public Vector3 CurrentCheckPointWorld
        {
            get { return m_tilemap.GetCellCenterWorld(m_currentCheckPointCell); }
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化Tilemap引用和初始存档点
        /// </summary>
        private void Start()
        {
            m_tilemap = GetComponent<Tilemap>();

            // 确保Collider是Trigger
            var col = GetComponent<TilemapCollider2D>();
            col.isTrigger = true;

            // 自动查找玩家
            if (m_playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    m_playerTransform = player.transform;
                }
            }

            // 初始存档点 = 玩家当前位置对应的存档点tile（就近原则）
            if (m_playerTransform != null)
            {
                m_currentCheckPointCell = m_tilemap.WorldToCell(m_playerTransform.position);
                // 如果该位置没有tile，查找最近的一个
                if (m_tilemap.GetTile(m_currentCheckPointCell) == null)
                {
                    m_currentCheckPointCell = FindNearestCheckPointCell(m_playerTransform.position);
                }
            }
            else
            {
                m_currentCheckPointCell = FindFirstCheckPointCell();
            }

            HighlightCell(m_currentCheckPointCell);
            Log.Info(LOG_MODULE, $"存档点管理器初始化，初始存档点: {m_currentCheckPointCell}");
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void OnEnable()
        {
            GameEvents.OnQuickRestart += OnQuickRestart;
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        private void OnDisable()
        {
            GameEvents.OnQuickRestart -= OnQuickRestart;
        }

        #endregion

        #region 存档点触发

        /// <summary>
        /// 玩家进入存档点Tile区域时，激活离玩家最近的存档点tile
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Vector3Int nearest = FindNearestCheckPointCell(other.transform.position);
            if (nearest == default && m_tilemap.GetTile(nearest) == null) return;
            if (nearest == m_currentCheckPointCell) return;

            // 取消旧存档点高亮
            RemoveHighlight(m_currentCheckPointCell);

            // 激活新存档点
            m_currentCheckPointCell = nearest;
            HighlightCell(m_currentCheckPointCell);

            Log.Info(LOG_MODULE, $"激活存档点: {m_currentCheckPointCell}");
        }

        #endregion

        #region 重生逻辑

        /// <summary>
        /// 快速重开事件回调：将玩家传送到当前存档点
        /// </summary>
        private void OnQuickRestart()
        {
            if (m_playerTransform == null)
            {
                Log.Warning(LOG_MODULE, "玩家Transform未设置，无法重生");
                return;
            }

            Vector3 respawnPos = m_tilemap.GetCellCenterWorld(m_currentCheckPointCell);
            m_playerTransform.position = respawnPos;
            Log.Info(LOG_MODULE, $"玩家已重生至存档点: {m_currentCheckPointCell}");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 高亮指定存档点tile
        /// </summary>
        private void HighlightCell(Vector3Int cellPos)
        {
            if (!m_highlightActive) return;
            m_tilemap.SetTileFlags(cellPos, TileFlags.None);
            m_tilemap.SetColor(cellPos, m_activeColor);
        }

        /// <summary>
        /// 移除指定tile的高亮
        /// </summary>
        private void RemoveHighlight(Vector3Int cellPos)
        {
            m_tilemap.SetColor(cellPos, Color.white);
        }

        /// <summary>
        /// 查找最接近指定位置的存档点tile
        /// </summary>
        private Vector3Int FindNearestCheckPointCell(Vector3 worldPos)
        {
            Vector3Int playerCell = m_tilemap.WorldToCell(worldPos);
            Vector3Int nearest = Vector3Int.zero;
            float nearestDist = float.MaxValue;
            bool found = false;

            BoundsInt bounds = m_tilemap.cellBounds;
            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                if (m_tilemap.GetTile(cell) != null)
                {
                    float dist = Vector3Int.Distance(cell, playerCell);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = cell;
                        found = true;
                    }
                }
            }

            return found ? nearest : FindFirstCheckPointCell();
        }

        /// <summary>
        /// 查找第一个存档点tile（遍历tilemap）
        /// </summary>
        private Vector3Int FindFirstCheckPointCell()
        {
            BoundsInt bounds = m_tilemap.cellBounds;
            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                if (m_tilemap.GetTile(cell) != null)
                {
                    return cell;
                }
            }
            return Vector3Int.zero;
        }

        #endregion
    }
}
