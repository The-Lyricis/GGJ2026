using UnityEngine;
using GGJ2026;
using UnityEngine.UIElements;


public class PauseButtonPanel : BasePanel
{
    private Button _PauseBtn;
    protected override void OnBindElements()
    {
        _PauseBtn = rootElement.Q<Button>("PauseButton");
        _PauseBtn.clicked += ()=>
        {
            GameManager.Instance.SetState(GameState.Paused);
        };
    }
}
