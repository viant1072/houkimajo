using UnityEngine;
using Houkimajo.Boss;
using Houkimajo.Player;
using Houkimajo.Systems;

namespace Houkimajo.Bullets
{
    /// <summary>
    /// Hành vi của 1 viên đạn — dùng chung cho đạn của player lẫn đạn của enemy/boss.
    /// Tự trả về pool khi bay ra khỏi màn hình hoặc va chạm, thay vì Destroy (xem BulletPool).
    /// Prefab cần có Collider2D (Is Trigger = true) và Rigidbody2D (Body Type = Kinematic)
    /// để OnTriggerEnter2D hoạt động.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BulletBase : MonoBehaviour
    {
        [SerializeField] private int damage = 10;
        [SerializeField] private Vector2 screenHalfSize = new Vector2(9f, 6f);

        private Vector2 direction;
        private float speed;
        private bool isPlayerBullet;
        private BulletPool ownerPool;

        public void Launch(Vector2 dir, float spd, bool playerOwned = false)
        {
            direction = dir.normalized;
            speed = spd;
            isPlayerBullet = playerOwned;
        }

        public void SetOwnerPool(BulletPool pool)
        {
            ownerPool = pool;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            if (IsOutOfBounds())
            {
                ReturnToPool();
            }
        }

        private bool IsOutOfBounds()
        {
            Vector2 pos = transform.position;
            return Mathf.Abs(pos.x) > screenHalfSize.x || Mathf.Abs(pos.y) > screenHalfSize.y;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPlayerBullet)
            {
                var bossHealth = other.GetComponent<BossHealth>();
                if (bossHealth != null)
                {
                    bossHealth.TakeDamage(damage);
                    ScoreManager.Instance?.AddScore(damage); // tạm: điểm = sát thương gây ra, để dễ test
                    ReturnToPool();
                }
            }
            else
            {
                var playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeHit();
                    ReturnToPool();
                }
            }
        }

        private void ReturnToPool()
        {
            if (ownerPool != null)
                ownerPool.Return(this);
            else
                Destroy(gameObject);
        }
    }
}
