using System;
using System.Collections;
using UnityEngine;

namespace Houkimajo.Player
{
    /// <summary>
    /// Quản lý mạng (lives) của player và thời gian bất tử ngắn ngay sau khi trúng đạn,
    /// để tránh mất nhiều mạng liên tiếp trong 1 cú trúng.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int lives = 3;
        [SerializeField] private float invincibilityDuration = 2f;

        public event Action<int> OnLivesChanged;
        public event Action OnPlayerDied;

        public bool IsInvincible { get; private set; }
        public int Lives => lives;

        public void TakeHit()
        {
            if (IsInvincible) return;

            lives--;
            OnLivesChanged?.Invoke(lives);

            if (lives <= 0)
            {
                OnPlayerDied?.Invoke();
            }
            else
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }

        private IEnumerator InvincibilityRoutine()
        {
            IsInvincible = true;
            yield return new WaitForSeconds(invincibilityDuration);
            IsInvincible = false;
        }
    }
}
