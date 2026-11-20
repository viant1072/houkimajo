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
                    FireAimed(false);
                    break;
                case PatternShape.AimedFan:
                    FireAimed(true);
                    break;
            }
        }

        private void FireRadial(float startAngle)
        {
            int count = Mathf.Max(1, pattern.bulletCount);
            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + step * i;
                Vector2 dir = AngleToDirection(angle);
                SpawnBullet(dir);
            }
        }

        private void FireAimed(bool fan)
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

            int count = fan ? Mathf.Max(1, pattern.bulletCount) : 1;
            float center = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float spread = Mathf.Clamp(pattern.spreadAngle, 0f, 180f);
            for (int i = 0; i < count; i++)
            {
                float offset = count > 1 ? -spread * 0.5f + spread * i / (count - 1) : 0f;
                SpawnBullet(AngleToDirection(center + offset));
            }
        }

        private void SpawnBullet(Vector2 direction)
        {
            var bullet = bulletPool.Get(transform.position);
            if (bullet == null) return;
            bullet.Launch(direction, pattern.bulletSpeed, playerOwned: false);
            bullet.SetPatternAppearance(pattern.bulletSprite, pattern.bulletTint, pattern.orientBulletToDirection);
        }

        private static Vector2 AngleToDirection(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}
