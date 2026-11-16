using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Houkimajo.Systems;

namespace Houkimajo.UI
{
    /// <summary>
    /// Menu chính: Chơi tiếp (vào màn sau màn cao nhất đã qua) và Chơi mới (xoá save, vào
    /// màn 1). Enter = Chơi tiếp, không có save thì Chơi mới.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private StageCatalogSO catalog;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text continueLabel;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Text newGameLabel;
        [SerializeField] private Text progressText;

        private bool hasProgress;
        private int continueStageIndex;
        private bool loading;

        private void Start()
        {
            Cursor.visible = true;
            Time.timeScale = 1f;

            var save = SaveSystem.Load();
            int stageCount = catalog != null ? catalog.Count : 1;
            hasProgress = SaveSystem.HasSave;
            // Phá đảo rồi thì "Chơi tiếp" vào lại màn cuối.
            continueStageIndex = Mathf.Clamp(save.highestStageCleared + 1, 0, Mathf.Max(0, stageCount - 1));

            if (continueButton != null)
            {
                continueButton.interactable = hasProgress;
                continueButton.onClick.AddListener(Continue);
            }
            if (continueLabel != null)
                continueLabel.text = hasProgress ? $"Chơi tiếp — Màn {continueStageIndex + 1}" : "Chơi tiếp";

            if (newGameButton != null) newGameButton.onClick.AddListener(NewGame);
            if (newGameLabel != null)
                newGameLabel.text = hasProgress ? "Chơi mới (xoá tiến trình)" : "Chơi mới";

            if (progressText != null)
            {
                int cleared = Mathf.Min(save.highestStageCleared + 1, stageCount);
                progressText.text = hasProgress
                    ? $"Đã qua {cleared}/{stageCount} màn · Vàng {save.gold}"
                    : "Chưa có tiến trình";
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                if (hasProgress) Continue();
                else NewGame();
            }
        }

        public void Continue()
        {
            if (loading || !hasProgress) return;
            loading = true;
            SceneFlow.LoadStage(continueStageIndex);
        }

        public void NewGame()
        {
            if (loading) return;
            loading = true;
            SaveSystem.Delete();
            SceneFlow.LoadStage(0);
        }
    }
}
