using UnityEngine;
using UnityEngine.UIElements;

namespace GGJ2026
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuView : MonoBehaviour
    {
        [Header("Logo (Optional)")]
        [SerializeField] private Sprite logoSprite;

        [Header("Scene Indices (Build Settings)")]
        [SerializeField] private int firstLevelIndex = 1;

        [Header("Hotkeys")]
        [SerializeField] private bool enterToStart = true;

        private VisualElement _root;
        private VisualElement _logo;
        private Button _btnStart;
        private Button _btnContinue;
        private Button _btnQuit;

        private void Awake()
        {
            var doc = GetComponent<UIDocument>();
            _root = doc.rootVisualElement;

            _logo = _root.Q<VisualElement>("logo");
            _btnStart = _root.Q<Button>("btnStart");
            _btnContinue = _root.Q<Button>("btnContinue");
            _btnQuit = _root.Q<Button>("btnQuit");

            if (_logo != null && logoSprite != null)
                _logo.style.backgroundImage = new StyleBackground(logoSprite);

            if (_btnStart != null) _btnStart.clicked += OnStartClicked;
            if (_btnContinue != null) _btnContinue.clicked += OnContinueClicked;
            if (_btnQuit != null) _btnQuit.clicked += OnQuitClicked;
        }

        private void OnDestroy()
        {
            if (_btnStart != null) _btnStart.clicked -= OnStartClicked;
            if (_btnContinue != null) _btnContinue.clicked -= OnContinueClicked;
            if (_btnQuit != null) _btnQuit.clicked -= OnQuitClicked;
        }

        private void Update()
        {
            if (!enterToStart) return;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                OnStartClicked();
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void OnStartClicked()
        {
            AudioManager.Instance.PlaySFX("UIClick");
            // 进入第一关 + 切到 Playing
            if (LevelManager.Instance != null)
                LevelManager.Instance.LoadLevel(firstLevelIndex);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(firstLevelIndex);

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);
        }

        private void OnContinueClicked()
        {
            AudioManager.Instance.PlaySFX("UIClick");
            int levelToLoad = GetContinueLevelIndex();

            if (LevelManager.Instance != null)
                LevelManager.Instance.LoadLevel(levelToLoad);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(levelToLoad);

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance.PlaySFX("UIClick");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private int GetContinueLevelIndex()
        {
            int saved = firstLevelIndex; // TODO: 接入存档后替换
            if (saved < firstLevelIndex) saved = firstLevelIndex;
            return saved;
        }
    }
}
