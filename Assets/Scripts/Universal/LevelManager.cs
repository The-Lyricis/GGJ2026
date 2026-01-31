using UnityEngine;
using UnityEngine.SceneManagement;

namespace GGJ2026
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        void Awake() {
            if (Instance == null) Instance = this;
        }

        // 加载下一关
        public void LoadNextLevel() {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            // 检查是否还有下一关（Build Settings里的总数）
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings) {
                SceneManager.LoadScene(nextSceneIndex);
            } else {
                Debug.Log("已经是最后一关了！返回主菜单。");
                LoadMainMenu();
            }
        }

        // 加载特定关卡（用于关卡选择界面）
        public void LoadLevel(int levelIndex) {
            // 假设索引0是主菜单，1是第一关
            SceneManager.LoadScene(levelIndex);
            UIManager.Instance.SetResetHintVisible(true);
            UIManager.Instance.SetResetHintText("Press R to Reset");
        }

        // 重置当前关卡
        public void RestartCurrentLevel() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu() {
            SceneManager.LoadScene(0); // 假设主菜单在 Build Settings 的第0位
        }
    }
}