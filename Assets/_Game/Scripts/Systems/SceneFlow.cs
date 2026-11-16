using UnityEngine;
using UnityEngine.SceneManagement;

namespace Houkimajo.Systems
{
    /// <summary>
    /// Chuyển scene ở 1 chỗ duy nhất, để luôn nhớ trả timeScale về 1 (màn trước có thể
    /// đang dừng ở Game Over / Victory) và không rải tên scene khắp code.
    /// </summary>
    public static class SceneFlow
    {
        public const string MainMenuScene = "MainMenu";
        public const string GameplayScene = "Stage01";

        public static void LoadMainMenu()
        {
            Time.timeScale = 1f;
            // Chưa chạy menu Setup thì MainMenu chưa có trong Build Settings — về màn 1 thay vì
            // văng lỗi và kẹt ở màn hình Game Over.
            if (!Application.CanStreamedLevelBeLoaded(MainMenuScene))
            {
                Debug.LogWarning("[Houkimajo] Scene MainMenu chưa có trong Build Settings — chạy menu " +
                    "Houkimajo/Setup Stages + Main Menu. Tạm quay về màn 1.");
                LoadStage(0);
                return;
            }
            SceneManager.LoadScene(MainMenuScene);
        }

        public static void LoadStage(int stageIndex)
        {
            GameSession.CurrentStageIndex = Mathf.Max(0, stageIndex);
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameplayScene);
        }
    }
}
