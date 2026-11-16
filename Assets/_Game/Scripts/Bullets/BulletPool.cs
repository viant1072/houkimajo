using System.Collections.Generic;
using UnityEngine;

namespace Houkimajo.Bullets
{
    /// <summary>
    /// Object pool cho đạn — tái sử dụng GameObject thay vì Instantiate/Destroy liên tục.
    /// Bullet-hell bắn hàng trăm viên/giây, nếu tạo/xóa thật sẽ giật lag nặng.
    /// Gắn script này vào 1 GameObject rỗng (ví dụ "PlayerBulletPool" / "EnemyBulletPool"),
    /// kéo bulletPrefab vào Inspector.
    ///
    /// Pool theo dõi luôn danh sách đạn đang bay để skill tác động được lên đạn địch
    /// (xoá đạn quanh người, đóng băng cả màn...) mà không phải FindObjectsOfType.
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        [SerializeField] private BulletBase bulletPrefab;
        [SerializeField] private int initialSize = 100;

        private readonly Queue<BulletBase> pool = new Queue<BulletBase>();
        private readonly List<BulletBase> active = new List<BulletBase>();

        /// <summary>
        /// Hệ số tốc độ chung cho mọi đạn của pool (1 = bình thường, 0 = đóng băng).
        /// Dùng cho skill kiểu "ngưng đọng thời gian" chỉ tác động lên đạn địch.
        /// </summary>
        public float SpeedScale { get; set; } = 1f;

        public int ActiveCount => active.Count;
        private int timeStopCount;
        private Sprite frozenOverlay;
        public bool IsTimeStopped => timeStopCount > 0;
        public float MotionScale => IsTimeStopped ? 0f : SpeedScale;

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
            if (pool.Count == 0) return null;
            BulletBase bullet = pool.Dequeue();
            bullet.transform.position = position;
            bullet.PoolIndex = active.Count;
            active.Add(bullet);
            bullet.gameObject.SetActive(true);
            bullet.ShowFrozen(IsTimeStopped, frozenOverlay);
            return bullet;
        }

        public void Return(BulletBase bullet)
        {
            // 1 viên có thể chạm 2 collider trong cùng frame -> bị trả 2 lần. Không chặn
            // thì nó nằm 2 lần trong queue và sau này bị "lấy ra" cho 2 chỗ cùng lúc.
            if (bullet.PoolIndex < 0) return;

            // Xoá kiểu swap-with-last: O(1), thứ tự trong list không quan trọng.
            int index = bullet.PoolIndex;
            int lastIndex = active.Count - 1;
            var last = active[lastIndex];
            active[index] = last;
            last.PoolIndex = index;
            active.RemoveAt(lastIndex);

            bullet.PoolIndex = -1;
            bullet.ShowFrozen(false, null);
            bullet.gameObject.SetActive(false);
            pool.Enqueue(bullet);
        }

        /// <summary>Xoá mọi đạn trong bán kính, trả về số viên đã xoá (để rớt xu, cộng điểm...).</summary>
        public int ClearInRadius(Vector2 center, float radius, int maximum = int.MaxValue)
        {
            float radiusSqr = radius * radius;
            int cleared = 0;

            // Duyệt ngược vì Return() đổi chỗ phần tử cuối vào vị trí bị xoá.
            for (int i = active.Count - 1; i >= 0 && cleared < maximum; i--)
            {
                var bullet = active[i];
                if (((Vector2)bullet.transform.position - center).sqrMagnitude <= radiusSqr)
                {
                    Return(bullet);
                    cleared++;
                }
            }

            return cleared;
        }

        public int ClearAlongLine(Vector2 start, Vector2 end, float width)
        {
            int cleared = 0;
            float radiusSquared = width * width * 0.25f;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (CombatGeometry.DistanceToSegmentSquared(active[i].transform.position, start, end) > radiusSquared) continue;
                Return(active[i]);
                cleared++;
            }
            return cleared;
        }

        public void ApplySlowInRadius(Vector2 center, float radius, float scale)
        {
            float squared = radius * radius;
            for (int i = 0; i < active.Count; i++)
                if (((Vector2)active[i].transform.position - center).sqrMagnitude <= squared) active[i].ApplyFieldSlow(scale);
        }

        public void SetTimeStopped(bool stopped, Sprite overlay = null)
        {
            timeStopCount = Mathf.Max(0, timeStopCount + (stopped ? 1 : -1));
            if (overlay != null) frozenOverlay = overlay;
            for (int i = 0; i < active.Count; i++) active[i].ShowFrozen(IsTimeStopped, frozenOverlay);
        }

        /// <summary>Xoá toàn bộ đạn đang bay của pool, trả về số viên đã xoá.</summary>
        public int ClearAll()
        {
            int cleared = active.Count;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                Return(active[i]);
            }
            return cleared;
        }
    }
}
