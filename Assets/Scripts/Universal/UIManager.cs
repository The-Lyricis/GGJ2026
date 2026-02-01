using UnityEngine;

namespace GGJ2026
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Prefabs")]
        [SerializeField] private MainMenuView mainMenuPrefab;
        [SerializeField] private ResetHintView resetHintPrefab;

        [Header("Transition Prefab View")]
        [SerializeField] private ScreenFaderView screenFaderPrefab;

        private MainMenuView mainMenuInstance;
        private ResetHintView resetHintInstance;
        private ScreenFaderView screenFaderInstance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private MainMenuView GetOrCreateMainMenu()
        {
            if (mainMenuInstance != null) return mainMenuInstance;
            if (mainMenuPrefab == null)
            {
                Debug.LogError("[UIManager] mainMenuPrefab not assigned.");
                return null;
            }
            mainMenuInstance = Instantiate(mainMenuPrefab, transform);
            return mainMenuInstance;
        }

        private ResetHintView GetOrCreateResetHint()
        {
            if (resetHintInstance != null) return resetHintInstance;
            if (resetHintPrefab == null)
            {
                Debug.LogError("[UIManager] resetHintPrefab not assigned.");
                return null;
            }
            resetHintInstance = Instantiate(resetHintPrefab, transform);
            return resetHintInstance;
        }

        private ScreenFaderView GetOrCreateScreenFader()
        {
            if (screenFaderInstance != null) return screenFaderInstance;
            if (screenFaderPrefab == null)
            {
                Debug.LogError("[UIManager] screenFaderPrefab not assigned.");
                return null;
            }

            screenFaderInstance = Instantiate(screenFaderPrefab, transform);

            // 安全起见：确保一开始是透明并放行输入
            screenFaderInstance.SetAlpha(0f, blockInput: false);

            return screenFaderInstance;
        }

        public void ShowMainMenu(bool show)
        {
            var view = GetOrCreateMainMenu();
            if (view != null) view.SetVisible(show);
        }

        public void SetResetHintVisible(bool visible)
        {
            var view = GetOrCreateResetHint();
            if (view != null) view.SetVisible(visible);
        }

        public void SetResetHintText(string text)
        {
            var view = GetOrCreateResetHint();
            if (view != null) view.SetText(text);
        }

        // =========================
        // Transition API
        // =========================

        public void FadeInBlack(float duration = 0.25f)
        {
            var view = GetOrCreateScreenFader();
            if (view != null) view.FadeIn(duration);
        }

        public void FadeOutBlack(float duration = 0.25f)
        {
            var view = GetOrCreateScreenFader();
            if (view != null) view.FadeOut(duration);
        }

        public void FadeTransition(System.Action middleAction, float fadeIn = 0.2f, float hold = 0.05f, float fadeOut = 0.2f)
        {
            var view = GetOrCreateScreenFader();
            if (view != null) view.FadeTransition(middleAction, fadeIn, hold, fadeOut);
            else middleAction?.Invoke();
        }
        public ScreenFaderView GetScreenFaderView()
        {
            return GetOrCreateScreenFader();
        }
    }
}
