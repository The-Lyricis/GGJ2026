using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public enum UILayer
{
    Panel,
    Popup,
    Overlay
}
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 配置")] [SerializeField] private UIDocument mainDocument;
    
    private Dictionary<string, VisualElement> _panelCache = new Dictionary<string, VisualElement>();
    
    private VisualElement _root;
    private VisualElement _panelLayer;
    private VisualElement _popupLayer;
    private VisualElement _overlayLayer;

    private void Awake()
    { 
        if (Instance == null)
        {
            Instance = this;
            _root = mainDocument.rootVisualElement;
            _root.style.position = Position.Absolute;
            _root.style.left = 0;
            _root.style.right = 0;
            _root.style.top = 0;
            _root.style.bottom = 0;
            _root.style.flexGrow = 1;
            
            _panelLayer = _root.Q<VisualElement>("PanelLayer");
            _popupLayer = _root.Q<VisualElement>("PopupLayer");
            _overlayLayer = _root.Q<VisualElement>("OverlayLayer");

        }
        else
        {
            Destroy(gameObject);
        }
    }

    public VisualElement OpenPanel(string assetPath, UILayer layer)
    {
        if (!_panelCache.TryGetValue(assetPath, out var panel))
        {
            var visualAsset = Resources.Load<VisualTreeAsset>(assetPath);//VisualTreeAsset是.uxml在内存中的对象格式
            if(visualAsset == null) return null;

            panel = visualAsset.Instantiate();
            panel.style.position = Position.Absolute;
            panel.style.left = 0;
            panel.style.right = 0;
            panel.style.top = 0;
            panel.style.bottom = 0;
            panel.style.flexGrow = 1;
            _panelCache.Add(assetPath, panel);
        }

        VisualElement ElementRoot = _panelLayer;
        switch (layer)
        {   
            case UILayer.Popup:
                ElementRoot = _popupLayer;
                break;
            case UILayer.Panel:
                ElementRoot = _panelLayer;
                break;
            case UILayer.Overlay:
                ElementRoot = _overlayLayer;
                break;
        }
        if (!ElementRoot.Contains(panel))
        {
            ElementRoot.Add(panel);
        }
        
        panel.style.display = DisplayStyle.Flex;
        return panel;
        
    }

    public void ClosePanel(string assetPath)
    {
        if (_panelCache.TryGetValue(assetPath, out var panel))
        {
            panel.style.display = DisplayStyle.None;
        }
    }
}

