using GGJ2026;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuPanel : BasePanel
{
    private Button _startBtn;
    private Button _continueBtn;
    private Button _quitBtn;

    protected override void OnBindElements()
    {
        _startBtn = rootElement.Q<Button>("StartButton");
        _quitBtn = rootElement.Q<Button>("QuitButton");
        
        _startBtn.clicked += ()=>
        {
            LevelManager.Instance.LoadNextLevel();
            GameManager.Instance.SetState(GameState.Playing);
        };
        _quitBtn.clicked += ()=>Application.Quit();
    }
}
