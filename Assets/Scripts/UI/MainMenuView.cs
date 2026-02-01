using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GGJ2026
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuView : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite backgroundSprite; // 全屏背景
        [SerializeField] private Sprite heroSprite;       // 中轴线图片

        [Header("Font (Resources)")]
        [Tooltip("Assets/Resources/Fonts/Pixel.ttf -> set path as \"Fonts/Pixel\" (no extension).")]
        [SerializeField] private string pixelFontResourcePath = "Fonts/Pixel";

        [Header("Scene Indices (Build Settings)")]
        [SerializeField] private int firstLevelIndex = 1;

        [Header("Hotkeys")]
        [SerializeField] private bool enterToStart = true;

        private VisualElement _root;
        private VisualElement _bg;
        private VisualElement _hero;
        private Button _btnStart;
        private Button _btnExit;

        private void Awake()
        {
            var doc = GetComponent<UIDocument>();
            _root = doc.rootVisualElement;

            _bg = _root.Q<VisualElement>("bg");
            _hero = _root.Q<VisualElement>("hero");
            _btnStart = _root.Q<Button>("btnStart");
            _btnExit = _root.Q<Button>("btnExit");

            if (_bg != null && backgroundSprite != null)
                _bg.style.backgroundImage = new StyleBackground(backgroundSprite);

            if (_hero != null && heroSprite != null)
                _hero.style.backgroundImage = new StyleBackground(heroSprite);

            ApplyPixelFont();

            if (_btnStart != null) _btnStart.clicked += OnStartClicked;
            if (_btnExit != null) _btnExit.clicked += OnExitClicked;
        }

        private void OnDestroy()
        {
            if (_btnStart != null) _btnStart.clicked -= OnStartClicked;
            if (_btnExit != null) _btnExit.clicked -= OnExitClicked;
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

        private void ApplyPixelFont()
        {
            var pixelFont = Resources.Load<Font>(pixelFontResourcePath);
            if (pixelFont == null)
            {
                Debug.LogError($"[MainMenuView] Pixel font not found at Resources/{pixelFontResourcePath}. " +
                               $"Expected: Assets/Resources/{pixelFontResourcePath}.ttf");
                return;
            }

            // 旧版兼容：递归强制应用到所有 TextElement（包括 Button 内部的文本）
            ApplyPixelFontToAllText(_root, pixelFont);
        }

        private static void ApplyPixelFontToAllText(VisualElement ve, Font font)
        {
            if (ve == null || font == null) return;

            // TextElement 覆盖字体（Label、Button 内部文本等都属于 TextElement）
            if (ve is TextElement te)
            {
                te.style.unityFont = font;
            }

            // 递归子节点
            foreach (var child in ve.Children())
            {
                ApplyPixelFontToAllText(child, font);
            }
        }

        private void OnStartClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UIClick");

            if (LevelManager.Instance != null)
                LevelManager.Instance.LoadLevel(firstLevelIndex);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(firstLevelIndex);

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);
        }

        private void OnExitClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UIClick");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
