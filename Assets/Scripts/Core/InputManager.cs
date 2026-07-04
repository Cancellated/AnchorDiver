using UnityEngine;
using Logger;

namespace MyGame.Managers
{
    /// <summary>
    /// 输入管理器，集中管理InputSystem实例
    /// 避免重复创建InputSystem实例，确保输入系统状态一致性
    /// </summary>
    public class InputManager : Singleton<InputManager>
    {
        #region 字段
        private GameControl _inputActions;
        private const string LOG_MODULE = LogModules.INPUT;
        #endregion

        #region 属性
        /// <summary>
        /// 全局唯一的InputActions实例
        /// </summary>
        public GameControl InputActions
        {
            get { return _inputActions; }
        }
        #endregion

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            
            // 创建InputActions实例
            _inputActions = new GameControl();
            
            // 默认启用游戏玩法输入
            _inputActions.GamePlay.Enable();
        }

        private void OnDestroy()
        {
            // 清理资源
            if (_inputActions != null)
            {
                _inputActions.Disable();
                _inputActions.Dispose();
            }
        }
        #endregion

        #region 输入模式切换
        /// <summary>
        /// 切换到游戏玩法输入模式
        /// </summary>
        public void SwitchToGamePlayMode()
        {
            Log.Info(LOG_MODULE, "切换到游戏操作输入模式");
            _inputActions.UI.Disable();
            _inputActions.GamePlay.Enable();
        }

        /// <summary>
        /// 切换到UI输入模式
        /// 特殊处理：保留控制台按键的功能，即使在UI模式下也能响应
        /// </summary>
        public void SwitchToUIMode()
        {
            Log.Info(LOG_MODULE, "切换到UI输入模式");
            _inputActions.GamePlay.Disable();
            // 单独启用控制台按键，确保在任何模式下都能唤出控制台
            _inputActions.GamePlay.Console.Enable();
            _inputActions.UI.Enable();
        }

        /// <summary>
        /// 同时启用游戏玩法和UI输入模式
        /// </summary>
        public void EnableBothModes()
        {
            Log.Info(LOG_MODULE, "同时启用游戏操作和UI输入模式");
            _inputActions.GamePlay.Enable();
            _inputActions.UI.Enable();
        }

        /// <summary>
        /// 禁用所有输入
        /// </summary>
        public void DisableAllInputs()
        {
            Log.Info(LOG_MODULE, "禁用所有输入");
            _inputActions.GamePlay.Disable();
            _inputActions.UI.Disable();
        }

        #endregion

        #region 调试工具
        /// <summary>
        /// 当前GamePlay操作模式是否启用
        /// </summary>
        public bool IsGamePlayEnabled
        {
            get { return _inputActions != null && _inputActions.GamePlay.enabled; }
        }

        /// <summary>
        /// 当前UI操作模式是否启用
        /// </summary>
        public bool IsUIEnabled
        {
            get { return _inputActions != null && _inputActions.UI.enabled; }
        }

        /// <summary>
        /// 获取当前输入模式描述（调试用）
        /// </summary>
        public string GetModeDescription()
        {
            bool gp = IsGamePlayEnabled;
            bool ui = IsUIEnabled;
            if (gp && ui) return "GamePlay + UI";
            if (gp) return "GamePlay";
            if (ui) return "UI";
            return "无";
        }
        #endregion
    }
}