using MyGame.UI.DeathOverlay.Controller;
using TMPro;
using UnityEngine;

namespace MyGame.UI.DeathOverlay.View
{
    /// <summary>
    /// 死亡覆盖层视图
    /// 玩家死亡时显示提示文字，按R重开
    /// </summary>
    public class DeathOverlayView : BaseView<DeathOverlayController>
    {
        [Header("UI组件")]
        [Tooltip("提示文本组件")]
        [SerializeField] private TextMeshProUGUI m_hintText;

        private const string HINT_TEXT = "按 R 键重新开始";

        /// <summary>
        /// 初始化面板
        /// </summary>
        protected override void Awake()
        {
            m_panelType = UIType.DeathOverlay;
            base.Awake();

            // 设置提示文字
            if (m_hintText != null)
            {
                m_hintText.text = HINT_TEXT;
            }
        }

        /// <summary>
        /// 尝试绑定控制器（从父级或自身获取）
        /// </summary>
        protected override void TryBindController()
        {
            var controller = GetComponentInParent<DeathOverlayController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<DeathOverlayController>();
            }
            BindController(controller);
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public override void Show()
        {
            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = 1f;
                m_canvasGroup.interactable = true;
                m_canvasGroup.blocksRaycasts = true;
            }
            IsVisible = true;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public override void Hide()
        {
            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = 0f;
                m_canvasGroup.interactable = false;
                m_canvasGroup.blocksRaycasts = false;
            }
            IsVisible = false;
        }
    }
}
