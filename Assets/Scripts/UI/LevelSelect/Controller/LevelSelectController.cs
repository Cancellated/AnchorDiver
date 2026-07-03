using Logger;
using MyGame.Data;
using MyGame.Events;
using MyGame.Managers;
using MyGame.UI.LevelSelect.Model;
using MyGame.UI.LevelSelect.View;
using UnityEngine;

namespace MyGame.UI.LevelSelect.Controller
{
    /// <summary>
    /// 选关界面控制器
    /// 负责协调关卡数据加载和场景跳转，点击关卡按钮直接进入
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        #region 配置

        [Header("关卡配置")]
        [Tooltip("关卡列表配置文件")]
        [SerializeField] private LevelListConfig m_levelListConfig;

        [Header("场景名称")]
        [Tooltip("主菜单场景名称")]
        [SerializeField] private string m_mainMenuScene = "MainMenu";

        #endregion

        #region MVC组件

        [Header("MVC组件")]
        [Tooltip("选关模型")]
        [SerializeField] private LevelSelectModel m_model;

        [Tooltip("选关视图")]
        [SerializeField] private LevelSelectView m_view;

        private const string LOG_MODULE = LogModules.LEVELSELECT;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化控制器和MVC组件
        /// </summary>
        private void Awake()
        {
            InitializeMVCComponents();
        }

        #endregion

        #region MVC组件初始化

        /// <summary>
        /// 初始化MVC组件，建立模型和视图之间的连接
        /// </summary>
        private void InitializeMVCComponents()
        {
            // 初始化模型
            m_model ??= new LevelSelectModel();
            if (!m_model.IsInitialized)
            {
                m_model.Initialize();
            }

            // 加载关卡配置数据
            if (m_levelListConfig != null)
            {
                m_model.LoadLevelsFromConfig(m_levelListConfig.levels);
            }

            // 初始化视图
            if (m_view == null)
            {
                m_view = GetComponentInChildren<LevelSelectView>(true);
            }

            if (m_view != null)
            {
                m_view.BindController(this);
                // 生成关卡按钮
                m_view.PopulateLevelButtons(m_model.Levels);
                // 显示选关面板（BaseView.Awake默认隐藏，需显式调用Show）
                m_view.Show();
            }
            else
            {
                Log.Warning(LOG_MODULE, "LevelSelectView 未找到");
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 关卡按钮点击，直接加载对应场景
        /// </summary>
        /// <param name="index">关卡索引</param>
        public void OnLevelButtonClicked(int index)
        {
            if (index < 0 || index >= m_model.Levels.Count)
            {
                Log.Warning(LOG_MODULE, $"无效的关卡索引: {index}");
                return;
            }

            LevelData level = m_model.Levels[index];
            Log.Info(LOG_MODULE, $"进入关卡: {level.levelName} → 加载场景 {level.sceneName}");

            // 切换到GamePlay输入模式（关卡场景需要玩家操作）
            InputManager.Instance.SwitchToGamePlayMode();

            // 触发游戏开始事件，加载关卡场景
            GameEvents.TriggerGameStart();
            SceneSwitcher.RequestLoadScene(level.sceneName);
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void OnBackToMainMenu()
        {
            Log.Info(LOG_MODULE, "返回主菜单");
            SceneSwitcher.RequestLoadScene(m_mainMenuScene);
        }

        #endregion
    }
}
