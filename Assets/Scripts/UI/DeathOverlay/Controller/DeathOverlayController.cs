using Logger;
using MyGame.Events;
using MyGame.Managers;
using MyGame.UI.DeathOverlay.View;
using UnityEngine;

namespace MyGame.UI.DeathOverlay.Controller
{
    /// <summary>
    /// 死亡覆盖层控制器
    /// 监听游戏结束事件，失败时显示重开提示
    /// 实际重开逻辑由 GameManager 通过 InputSystem 的 Restart Action 统一处理
    /// </summary>
    public class DeathOverlayController : MonoBehaviour
    {
        #region MVC组件

        [Header("MVC组件")]
        [Tooltip("死亡覆盖层视图")]
        [SerializeField] private DeathOverlayView m_view;

        private const string LOG_MODULE = LogModules.DEATHOVERLAY;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化视图引用并注册事件
        /// </summary>
        private void Awake()
        {
            InitializeView();

            GameEvents.OnGameOver += OnGameOver;
            GameEvents.OnQuickRestart += OnQuickRestart;
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        private void OnDestroy()
        {
            GameEvents.OnGameOver -= OnGameOver;
            GameEvents.OnQuickRestart -= OnQuickRestart;
        }

        #endregion

        #region MVC组件初始化

        /// <summary>
        /// 初始化视图引用
        /// </summary>
        private void InitializeView()
        {
            if (m_view == null)
            {
                m_view = GetComponentInChildren<DeathOverlayView>(true);
            }

            if (m_view != null)
            {
                m_view.BindController(this);
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 游戏结束事件回调，失败时显示死亡面板
        /// </summary>
        /// <param name="isWin">是否胜利，false为死亡</param>
        private void OnGameOver(bool isWin)
        {
            if (isWin) return;

            Log.Info(LOG_MODULE, "玩家死亡，显示重开提示");

            // 恢复时间流速，确保UI可正常交互
            Time.timeScale = 1f;

            // 切换到UI输入模式，确保PlayerController不再响应输入
            if (InputManager.Instance != null)
            {
                InputManager.Instance.SwitchToUIMode();
            }

            // 显示死亡面板
            if (m_view != null)
            {
                m_view.Show();
            }
        }

        /// <summary>
        /// 快速重开事件回调：隐藏死亡面板，切换回GamePlay模式
        /// </summary>
        private void OnQuickRestart()
        {
            Log.Info(LOG_MODULE, "快速重开，隐藏死亡面板");

            // 隐藏死亡面板
            if (m_view != null)
            {
                m_view.Hide();
            }

            // 切换回GamePlay输入模式
            if (InputManager.Instance != null)
            {
                InputManager.Instance.SwitchToGamePlayMode();
            }
        }

        #endregion
    }
}
