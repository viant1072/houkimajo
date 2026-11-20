using System;
using UnityEngine;

namespace Houkimajo.Bullets
{
    /// <summary>Sát thương đến từ đâu — để năng lượng ultimate không tự tích từ chính ultimate.</summary>
    public enum DamageSource
    {
        BasicShot,
        Skill,
        Ultimate
    }

    /// <summary>
    /// Điểm chung để gây sát thương từ phía player. Đạn thường và skill đều đi qua đây,
    /// nên các hệ thống nghe sát thương (điểm, năng lượng ultimate...) chỉ đăng ký 1 chỗ,
    /// không phải biết sát thương tới từ viên đạn hay từ skill nào.
    /// </summary>
    public static class DamageEvents
    {
        /// <summary>(sát thương, nguồn) — phát mỗi khi player gây sát thương.</summary>
        public static event Action<int, DamageSource> OnPlayerDamageDealt;
        public static event Action<Vector2, DamageSource> OnPlayerHit;

        public static void DealPlayerDamage(IDamageable target, int amount, DamageSource source, Vector2? hitPosition = null)
        {
            if (target == null || !target.IsAlive || amount <= 0) return;

            var component = target as Component;
            bool hasPosition = hitPosition.HasValue || component != null;
            Vector2 position = hitPosition ?? (component != null ? (Vector2)component.transform.position : Vector2.zero);
            target.TakeDamage(amount);
            OnPlayerDamageDealt?.Invoke(amount, source);
            if (hasPosition) OnPlayerHit?.Invoke(position, source);
        }
    }
}
