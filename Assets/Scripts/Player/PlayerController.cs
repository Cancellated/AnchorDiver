using Logger;
using MyGame.Events;
using MyGame.Managers;
using UnityEngine;

namespace MyGame.Control
{
    /// <summary>
    /// 玩家水下移动控制器
    /// 负责自动上浮、CD控制下潜和水平游泳
    /// 需要挂载在带有Rigidbody2D的GameObject上
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        #region 配置参数

        [Header("浮力设置")]
        [Tooltip("持续向上的浮力大小")]
        [SerializeField] private float m_buoyancyForce = 6f;

        [Header("下潜设置")]
        [Tooltip("下潜冲刺的瞬时力度")]
        [SerializeField] private float m_diveForce = 4f;

        [Tooltip("下潜冷却时间（秒）")]
        [SerializeField] private float m_diveCooldown = 1f;

        [Header("水平移动")]
        [Tooltip("水平游泳速度")]
        [SerializeField] private float m_swimSpeed = 4f;

        #endregion

        #region 内部状态

        private Rigidbody2D m_rigidbody;
        private GameControl m_inputActions;
        private float m_diveCooldownTimer;
        private bool m_isDiveReady = true;
        private bool m_isDead;

        private const string LOG_MODULE = LogModules.PLAYER;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化组件引用和物理设置
        /// </summary>
        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody2D>();

            // 禁用默认重力，完全由浮力控制垂直运动
            m_rigidbody.gravityScale = 0f;

            // 获取输入系统实例
            if (InputManager.Instance != null)
            {
                m_inputActions = InputManager.Instance.InputActions;
            }
            else
            {
                Log.Warning(LOG_MODULE, "InputManager 未找到，创建临时输入实例");
                m_inputActions = new GameControl();
                m_inputActions.GamePlay.Enable();
            }
        }

        /// <summary>
        /// 注册游戏结束事件监听
        /// </summary>
        private void OnEnable()
        {
            GameEvents.OnGameOver += OnGameOver;
        }

        /// <summary>
        /// 注销游戏结束事件监听
        /// </summary>
        private void OnDisable()
        {
            GameEvents.OnGameOver -= OnGameOver;
        }

        /// <summary>
        /// 每帧处理下潜CD计时（死亡后跳过）
        /// </summary>
        private void Update()
        {
            if (m_isDead) return;

            UpdateDiveCooldown();
            HandleDiveInput();
        }

        /// <summary>
        /// 固定物理帧处理浮力和水平移动（死亡后跳过）
        /// </summary>
        private void FixedUpdate()
        {
            if (m_isDead) return;

            ApplyBuoyancy();
            ApplyHorizontalMovement();
        }

        #endregion

        #region 死亡处理

        /// <summary>
        /// 游戏结束事件回调
        /// </summary>
        /// <param name="isWin">是否胜利</param>
        private void OnGameOver(bool isWin)
        {
            if (m_isDead) return;

            m_isDead = true;

            // 停止所有物理运动
            m_rigidbody.velocity = Vector2.zero;
            m_rigidbody.simulated = false;

            Log.Info(LOG_MODULE, $"玩家死亡，已禁用控制。胜利：{isWin}");
        }

        #endregion

        #region 浮力机制

        /// <summary>
        /// 施加持续向上的浮力，使角色自动上浮
        /// </summary>
        private void ApplyBuoyancy()
        {
            m_rigidbody.AddForce(Vector2.up * m_buoyancyForce, ForceMode2D.Force);
        }

        #endregion

        #region 下潜机制

        /// <summary>
        /// 更新下潜冷却计时器
        /// </summary>
        private void UpdateDiveCooldown()
        {
            if (!m_isDiveReady)
            {
                m_diveCooldownTimer -= Time.deltaTime;
                if (m_diveCooldownTimer <= 0f)
                {
                    m_isDiveReady = true;
                }
            }
        }

        /// <summary>
        /// 检测并处理下潜输入
        /// </summary>
        private void HandleDiveInput()
        {
            if (m_inputActions == null) return;

            // Jump键映射到下潜（反向跳跃）
            if (m_inputActions.GamePlay.Jump.triggered && m_isDiveReady)
            {
                PerformDive();
            }
        }

        /// <summary>
        /// 执行下潜冲刺
        /// </summary>
        private void PerformDive()
        {
            // 清零当前垂直速度，使下潜更干脆
            Vector2 velocity = m_rigidbody.velocity;
            velocity.y = 0f;
            m_rigidbody.velocity = velocity;

            // 施加瞬时向下的力
            m_rigidbody.AddForce(Vector2.down * m_diveForce, ForceMode2D.Impulse);

            // 进入冷却
            m_isDiveReady = false;
            m_diveCooldownTimer = m_diveCooldown;

            Log.Info(LOG_MODULE, $"下潜！冷却 {m_diveCooldown}s");
        }

        /// <summary>
        /// 下潜是否处于就绪状态（供外部查询，如HUD显示冷却指示器）
        /// </summary>
        public bool IsDiveReady
        {
            get { return m_isDiveReady; }
        }

        /// <summary>
        /// 下潜冷却进度（0~1，供外部查询）
        /// </summary>
        public float DiveCooldownProgress
        {
            get
            {
                if (m_isDiveReady) return 1f;
                return 1f - (m_diveCooldownTimer / m_diveCooldown);
            }
        }

        #endregion

        #region 水平移动

        /// <summary>
        /// 读取水平输入并施加速度
        /// 使用velocity直接设水平分量，保留垂直方向物理
        /// </summary>
        private void ApplyHorizontalMovement()
        {
            if (m_inputActions == null) return;

            Vector2 moveInput = m_inputActions.GamePlay.Move.ReadValue<Vector2>();
            float targetVelocityX = moveInput.x * m_swimSpeed;

            m_rigidbody.velocity = new Vector2(targetVelocityX, m_rigidbody.velocity.y);
        }

        #endregion
    }
}
