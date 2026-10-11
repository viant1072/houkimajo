using UnityEngine;
using UnityEngine.UI;
using Houkimajo.Systems;
using Houkimajo.Player;
using Houkimajo.Boss;

namespace Houkimajo.UI
{
    /// <summary>
    /// Hiển thị điểm số, mạng và HP boss lên UI, lắng nghe event thay vì Update() mỗi frame.
    /// Dùng tạm cho mục đích debug/playtest — thay bằng UI đẹp hơn khi có art thật.
    /// Kéo Text (hoặc TMP_Text, đổi kiểu field nếu dùng TextMeshPro) vào Inspector.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text livesText;
        [SerializeField] private Text bossHpText;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private BossHealth bossHealth;

        private void OnEnable()
        {
            // Chỉ đăng ký lắng nghe ở đây — KHÔNG đọc giá trị hiện tại (CurrentHP, Lives...)
            // vì Unity không đảm bảo Awake() của BossHealth/PlayerHealth đã chạy xong
            // trước OnEnable() của HUDController (object khác). Đọc sớm có thể ra giá
            // trị mặc định (0) nếu Boss khởi tạo sau HUDController.
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged += UpdateScoreText;

            if (playerHealth != null)
                playerHealth.OnLivesChanged += UpdateLivesText;

            if (bossHealth != null)
                bossHealth.OnHPChanged += UpdateBossHpText;
        }

        private void Start()
        {
            // Start() chạy sau khi TOÀN BỘ object trong scene đã Awake() xong — an toàn
            // để đọc giá trị ban đầu ở đây.
            if (playerHealth != null)
                UpdateLivesText(playerHealth.Lives);

            if (bossHealth != null)
                UpdateBossHpText(bossHealth.CurrentHP, bossHealth.MaxHP);
        }

        private void OnDisable()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= UpdateScoreText;

            if (playerHealth != null)
                playerHealth.OnLivesChanged -= UpdateLivesText;

            if (bossHealth != null)
                bossHealth.OnHPChanged -= UpdateBossHpText;
        }

        private void UpdateScoreText(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }

        private void UpdateLivesText(int lives)
        {
            if (livesText != null)
                livesText.text = $"Lives: {lives}";
        }

        private void UpdateBossHpText(int current, int max)
        {
            if (bossHpText != null)
                bossHpText.text = $"Boss HP: {current}/{max}";
        }
    }
}
