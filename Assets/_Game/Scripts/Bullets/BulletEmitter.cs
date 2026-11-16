using UnityEngine;

namespace Houkimajo.Bullets
{
    /// <summary>
    /// Gắn vào enemy/boss để bắn pattern đạn theo cấu hình BulletPatternSO.
    /// Boss đổi pha -> gọi SetPattern() với asset pattern khác, không cần code riêng cho từng pha.
    /// </summary>
    public class BulletEmitter : MonoBehaviour
    {
        [SerializeField] private BulletPool bulletPool;
        [SerializeField] private BulletPatternSO pattern;

        private float timer;
        private float currentAngle;
        private Transform playerTransform;

        /// <summary>Gán pool lúc chạy — quái lấy từ pool nên không kéo reference qua Inspector được.</summary>
        public void SetPool(BulletPool pool)
        {
            bulletPool = pool;
        }

        public void SetPattern(BulletPatternSO newPattern)
        {
            pattern = newPattern;
            currentAngle = 0f;
            timer = 0f;
        }

        private void Update()
        {
            if (pattern == null || bulletPool == null || bulletPool.IsTimeStopped || Time.deltaTime <= 0f) return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Fire();
                timer = pattern.fireInterval;
            }
        }

        private void Fire()
        {
            switch (pattern.shape)
            {
                case PatternShape.Radial:
                    FireRadial(0f);
                    break;
                case PatternShape.Spiral:
                    FireRadial(currentAngle);
                    currentAngle += pattern.rotationPerShot;
                    break;
                case PatternShape.Aimed:
                    FireAimed();
                    break;
            }
        }

        private void FireRadial(float startAngle)
        {
            float step = 360f / pattern.bulletCount;
            for (int i = 0; i < pattern.bulletCount; i++)
            {
                float angle = startAngle + step * i;
                Vector2 dir = AngleToDirection(angle);
                var bullet = bulletPool.Get(transform.position);
                if (bullet == null) continue;
                bullet.Launch(dir, pattern.bulletSpeed, playerOwned: false);
            }
        }

        private void FireAimed()
        {
            // Tìm player 1 lần rồi cache — FindGameObjectWithTag mỗi lượt bắn (vài lần/giây
            // x nhiều quái) là tốn vô ích. Chỉ tìm lại nếu chưa có (player spawn muộn).
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            Vector2 dir = playerTransform != null
                ? ((Vector2)playerTransform.position - (Vector2)transform.position).normalized
                : Vector2.down;

            var bullet = bulletPool.Get(transform.position);
            if (bullet == null) return;
            bullet.Launch(dir, pattern.bulletSpeed, playerOwned: false);
        }

        private static Vector2 AngleToDirection(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}
