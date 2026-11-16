using System.Collections.Generic;
using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Pool cho 1 loại prefab SkillProjectile — cùng nguyên tắc với BulletPool: không
    /// Instantiate/Destroy lúc đang chơi. Do SkillPools tạo tự động cho từng prefab.
    /// </summary>
    public class SkillProjectilePool : MonoBehaviour
    {
        private SkillProjectile prefab;
        private readonly Queue<SkillProjectile> pool = new Queue<SkillProjectile>();
        private readonly List<SkillProjectile> active = new List<SkillProjectile>();

        public void Initialize(SkillProjectile projectilePrefab, int initialSize)
        {
            prefab = projectilePrefab;
            for (int i = 0; i < initialSize; i++)
            {
                var projectile = CreateNew();
                projectile.gameObject.SetActive(false);
                pool.Enqueue(projectile);
            }
        }

        private SkillProjectile CreateNew()
        {
            var projectile = Instantiate(prefab, transform);
            projectile.SetOwnerPool(this);
            return projectile;
        }

        public SkillProjectile Get(Vector2 position)
        {
            if (pool.Count == 0) return null;
            var projectile = pool.Dequeue();
            projectile.transform.position = position;
            projectile.PoolIndex = active.Count;
            active.Add(projectile);
            projectile.gameObject.SetActive(true);
            return projectile;
        }

        public void Return(SkillProjectile projectile)
        {
            // Chặn trả 2 lần trong cùng frame (giống BulletPool).
            if (projectile.PoolIndex < 0) return;

            int index = projectile.PoolIndex;
            int lastIndex = active.Count - 1;
            var last = active[lastIndex];
            active[index] = last;
            last.PoolIndex = index;
            active.RemoveAt(lastIndex);

            projectile.PoolIndex = -1;
            projectile.gameObject.SetActive(false);
            pool.Enqueue(projectile);
        }
    }
}
