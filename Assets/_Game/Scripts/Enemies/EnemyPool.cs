using System.Collections.Generic;
using UnityEngine;

namespace Houkimajo.Enemies
{
    /// <summary>
    /// Pool quái thường — cùng nguyên tắc BulletPool: tạo sẵn lúc vào màn, không
    /// Instantiate/Destroy giữa trận. 1 prefab chung, hình dạng/chỉ số nạp từ EnemyDataSO.
    /// </summary>
    public class EnemyPool : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab;

        [Tooltip("Số quái tạo sẵn — nên >= giới hạn quái cùng lúc của EnemySpawner")]
        [SerializeField] private int initialSize = 40;

        private readonly Queue<EnemyController> pool = new Queue<EnemyController>();
        private readonly List<EnemyController> active = new List<EnemyController>();

        public int ActiveCount => active.Count;

        private void Awake()
        {
            if (enemyPrefab == null) return;
            for (int i = 0; i < initialSize; i++)
            {
                var enemy = Instantiate(enemyPrefab, transform);
                enemy.gameObject.SetActive(false);
                pool.Enqueue(enemy);
            }
        }

        /// <summary>Lấy 1 quái ra, trả null nếu pool đã cạn (spawner sẽ thử lại sau).</summary>
        public EnemyController Get(Vector2 position)
        {
            if (pool.Count == 0) return null;

            var enemy = pool.Dequeue();
            enemy.transform.position = position;
            enemy.PoolIndex = active.Count;
            active.Add(enemy);
            enemy.gameObject.SetActive(true);
            return enemy;
        }

        public void Return(EnemyController enemy)
        {
            if (enemy.PoolIndex < 0) return; // chặn trả 2 lần

            int index = enemy.PoolIndex;
            int lastIndex = active.Count - 1;
            var last = active[lastIndex];
            active[index] = last;
            last.PoolIndex = index;
            active.RemoveAt(lastIndex);

            enemy.PoolIndex = -1;
            enemy.gameObject.SetActive(false);
            pool.Enqueue(enemy);
        }

        /// <summary>Thu hồi mọi quái đang hoạt động (kết thúc màn).</summary>
        public void DespawnAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (i < active.Count) active[i].Despawn();
            }
        }
    }
}
