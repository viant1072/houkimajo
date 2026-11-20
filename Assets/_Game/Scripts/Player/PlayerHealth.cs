using System;
using UnityEngine;

namespace Houkimajo.Player
{
    /// <summary>
    /// Quản lý HP của player và thời gian bất tử. Có 2 loại bất tử:
    /// - sau khi trúng đạn (nhấp nháy, tránh mất nhiều mạng liên tiếp trong 1 cú trúng)
    /// - do skill cấp (lướt, dịch chuyển...) — không nhấp nháy, để người chơi không
    ///   tưởng nhầm là mình vừa bị trúng.
    /// Dùng mốc thời gian thay vì coroutine để 2 nguồn bất tử chồng lên nhau đơn giản.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHP = 100;
        [SerializeField, Min(1), Tooltip("HP mất mỗi hit đạn hoặc chạm quái")]
        private int hitDamage = 20;
        private int currentHP;

        [Tooltip("Thời gian bất tử sau khi trúng đạn (giây)")]
        [SerializeField] private float invincibilityDuration = 2f;

        public event Action<int, int> OnHPChanged;
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

        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;

        private void Awake()
        {
            maxHP = Mathf.Max(1, maxHP);
            currentHP = maxHP;
        }

        public void TakeHit()
        {
            if (IsInvincible || currentHP <= 0) return;
            if (ShieldHits > 0)
            {
                shieldHits--;
                GrantInvincibility(shieldInvincibility);
                OnShieldBlocked?.Invoke(shieldHits == 0);
                return;
            }

            currentHP = Mathf.Max(0, currentHP - Mathf.Max(1, hitDamage));
            OnHPChanged?.Invoke(currentHP, maxHP);

            if (currentHP <= 0)
            {
                OnPlayerDied?.Invoke();
            }
            else
            {
                hitInvincibleUntil = Time.time + invincibilityDuration;
            }
        }

        /// <summary>Hồi HP cho nhân vật còn sống; trả lượng hồi thực tế sau khi kẹp max.</summary>
        public int Heal(int amount)
        {
            if (currentHP <= 0 || amount <= 0) return 0;
            int restored = Mathf.Min(amount, maxHP - currentHP);
            if (restored <= 0) return 0;
            currentHP += restored;
            OnHPChanged?.Invoke(currentHP, maxHP);
            return restored;
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
