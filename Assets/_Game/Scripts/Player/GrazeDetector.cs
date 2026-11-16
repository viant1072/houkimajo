using System;
using UnityEngine;
using Houkimajo.Bullets;
using Houkimajo.Systems;

namespace Houkimajo.Player
{
    /// <summary>
    /// Phát hiện "graze" — đạn địch sượt qua sát người mà chưa trúng hitbox. Gắn lên
    /// 1 GameObject con của Player có CircleCollider2D (Is Trigger) lớn hơn hitbox.
    /// Graze thưởng điểm và (sau này) năng lượng ultimate, khuyến khích né sát.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class GrazeDetector : MonoBehaviour
    {
        [Tooltip("Điểm cộng cho mỗi viên đạn sượt qua")]
        [SerializeField] private int scorePerGraze = 10;

        /// <summary>Phát ra mỗi lần graze 1 viên đạn (mỗi viên chỉ tính 1 lần).</summary>
        public event Action OnGraze;

        public int GrazeCount { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<BulletBase>(out var bullet)) return;
            if (bullet.IsPlayerBullet || bullet.HasBeenGrazed) return;

            bullet.MarkGrazed();
            GrazeCount++;
            ScoreManager.Instance?.AddScore(scorePerGraze);
            OnGraze?.Invoke();
        }
    }
}
