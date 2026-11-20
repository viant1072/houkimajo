using System;
using UnityEngine;

namespace Houkimajo.Player
{
    /// <summary>MP riêng của player, dùng delta có scale để hồi dừng cùng gameplay.</summary>
    public class PlayerMana : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxMP = 100f;
        [SerializeField, Min(0f), Tooltip("MP hồi mỗi giây gameplay")]
        private float regenerationPerSecond = 5f;
        private float currentMP;
        private PlayerHealth health;
        public float CurrentMP => currentMP;
        public float MaxMP => maxMP;
        public event Action<float, float> OnMPChanged;

        private void Awake()
        {
            maxMP = Mathf.Max(1f, maxMP);
            currentMP = maxMP;
            health = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f || currentMP >= maxMP || (health != null && health.CurrentHP <= 0)) return;
            currentMP = Mathf.Min(maxMP, currentMP + Mathf.Max(0f, regenerationPerSecond) * Time.deltaTime);
            OnMPChanged?.Invoke(currentMP, maxMP);
        }

        public bool TrySpend(float amount)
        {
            amount = Mathf.Max(0f, amount);
            if (currentMP < amount) return false;
            if (amount <= 0f) return true;
            currentMP -= amount;
            OnMPChanged?.Invoke(currentMP, maxMP);
            return true;
        }
    }
}
