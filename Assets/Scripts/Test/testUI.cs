using System;
using UnityEngine;
using UnityEngine.UIElements;

public class testUI : MonoBehaviour
{
    [SerializeField] private UIDocument mainDocument;

    private void Start()
    {
        
        var root = mainDocument.rootVisualElement;
        var visualAsset = Resources.Load<VisualTreeAsset>("MainMenu");
        var panel = visualAsset.Instantiate();
        
        panel.style.position = Position.Absolute;
        panel.style.left = 0;
        panel.style.right = 0;
        panel.style.top = 0;
        panel.style.bottom = 0;
        panel.style.flexGrow = 1;
        
        
        root.style.position = Position.Absolute;
        root.style.left = 0;
        root.style.right = 0;
        root.style.top = 0;
        root.style.bottom = 0;
        root.style.flexGrow = 1;

        root.Add(panel);
    }
}
