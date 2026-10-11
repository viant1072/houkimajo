using System.Collections.Generic;
using UnityEngine;

namespace Houkimajo.Bullets
{
    /// <summary>
    /// Object pool cho đạn — tái sử dụng GameObject thay vì Instantiate/Destroy liên tục.
    /// Bullet-hell bắn hàng trăm viên/giây, nếu tạo/xóa thật sẽ giật lag nặng.
    /// Gắn script này vào 1 GameObject rỗng (ví dụ "PlayerBulletPool" / "EnemyBulletPool"),
    /// kéo bulletPrefab vào Inspector.
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        [SerializeField] private BulletBase bulletPrefab;
        [SerializeField] private int initialSize = 100;

        private readonly Queue<BulletBase> pool = new Queue<BulletBase>();

        private void Awake()
        {
            for (int i = 0; i < initialSize; i++)
            {
                var bullet = CreateNew();
                bullet.gameObject.SetActive(false);
                pool.Enqueue(bullet);
            }
        }

        private BulletBase CreateNew()
        {
            var bullet = Instantiate(bulletPrefab, transform);
            bullet.SetOwnerPool(this);
            return bullet;
        }

        public BulletBase Get(Vector2 position)
        {
            BulletBase bullet = pool.Count > 0 ? pool.Dequeue() : CreateNew();
            bullet.transform.position = position;
            bullet.gameObject.SetActive(true);
            return bullet;
        }

        public void Return(BulletBase bullet)
        {
            bullet.gameObject.SetActive(false);
            pool.Enqueue(bullet);
        }
    }
}
