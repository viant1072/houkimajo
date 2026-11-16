using System.Collections.Generic;
using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Quản lý pool cho mọi loại projectile skill. Mỗi prefab có 1 pool riêng, tạo khi
    /// skill Prewarm() lúc bắt đầu stage — nên không phải đặt tay từng pool vào scene
    /// mỗi khi thêm skill mới. Gắn lên 1 GameObject rỗng "SkillPools".
    /// </summary>
    public class SkillPools : MonoBehaviour
    {
        [Tooltip("Số projectile tạo sẵn cho mỗi loại")]
        [SerializeField] private int defaultPrewarmSize = 32;

        private readonly Dictionary<SkillProjectile, SkillProjectilePool> pools =
            new Dictionary<SkillProjectile, SkillProjectilePool>();

        /// <summary>Lấy (hoặc tạo nếu chưa có) pool cho prefab này.</summary>
        public SkillProjectilePool GetPool(SkillProjectile prefab, int prewarmSize = 0)
        {
            if (prefab == null) return null;
            if (pools.TryGetValue(prefab, out var existing)) return existing;

            var go = new GameObject($"Pool_{prefab.name}");
            go.transform.SetParent(transform, false);
            var pool = go.AddComponent<SkillProjectilePool>();
            pool.Initialize(prefab, Mathf.Max(defaultPrewarmSize, prewarmSize));
            pools.Add(prefab, pool);
            return pool;
        }

        public SkillVfxPool Effects { get; private set; }

        private void Awake()
        {
            Effects = GetComponent<SkillVfxPool>();
            if (Effects == null) Effects = gameObject.AddComponent<SkillVfxPool>();
        }
    }
}
