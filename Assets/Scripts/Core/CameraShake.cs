using Logger;
using MyGame.Events;
using UnityEngine;

namespace MyGame.Managers
{
    /// <summary>
    /// 摄像机震动组件
    /// 监听锚落地等事件，通过协程产生屏幕震动效果
    /// 挂载在Main Camera上使用
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("震动参数")]
        [Tooltip("震动持续时间（秒）")]
        [SerializeField] private float m_duration = 0.1f;

        [Tooltip("震动强度（单位偏移量）")]
        [SerializeField] private float m_intensity = 0.1f;

        private Vector3 m_originalLocalPos;
        private Coroutine m_shakeCoroutine;

        private const string LOG_MODULE = "CameraShake";

        /// <summary>
        /// 初始化时记录摄像机原始位置
        /// </summary>
        private void Awake()
        {
            m_originalLocalPos = transform.localPosition;
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void OnEnable()
        {
            GameEvents.OnAnchorLanded += OnAnchorLanded;
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        private void OnDisable()
        {
            GameEvents.OnAnchorLanded -= OnAnchorLanded;
        }

        /// <summary>
        /// 锚落地时开始震动
        /// </summary>
        private void OnAnchorLanded()
        {
            if (m_shakeCoroutine != null)
            {
                StopCoroutine(m_shakeCoroutine);
            }
            m_shakeCoroutine = StartCoroutine(ShakeCoroutine());
        }

        /// <summary>
        /// 震动协程：每帧随机偏移摄像机位置，按衰减曲线逐渐归零
        /// </summary>
        private System.Collections.IEnumerator ShakeCoroutine()
        {
            Log.Info(LOG_MODULE, "开始屏幕震动");
            float elapsed = 0f;

            while (elapsed < m_duration)
            {
                // 衰减曲线：强度随时间递减，震动自然减弱
                float progress = elapsed / m_duration;
                float currentIntensity = m_intensity * (1f - progress * progress);

                float offsetX = Random.Range(-1f, 1f) * currentIntensity;
                float offsetY = Random.Range(-1f, 1f) * currentIntensity;

                transform.localPosition = m_originalLocalPos + new Vector3(offsetX, offsetY, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 震动结束，恢复原始位置
            transform.localPosition = m_originalLocalPos;
            m_shakeCoroutine = null;
        }
    }
}
