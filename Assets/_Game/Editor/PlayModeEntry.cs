using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Cho nút Play đi qua menu chính như khi mở bản build.</summary>
    internal static class PlayModeEntry
    {
        private const string MenuPath = "Houkimajo/Start Play Mode From Main Menu";
        private const string PreferenceKey = "Houkimajo.StartPlayModeFromMainMenu";

        private static bool StartFromMainMenu => SessionState.GetBool(PreferenceKey, true);

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // Batch smoke test tự mở scene cần kiểm tra.
            if (!Application.isBatchMode)
            {
                EditorApplication.delayCall += ApplyStartScene;
            }
        }

        [MenuItem(MenuPath)]
        private static void ToggleStartScene()
        {
            SessionState.SetBool(PreferenceKey, !StartFromMainMenu);
            ApplyStartScene();
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateStartSceneToggle()
        {
            Menu.SetChecked(MenuPath, StartFromMainMenu);
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        private static void ApplyStartScene()
        {
            // Unity tự khôi phục các scene đang chỉnh khi thoát Play Mode.
            EditorSceneManager.playModeStartScene = StartFromMainMenu
                ? AssetDatabase.LoadAssetAtPath<SceneAsset>(StageBootstrapper.MainMenuPath)
                : null;
        }
    }
}
