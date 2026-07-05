using System.Collections.Generic;
using Logger;
using MyGame.Events;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyGame.Item
{
    /// <summary>
    /// 栅栏管理器（一个GateManager = 一个Gate开关组）
    /// 挂载在Gate Tilemap的GameObject上。
    /// 
    /// 编辑器配置：
    ///   Gate_Tilemap  — 画栅栏tile，挂TilemapCollider2D(非Trigger) + GateManager
    ///   Switch_Tilemap — 画开关tile, 挂TilemapCollider2D(IsTrigger=true) + GateSwitch
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(TilemapCollider2D))]
    public class GateManager : MonoBehaviour
    {
        [Header("配置")]
        [Tooltip("是否可反复开关")]
        [SerializeField] private bool m_canToggleBack = false;

        [Tooltip("关卡开始时栅栏是否处于打开状态")]
        [SerializeField] private bool m_startOpen = false;

        [Header("栅栏tile")]
        [Tooltip("关闭状态的栅栏tile")]
        [SerializeField] private TileBase m_gateClosedTile;

        [Tooltip("开启状态的栅栏tile")]
        [SerializeField] private TileBase m_gateOpenTile;

        private Tilemap m_gateTilemap;
        private TilemapCollider2D m_gateCollider;
        private bool m_isOpen;

        // 栅栏cell列表（Awake时扫描）
        private List<Vector3Int> m_gateCells = new();
        private const string LOG_MODULE = "Gate";

        private void Awake()
        {
            m_gateTilemap = GetComponent<Tilemap>();
            m_gateCollider = GetComponent<TilemapCollider2D>();
            ScanGateTilemap();
            GameEvents.OnQuickRestart += OnQuickRestart;
        }

        private void OnDestroy()
        {
            GameEvents.OnQuickRestart -= OnQuickRestart;
        }

        #region 初始化

        private void ScanGateTilemap()
        {
            m_gateCells.Clear();
            foreach (Vector3Int cell in m_gateTilemap.cellBounds.allPositionsWithin)
            {
                if (m_gateTilemap.GetTile(cell) != null)
                {
                    m_gateCells.Add(cell);
                }
            }
        }

        /// <summary>
        /// 根据初始配置应用开关状态
        /// </summary>
        private void Start()
        {
            if (m_startOpen)
            {
                m_isOpen = true;
                OpenGates();
            }
        }

        #endregion

        #region 触发

        /// <summary>
        /// 由GateSwitch调用的公开入口
        /// </summary>
        public void OnSwitchTriggered(Vector3Int switchCell)
        {
            Toggle();
            Log.Info(LOG_MODULE, $"开关 {switchCell} 触发");
        }

        private void Toggle()
        {
            m_isOpen = !m_isOpen;

            if (m_isOpen)
            {
                OpenGates();
            }
            else
            {
                if (!m_canToggleBack) return;
                CloseGates();
            }
        }

        private void OpenGates()
        {
            Log.Info(LOG_MODULE, $"打开 {m_gateCells.Count} 个栅栏");

            foreach (var cell in m_gateCells)
            {
                m_gateTilemap.SetTile(cell, m_gateOpenTile);
            }
            m_gateCollider.enabled = false;
        }

        private void CloseGates()
        {
            Log.Info(LOG_MODULE, $"关闭 {m_gateCells.Count} 个栅栏");

            foreach (var cell in m_gateCells)
            {
                m_gateTilemap.SetTile(cell, m_gateClosedTile);
            }
            m_gateCollider.enabled = true;
        }

        #endregion

        #region 重置

        private void OnQuickRestart()
        {
            if (m_startOpen)
            {
                m_isOpen = true;
                OpenGates();
            }
            else
            {
                m_isOpen = false;
                CloseGates();
            }
        }

        #endregion
    }
}
