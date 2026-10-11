using UnityEngine;
using UnityEngine.UI;
using Houkimajo.Player;
using Houkimajo.Boss;
using Houkimajo.Systems;

namespace Houkimajo.UI
{
    /// <summary>
    /// Hiện chữ to giữa màn hình khi Player chết (Game Over) hoặc Boss hết HP (Victory),
    /// đồng thời dừng game lại (timeScale = 0). Dùng tạm cho playtest — thay bằng
    /// màn hình kết thúc đẹp hơn khi có art thật.
    /// </summary>
    public class EndStateUI : MonoBehaviour
    {
        [SerializeField] private Text endStateText;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private BossHealth bossHealth;

        private void OnEnable()
        {
            if (playerHealth != null) playerHealth.OnPlayerDied += HandleGameOver;
            if (bossHealth != null) bossHealth.OnDefeated += HandleVictory;
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnPlayerDied -= HandleGameOver;
            if (bossHealth != null) bossHealth.OnDefeated -= HandleVictory;
        }

        private void HandleGameOver()
        {
            ShowText("GAME OVER");
            GameManager.Instance?.SetState(GameState.GameOver);
        }

        private void HandleVictory()
        {
            ShowText("VICTORY");
            GameManager.Instance?.SetState(GameState.GameOver); // chưa có state "Win" riêng, dùng tạm GameOver để dừng game
        }

        private void ShowText(string message)
        {
            if (endStateText == null) return;
            endStateText.text = message;
            endStateText.gameObject.SetActive(true);
        }
    }
}
