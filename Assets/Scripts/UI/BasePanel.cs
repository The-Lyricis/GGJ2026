using GGJ2026;
using UnityEngine;
using UnityEngine.UIElements;

public class BasePanel : MonoBehaviour
{
    [SerializeField] protected string uxmlPath;
    [SerializeField] private UILayer uiLayer = UILayer.Panel;// 对应的uxml资源路径
    protected VisualElement rootElement;

    public virtual void Show()
    {
        Debug.Log("Show Panel: " + uxmlPath);
        rootElement = UIManager.Instance.OpenPanel(uxmlPath, uiLayer);
        OnBindElements();
    }

    public virtual void Hide()
    {
        UIManager.Instance.ClosePanel(uxmlPath);
    }

    protected virtual void OnBindElements(){}
    
}
