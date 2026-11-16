using System;

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

        public static void DealPlayerDamage(IDamageable target, int amount, DamageSource source)
        {
            if (target == null || !target.IsAlive || amount <= 0) return;

            target.TakeDamage(amount);
            OnPlayerDamageDealt?.Invoke(amount, source);
        }
    }
}
