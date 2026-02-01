using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GGJ2026
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        [Header("Transition")]
        [SerializeField] private float fadeIn = 0.6f;
        [SerializeField] private float holdBlack = 0.1f;
        [SerializeField] private float fadeOut = 0.6f;

        private Coroutine loading;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void LoadNextLevel()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
                LoadSceneWithFade(nextSceneIndex);
            else
                LoadMainMenu();
        }

        public void LoadLevel(int levelIndex) => LoadSceneWithFade(levelIndex);

        public void RestartCurrentLevel() => LoadSceneWithFade(SceneManager.GetActiveScene().buildIndex);

        public void LoadMainMenu() => LoadSceneWithFade(0);

        private void LoadSceneWithFade(int buildIndex)
        {
            if (loading != null) StopCoroutine(loading);
            loading = StartCoroutine(CoLoadSceneWithFade(buildIndex));
        }

        private IEnumerator CoLoadSceneWithFade(int buildIndex)
        {
            // 0) 取到 fader（确保它存在）
            //    你之前的 UIManager 是 Prefab View 方式：这里依赖 UIManager 的 GetOrCreateScreenFader()
            //    为了不破坏封装，我们只通过 UIManager 暴露的接口获取/使用（见下文 UIManager 需新增一个 Getter）
            var fader = UIManager.Instance != null ? UIManager.Instance.GetScreenFaderView() : null;

            // 1) 先淡入到黑，并等待淡入完成（关键：保证“全黑时才加载”）
            if (fader != null)
                yield return fader.FadeToAndWait(1f, fadeIn, blockInputDuringFade: true);
            else
                yield return null;

            // ✅ 这里已经是全黑：你要做的 UI 操作放这里最安全
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMainMenu(false);

            // 2) 全黑保持一小段（可选）
            if (holdBlack > 0f) yield return new WaitForSeconds(holdBlack);

            // 3) 全黑期间开始加载场景（异步）
            var op = SceneManager.LoadSceneAsync(buildIndex);
            if (op == null) yield break;

            // 如果你想“加载完再激活”，可启用以下模式（更可控）
            // op.allowSceneActivation = false;

            while (!op.isDone)
            {
                // if (!op.allowSceneActivation && op.progress >= 0.9f) op.allowSceneActivation = true;
                yield return null;
            }

            // 4) 场景加载完成后，设置对应 UI（如 reset hint）
            if (UIManager.Instance != null)
            {
                bool isMainMenu = (buildIndex == 0);

                UIManager.Instance.SetResetHintVisible(!isMainMenu);
                if (!isMainMenu)
                    UIManager.Instance.SetResetHintText("Press R to Reset");

                // 如回到主菜单希望显示菜单：可打开下面一行
                // UIManager.Instance.ShowMainMenu(isMainMenu);
            }

            // 5) 淡出回到画面，并等待淡出完成
            if (fader != null)
                yield return fader.FadeToAndWait(0f, fadeOut, blockInputDuringFade: true);

            loading = null;
        }
    }
}
