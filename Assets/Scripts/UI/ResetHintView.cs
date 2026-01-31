using UnityEngine;
using UnityEngine.UIElements;

namespace GGJ2026
{
    /// <summary>
    /// ResetHint UI 的“视图脚本”：
    /// - 绑定 UIDocument 的 root
    /// - 提供 SetVisible / SetText 等接口
    /// 注意：它不负责输入、不负责逻辑，只负责 UI 表现。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ResetHintView : MonoBehaviour
    {
        private UIDocument doc;
        private Label hintLabel;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();

            // UIDocument.rootVisualElement 在 Awake 通常可用；如遇到时序问题可挪到 OnEnable
            var root = doc.rootVisualElement;

            hintLabel = root.Q<Label>("hintLabel");
            if (hintLabel == null)
                Debug.LogWarning("[ResetHintView] Cannot find Label 'hintLabel' in UXML.");
        }

        public void SetVisible(bool visible)
        {
            // 直接关掉 GameObject 或 UIDocument 均可
            gameObject.SetActive(visible);
        }

        public void SetText(string text)
        {
            if (hintLabel != null) hintLabel.text = text;
        }
    }
}
