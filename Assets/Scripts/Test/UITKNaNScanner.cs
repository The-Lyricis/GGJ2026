using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UITKNaNScanner : MonoBehaviour
{
    [SerializeField] private UIDocument doc;

    private void OnEnable()
    {
        var root = doc.rootVisualElement;

        // 等一帧，让面板完成一次更新
        root.schedule.Execute(() =>
        {
            var hits = new List<string>();
            Scan(root, "root", hits);

            if (hits.Count == 0)
                Debug.Log("[NaNScanner] No NaN found in resolvedStyle.");
            else
                Debug.Log("[NaNScanner] Found NaN:\n" + string.Join("\n", hits));
            
            
            
        }).ExecuteLater(0);
    }

    private void Scan(VisualElement ve, string path, List<string> hits)
    {
        var rs = ve.resolvedStyle;

        if (float.IsNaN(rs.width) || float.IsNaN(rs.height) ||
            float.IsNaN(rs.left) || float.IsNaN(rs.top) ||
            float.IsNaN(rs.right) || float.IsNaN(rs.bottom))
        {
            hits.Add($"{path}  (name={ve.name}, class={string.Join(",", ve.GetClasses())})  " +
                     $"w={rs.width}, h={rs.height}, l={rs.left}, t={rs.top}, r={rs.right}, b={rs.bottom}");
        }

        int i = 0;
        foreach (var child in ve.Children())
        {
            Scan(child, $"{path}/{child.GetType().Name}[{i}]", hits);
            i++;
        }
    }
}