using UnityEngine;

namespace GGJ2026
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Prefabs")]
        [SerializeField] private MainMenuView mainMenuPrefab;
        [SerializeField] private ResetHintView resetHintPrefab;

        private MainMenuView mainMenuInstance;
        private ResetHintView resetHintInstance;

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
    }
}
