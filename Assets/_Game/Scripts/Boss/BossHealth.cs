using System;
using UnityEngine;

namespace Houkimajo.Boss
{
    /// <summary>
    /// Máu của boss. BulletBase gọi TakeDamage() khi đạn player trúng.
    /// </summary>
    public class BossHealth : MonoBehaviour
    {
        [SerializeField] private int maxHP = 1000;

        public int MaxHP => maxHP;
        public int CurrentHP { get; private set; }

        public event Action<int, int> OnHPChanged; // (current, max)
        public event Action OnDefeated;

        private void Awake()
        {
            CurrentHP = maxHP;
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHP <= 0) return;

            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            OnHPChanged?.Invoke(CurrentHP, maxHP);

            if (CurrentHP <= 0)
            {
                OnDefeated?.Invoke();
            }
        }
    }
}
