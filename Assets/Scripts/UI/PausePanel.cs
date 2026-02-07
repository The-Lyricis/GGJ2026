using GGJ2026;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class PausePanel : BasePanel
{
    [Header("Pause Logic")]
    [SerializeField] private bool pauseByTimeScale = true;

    [Header("Audio (Optional)")]
    [SerializeField] private string sfxVolumeParam = "SFXVolume";
    [SerializeField] private string bgmVolumeParam = "BGMVolume";

    [Header("Scene (Optional)")]
    [SerializeField] private string mainMenuSceneName = "InitScene";

    // UI refs
    private VisualElement overlay;

    private Slider sfxSlider;
    private Slider bgmSlider;
    private Label sfxValue;
    private Label bgmValue;

    private Button btnResume;
    private Button btnMainMenu;
    private Button btnQuit;

    private bool isOpen;

    public override void Show()
    {
        base.Show(); // OpenPanel + OnBindElements()

        // 默认打开时：显示并暂停
        SetOpen(true);
    }

    public override void Hide()
    {
        SetOpen(false);
        base.Hide();
    }

    /// <summary>外部可调用：切换暂停面板。</summary>
    public void Toggle()
    {
        SetOpen(!isOpen);
        if (isOpen) Show(); else Hide();
    }

    /// <summary>仅设置 UI 显示与 TimeScale，不负责 Open/Close 面板资源。</summary>
    public void SetOpen(bool open)
    {
        isOpen = open;

        if (overlay != null)
            overlay.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;

        if (pauseByTimeScale)
            Time.timeScale = open ? 0f : 1f;
    }

    protected override void OnBindElements()
    {
        if (rootElement == null)
        {
            Debug.LogError($"PausePanel: rootElement is null. uxmlPath={uxmlPath}");
            return;
        }

        // 你的 UXML 中 overlay 是 name="Overlay"
        overlay = rootElement.Q<VisualElement>("Overlay");
        if (overlay == null)
        {
            // 兜底：如果 OpenPanel 返回的 root 就是 Overlay，也允许直接使用 rootElement
            overlay = rootElement;
        }

        sfxSlider = rootElement.Q<Slider>("SfxSlider");
        bgmSlider = rootElement.Q<Slider>("BgmSlider");
        sfxValue  = rootElement.Q<Label>("SfxValue");
        bgmValue  = rootElement.Q<Label>("BgmValue");

        btnResume = rootElement.Q<Button>("BtnResume");
        btnMainMenu = rootElement.Q<Button>("BtnMainMenu");
        btnQuit     = rootElement.Q<Button>("BtnQuit");

        BindEvents();
        RefreshVolumeTexts();
    }

    private void BindEvents()
    {
        // Slider
        if (sfxSlider != null)
        {
            sfxSlider.RegisterValueChangedCallback(evt =>
            {
                UpdatePercentLabel(sfxValue, evt.newValue);
                AudioManager.Instance.SetSFXVolume(evt.newValue);
            });
        }

        if (bgmSlider != null)
        {
            bgmSlider.RegisterValueChangedCallback(evt =>
            {
                UpdatePercentLabel(bgmValue, evt.newValue);
                AudioManager.Instance.SetMusicVolume(evt.newValue);
            });
        }

        // Buttons
        
        if (btnResume != null)   
            btnResume.clicked += OnClickResume;
        
        if (btnMainMenu != null)
            btnMainMenu.clicked += OnClickMainMenu;

        if (btnQuit != null)
            btnQuit.clicked += OnClickQuit;
    }

    private void RefreshVolumeTexts()
    {
        if (sfxSlider != null) UpdatePercentLabel(sfxValue, sfxSlider.value);
        if (bgmSlider != null) UpdatePercentLabel(bgmValue, bgmSlider.value);
    }

    private static void UpdatePercentLabel(Label label, float linear01)
    {
        if (label == null) return;
        int p = Mathf.RoundToInt(Mathf.Clamp01(linear01) * 100f);
        label.text = $"{p}%";
    }



    private void OnClickResume()
    {
        GameManager.Instance.SetState(GameState.Playing);
    }
    
    private void OnClickMainMenu()
    {
        // 先恢复时间再切场景
        if (pauseByTimeScale) Time.timeScale = 1f;

        GameManager.Instance.SetState(GameState.MainMenu);
    }

    private void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDisable()
    {
        // 防止重复绑定导致多次触发（尤其反复 Show/Hide）
        if (btnMainMenu != null) btnMainMenu.clicked -= OnClickMainMenu;
        if (btnQuit != null) btnQuit.clicked -= OnClickQuit;
    }
}
