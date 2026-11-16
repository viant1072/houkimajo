using System;
using UnityEngine;
using Houkimajo.Bullets;

namespace Houkimajo.Systems
{
    /// <summary>
    /// Theo dõi điểm số, phát event khi điểm thay đổi để UI (hoặc hệ thống khác) lắng nghe
    /// thay vì phải tham chiếu trực tiếp lẫn nhau.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public event Action<int> OnScoreChanged;

        public int Score { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            // Tạm thời: điểm = tổng sát thương gây ra, để dễ test. Nghe qua event để
            // đạn không phải biết tới ScoreManager.
            DamageEvents.OnPlayerDamageDealt += HandleDamageDealt;
        }

        private void OnDisable()
        {
            // Event static sống xuyên scene -> bắt buộc huỷ đăng ký, không thì giữ
            // reference tới object đã bị huỷ.
            DamageEvents.OnPlayerDamageDealt -= HandleDamageDealt;
        }

        private void HandleDamageDealt(int amount, DamageSource source)
        {
            AddScore(amount);
        }

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void ResetScore()
        {
            Score = 0;
            OnScoreChanged?.Invoke(Score);
        }
    }
}
