using System;
using UnityEngine;

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
