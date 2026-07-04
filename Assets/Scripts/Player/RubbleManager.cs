using System.Collections.Generic;
using Logger;
using MyGame.Events;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyGame.Control
{
    /// <summary>
    /// 碎石地形管理器
    /// 追踪被破坏的碎石tile，在快速重开时恢复
    /// 提供销毁tile的公共接口供锚系统调用
    /// </summary>
    public class RubbleManager : MonoBehaviour
    {
        [Header("Tilemap引用")]
        [Tooltip("碎石层Tilemap")]
        [SerializeField] private Tilemap m_rubbleTilemap;

        /// <summary>
        /// 记录被破坏的tile及其原始数据，用于重生时恢复
        /// </summary>
        private readonly Dictionary<Vector3Int, TileBase> m_destroyedTiles = new();

        private const string LOG_MODULE = "Rubble";

        #region 生命周期

        /// <summary>
        /// 注册快速重开事件
        /// </summary>
        private void OnEnable()
        {
            GameEvents.OnQuickRestart += RestoreAllTiles;
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        private void OnDisable()
        {
            GameEvents.OnQuickRestart -= RestoreAllTiles;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 销毁指定位置的碎石tile（由锚系统调用）
        /// </summary>
        /// <param name="worldPosition">世界坐标位置</param>
        /// <returns>是否成功销毁</returns>
        public bool DestroyTileAt(Vector3 worldPosition)
        {
            if (m_rubbleTilemap == null)
            {
                Log.Warning(LOG_MODULE, "碎石Tilemap未设置");
                return false;
            }

            Vector3Int cellPos = m_rubbleTilemap.WorldToCell(worldPosition);
            TileBase tile = m_rubbleTilemap.GetTile(cellPos);

            if (tile == null) return false;

            // 记录原始tile以便重生时恢复
            m_destroyedTiles[cellPos] = tile;

            // 移除tile
            m_rubbleTilemap.SetTile(cellPos, null);

            Log.Info(LOG_MODULE, $"碎石已销毁: {cellPos}");

            return true;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 快速重开时恢复所有被破坏的tile
        /// </summary>
        private void RestoreAllTiles()
        {
            if (m_rubbleTilemap == null) return;

            int count = m_destroyedTiles.Count;
            if (count == 0) return;

            foreach (var kvp in m_destroyedTiles)
            {
                m_rubbleTilemap.SetTile(kvp.Key, kvp.Value);
            }

            m_destroyedTiles.Clear();
            Log.Info(LOG_MODULE, $"已恢复 {count} 块碎石");
        }

        #endregion
    }
}
