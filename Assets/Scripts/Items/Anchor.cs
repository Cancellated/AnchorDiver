using Logger;
using MyGame.Events;
using UnityEngine;
using MyGame.Data;


namespace MyGame.Item
{
    /// <summary>
    /// 船锚行为脚本
    /// 使用Rigidbody2D重力下落，CircleCast检测碰撞和碎石破坏
    /// 锚定后将Rigidbody2D设为Static停止
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Anchor : MonoBehaviour
    {
        [Header("重力配置")]
        [Tooltip("锚下落的重力倍率")]
        [SerializeField] private float m_gravityScale = 2f;

        [Header("碰撞配置")]
        [Tooltip("Collision检测半径")]
        [SerializeField] private float m_hitRadius = 0.3f;

        private Rigidbody2D m_rigidbody;
        private RubbleManager m_rubbleManager;
        private bool m_isAnchored;
        private Transform m_attachedPlayer;

        private const string LOG_MODULE = "Anchor";

        #region 生命周期

        /// <summary>
        /// 初始化组件引用和物理状态
        /// </summary>
        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody2D>();
            m_rigidbody.gravityScale = m_gravityScale;
            m_rigidbody.bodyType = RigidbodyType2D.Dynamic;

            m_rubbleManager = FindFirstObjectByType<RubbleManager>();
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void OnEnable()
        {
            GameEvents.OnGameOver += OnGameOver;
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        private void OnDisable()
        {
            GameEvents.OnGameOver -= OnGameOver;
        }

        /// <summary>
        /// 每帧检测下方碰撞
        /// </summary>
        private void Update()
        {
            if (m_isAnchored) return;

            CheckCollision();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 游戏结束事件回调（胜负均触发）
        /// 若锚抓着玩家则解绑，防止后续锚定时重复触发死亡
        /// </summary>
        private void OnGameOver(bool isWin)
        {
            if (m_attachedPlayer != null)
            {
                Log.Info(LOG_MODULE, "游戏结束，释放被抓住的玩家");
                DetachPlayer();
            }
        }

        /// <summary>
        /// 将玩家从锚上解绑，恢复其独立状态
        /// </summary>
        private void DetachPlayer()
        {
            if (m_attachedPlayer == null) return;

            // 恢复玩家物理状态
            Rigidbody2D playerRb = m_attachedPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.bodyType = RigidbodyType2D.Dynamic;
            }

            m_attachedPlayer.SetParent(null);
            m_attachedPlayer = null;
        }

        /// <summary>
        /// CircleCastAll检测下方碰撞，处理所有碎石破坏和地形锚定
        /// </summary>
        private void CheckCollision()
        {
            Vector2 origin = transform.position;
            RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, m_hitRadius, Vector2.down, 0.1f,
                LayerMask.GetMask("Default"));

            if (hits.Length == 0) return;

            bool hitSolid = false;
            string solidName = "";

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                // 砸中玩家 → 抓住玩家随锚下落，锚定时再击杀（给目标点留判定窗口）
                if (hit.collider.CompareTag("Player"))
                {
                    Log.Info(LOG_MODULE, "锚砸中玩家，抓住随锚拖拽");
                    m_attachedPlayer = hit.collider.transform;
                    m_attachedPlayer.SetParent(transform);

                    // 切换为Kinematic，使其跟随锚移动同时保留碰撞/触发器检测
                    Rigidbody2D playerRb = hit.collider.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        playerRb.velocity = Vector2.zero;
                        playerRb.bodyType = RigidbodyType2D.Kinematic;
                    }

                    continue;
                }

                // 尝试销毁碎石
                if (m_rubbleManager != null)
                {
                    if (m_rubbleManager.DestroyTileAt(hit.point))
                    {
                        Log.Info(LOG_MODULE, "锚击碎碎石");
                        continue;
                    }
                }

                // 非碎石、非触发器的地形 → 锚停止
                if (!hit.collider.isTrigger)
                {
                    hitSolid = true;
                    solidName = hit.collider.name;
                }
                else
                {
                    // 触发器：主动触发开关（锚速度太快时物理事件可能漏掉）
                    var gateSwitch = hit.collider.GetComponent<GateSwitch>();
                    if (gateSwitch != null)
                    {
                        gateSwitch.TryTrigger(transform.position);
                    }
                }
            }

            if (hitSolid)
            {
                Log.Info(LOG_MODULE, $"锚命中地形: {solidName}");
                AnchorDown();
            }
        }

        /// <summary>
        /// 锚停止下落，设为Static固定在当前位置
        /// </summary>
        private void AnchorDown()
        {
            if (m_isAnchored) return;
            m_isAnchored = true;

            m_rigidbody.bodyType = RigidbodyType2D.Static;

            Log.Info(LOG_MODULE, "锚已锚定");

            // 通知锚落地，触发屏幕震动等效果
            GameEvents.TriggerAnchorLanded();

            // 锚定时若抓着玩家，击杀玩家
            if (m_attachedPlayer != null)
            {
                Log.Info(LOG_MODULE, "锚锚定，击杀被拖拽的玩家");
                GameEvents.TriggerGameOver(false);
            }
        }

        #endregion
    }
}
