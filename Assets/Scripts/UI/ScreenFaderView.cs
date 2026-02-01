using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace GGJ2026
{
    /// <summary>
    /// UI Toolkit 黑屏转场 View（Prefab View）
    /// - 通过控制全屏 VisualElement 的 opacity 实现淡入/淡出
    /// - 通过 pickingMode 控制是否拦截输入（兼容旧版 UI Toolkit）
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ScreenFaderView : MonoBehaviour
    {
        [Header("Optional")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Query Names")]
        [SerializeField] private string faderRootName = "FaderRoot";

        private VisualElement root;
        private VisualElement fader;
        private Coroutine running;

        private void Awake()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            Build();
        }

        private void OnEnable()
        {
            // 处理“脚本启用早于 UIDocument 初始化”的边界情况
            if (fader == null) Build();
        }

        private void Build()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[ScreenFaderView] UIDocument missing.");
                return;
            }

            root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("[ScreenFaderView] rootVisualElement is null. Check UIDocument/PanelSettings.");
                return;
            }

            fader = root.Q<VisualElement>(faderRootName);
            if (fader == null)
            {
                Debug.LogError($"[ScreenFaderView] Cannot find VisualElement named '{faderRootName}' in UXML.");
                return;
            }

            // 初始化为透明且不拦截输入
            SetAlpha(0f, blockInput: false);
        }

        /// <summary>
        /// 立即设置黑幕透明度与输入拦截。
        /// </summary>
        public void SetAlpha(float alpha, bool blockInput)
        {
            if (fader == null) return;

            alpha = Mathf.Clamp01(alpha);
            fader.style.opacity = alpha;

            // 旧版 UI Toolkit：使用 pickingMode 控制输入穿透
            fader.pickingMode = blockInput ? PickingMode.Position : PickingMode.Ignore;
        }

        /// <summary>
        /// 淡入到黑（0 -> 1）
        /// </summary>
        public void FadeIn(float duration = 0.25f)
        {
            StartFade(targetAlpha: 1f, duration, blockInputDuringFade: true);
        }

        /// <summary>
        /// 从黑淡出（1 -> 0）
        /// </summary>
        public void FadeOut(float duration = 0.25f)
        {
            StartFade(targetAlpha: 0f, duration, blockInputDuringFade: true);
        }

        /// <summary>
        /// 常用：淡入(到黑) -> 执行动作 -> 淡出(回到可见)
        /// middleAction 可用于 LoadScene / 切关逻辑等。
        /// </summary>
        public void FadeTransition(Action middleAction, float fadeIn = 0.2f, float hold = 0.05f, float fadeOut = 0.2f)
        {
            if (fader == null) return;

            if (running != null) StopCoroutine(running);
            running = StartCoroutine(CoFadeTransition(middleAction, fadeIn, hold, fadeOut));
        }

        private void StartFade(float targetAlpha, float duration, bool blockInputDuringFade)
        {
            if (fader == null) return;

            if (running != null) StopCoroutine(running);
            running = StartCoroutine(CoFadeTo(targetAlpha, duration, blockInputDuringFade));
        }

        private IEnumerator CoFadeTransition(Action middleAction, float fadeIn, float hold, float fadeOut)
        {
            // 转场期间拦截输入
            fader.pickingMode = PickingMode.Position;

            // 1) 淡入到黑
            yield return CoFadeTo(1f, fadeIn, blockInputDuringFade: true);

            // 2) 中间动作（例如加载关卡）
            middleAction?.Invoke();

            // 3) 黑屏停留（避免闪烁）
            if (hold > 0f) yield return new WaitForSeconds(hold);

            // 4) 淡出回画面
            yield return CoFadeTo(0f, fadeOut, blockInputDuringFade: true);


            // 完成后放行输入
            fader.pickingMode = PickingMode.Ignore;
            running = null;
        }

        private IEnumerator CoFadeTo(float target, float duration, bool blockInputDuringFade)
        {
            target = Mathf.Clamp01(target);
            float start = fader.resolvedStyle.opacity;

            // 动画期间是否拦截输入
            fader.pickingMode = blockInputDuringFade ? PickingMode.Position : PickingMode.Ignore;

            // duration<=0：立即到位
            if (duration <= 0f)
            {
                SetAlpha(target, blockInput: blockInputDuringFade);
                running = null;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                // 使用 unscaledDeltaTime：即使 Time.timeScale=0 也能完成转场
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                fader.style.opacity = Mathf.Lerp(start, target, k);
                yield return null;
            }

            fader.style.opacity = target;
            if (Mathf.Approximately(target, 0f))
            {
                // 透明时通常放行输入（由上层逻辑决定，这里给一个安全默认）
                fader.pickingMode = PickingMode.Ignore;
            }

            running = null;
        }
        public IEnumerator FadeToAndWait(float targetAlpha, float duration, bool blockInputDuringFade = true)
        {
            if (fader == null) yield break;

            // 终止正在运行的转场，避免叠加
            if (running != null) StopCoroutine(running);

            // 直接跑内部协程，并把 running 记录下来
            running = StartCoroutine(CoFadeTo(Mathf.Clamp01(targetAlpha), duration, blockInputDuringFade));

            // 等待这一段转场完成
            yield return running;
        }
    }
}
