using System;
using UnityEngine;

namespace Houkimajo.Player
{
    /// <summary>
    /// Quản lý mạng (lives) của player và thời gian bất tử. Có 2 loại bất tử:
    /// - sau khi trúng đạn (nhấp nháy, tránh mất nhiều mạng liên tiếp trong 1 cú trúng)
    /// - do skill cấp (lướt, dịch chuyển...) — không nhấp nháy, để người chơi không
    ///   tưởng nhầm là mình vừa bị trúng.
    /// Dùng mốc thời gian thay vì coroutine để 2 nguồn bất tử chồng lên nhau đơn giản.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int lives = 3;

        [Tooltip("Thời gian bất tử sau khi trúng đạn (giây)")]
        [SerializeField] private float invincibilityDuration = 2f;

        public event Action<int> OnLivesChanged;
        public event Action OnPlayerDied;
        public event Action<bool> OnShieldBlocked;
        private int shieldHits;
        private float shieldUntil = -1f;
        private float shieldInvincibility = 0.5f;
        public int ShieldHits => Time.time < shieldUntil ? shieldHits : 0;

        // Time.time có scale: pause/shop thì đồng hồ bất tử cũng đứng yên.
        private float hitInvincibleUntil = -1f;
        private float skillInvincibleUntil = -1f;

        public bool IsInvincible => Time.time < hitInvincibleUntil || Time.time < skillInvincibleUntil;

        /// <summary>Đang trong thời gian bất tử sau khi trúng đạn (dùng để nhấp nháy).</summary>
        public bool IsRecoveringFromHit => Time.time < hitInvincibleUntil;

        public int Lives => lives;

        public void TakeHit()
        {
            if (IsInvincible || lives <= 0) return;
            if (ShieldHits > 0)
            {
                shieldHits--;
                GrantInvincibility(shieldInvincibility);
                OnShieldBlocked?.Invoke(shieldHits == 0);
                return;
            }

            lives--;
            OnLivesChanged?.Invoke(lives);

            if (lives <= 0)
            {
                OnPlayerDied?.Invoke();
            }
            else
            {
                hitInvincibleUntil = Time.time + invincibilityDuration;
            }
        }

        /// <summary>Bất tử thêm <paramref name="seconds"/> giây (skill). Không rút ngắn bất tử đang có.</summary>
        public void GrantInvincibility(float seconds)
        {
            skillInvincibleUntil = Mathf.Max(skillInvincibleUntil, Time.time + seconds);
        }

        public void AddShield(int hits, float duration, float invincibility = 0.5f)
        {
            shieldHits = Mathf.Max(1, hits);
            shieldUntil = Time.time + duration;
            shieldInvincibility = invincibility;
        }

        public void ClearShield() { shieldHits = 0; shieldUntil = -1f; }
    }
}
