using System;
using UnityEngine;
using Houkimajo.Bullets;
using Houkimajo.Systems;

namespace Houkimajo.Boss
{
    /// <summary>
    /// Máu của boss. Đạn player gọi TakeDamage() qua interface IDamageable khi bắn trúng.
    /// </summary>
    public class BossHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHP = 1000;

        public int MaxHP => maxHP;
        public int CurrentHP { get; private set; }

        public event Action<int, int> OnHPChanged; // (current, max)
        public event Action OnDefeated;

        public bool IsAlive => CurrentHP > 0;
        private HitFlash hitFlash;

        private void Awake()
        {
            CurrentHP = maxHP;
            hitFlash = GetComponent<HitFlash>();
        }

        private void OnEnable()
        {
            // Đăng ký làm mục tiêu cho skill tự tìm địch (tên lửa, sét...)
            TargetRegistry.Register(this, transform);
        }

        private void OnDisable()
        {
            TargetRegistry.Unregister(this);
        }

        /// <summary>Đặt lại máu theo cấu hình của màn (mỗi màn boss trâu khác nhau).</summary>
        public void ResetHealth(int newMaxHP)
        {
            maxHP = Mathf.Max(1, newMaxHP);
            CurrentHP = maxHP;
            OnHPChanged?.Invoke(CurrentHP, maxHP);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHP <= 0) return;

            if (amount > 0) hitFlash?.Flash();
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            OnHPChanged?.Invoke(CurrentHP, maxHP);

            if (CurrentHP <= 0)
            {
                TargetRegistry.Unregister(this);
                OnDefeated?.Invoke();
            }
        }
    }
}
