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

        public void SetPattern(BulletPatternSO newPattern)
        {
            pattern = newPattern;
            currentAngle = 0f;
            timer = 0f;
        }

        private void Update()
        {
            if (pattern == null || bulletPool == null) return;

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
                bullet.Launch(dir, pattern.bulletSpeed, playerOwned: false);
            }
        }

        private void FireAimed()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            Vector2 dir = player != null
                ? ((Vector2)player.transform.position - (Vector2)transform.position).normalized
                : Vector2.down;

            var bullet = bulletPool.Get(transform.position);
            bullet.Launch(dir, pattern.bulletSpeed, playerOwned: false);
        }

        private static Vector2 AngleToDirection(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}
