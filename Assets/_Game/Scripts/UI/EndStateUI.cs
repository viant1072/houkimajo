using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Houkimajo.Systems;

namespace Houkimajo.UI
{
    /// <summary>
    /// Màn kết thúc: thắng màn (tự chuyển, do StageDirector lo), phá đảo, hoặc Game Over
    /// (chờ người chơi bấm Enter = chơi lại, Esc = về menu). Dùng phím tắt thay cho nút để
    /// scene gameplay không cần EventSystem.
    /// </summary>
    public class EndStateUI : MonoBehaviour
    {
        [SerializeField] private StageDirector director;
        [SerializeField] private Text endStateText;

        [Tooltip("Dòng nhỏ dưới chữ to: hướng dẫn phím / thông báo chuyển màn")]
        [SerializeField] private Text hintText;

        private bool waitingForRetryInput;

        private void OnEnable()
        {
            if (director == null) return;
            director.OnStageCleared += HandleStageCleared;
            director.OnStageFailed += HandleStageFailed;
        }

        private void OnDisable()
        {
            if (director == null) return;
            director.OnStageCleared -= HandleStageCleared;
            director.OnStageFailed -= HandleStageFailed;
        }

        private void Update()
        {
            if (!waitingForRetryInput) return;

            // timeScale = 0 lúc Game Over nhưng Update vẫn chạy, đọc phím bình thường.
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                waitingForRetryInput = false;
                director.RetryStage();
            }
            else if (keyboard.escapeKey.wasPressedThisFrame)
            {
                waitingForRetryInput = false;
                SceneFlow.LoadMainMenu();
            }
        }

        private void HandleStageCleared(bool hasNextStage)
        {
            if (hasNextStage)
                Show($"HOÀN THÀNH MÀN {director.StageIndex + 1}", "Sang màn kế tiếp...");
            else
                Show("PHÁ ĐẢO!", "Về menu chính...");
        }

        private void HandleStageFailed()
        {
            Show("GAME OVER", "Enter: chơi lại màn   ·   Esc: về menu");
            waitingForRetryInput = true;
        }

        private void Show(string title, string hint)
        {
            if (endStateText != null)
            {
                endStateText.text = title;
                endStateText.gameObject.SetActive(true);
            }

            if (hintText != null)
            {
                hintText.text = hint;
                hintText.gameObject.SetActive(true);
            }
        }
    }
}
