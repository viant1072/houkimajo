using UnityEngine;

namespace Houkimajo.Systems
{
    /// <summary>
    /// Vùng chơi = toàn bộ khung nhìn của camera chính (arena 1 màn hình). Tính từ
    /// camera thay vì hardcode số, để đổi tỉ lệ màn hình / orthographicSize không phải
    /// sửa lại giới hạn ở player và đạn.
    /// </summary>
    public static class ArenaBounds
    {
        private static Rect cachedRect;
        private static int cachedFrame = -1;

        /// <summary>
        /// Hình chữ nhật world-space camera đang nhìn thấy. Cache theo frame vì hàng
        /// trăm viên đạn gọi hàm này mỗi frame — chỉ tính 1 lần/frame là đủ.
        /// </summary>
        public static Rect Get()
        {
            if (cachedFrame == Time.frameCount) return cachedRect;

            var cam = Camera.main;
            if (cam == null || !cam.orthographic)
            {
                // Không có camera ortho: trả vùng mặc định để game không crash.
                cachedRect = new Rect(-9f, -5.5f, 18f, 11f);
            }
            else
            {
                float halfHeight = cam.orthographicSize;
                float halfWidth = halfHeight * cam.aspect;
                Vector2 center = cam.transform.position;
                cachedRect = new Rect(center.x - halfWidth, center.y - halfHeight,
                    halfWidth * 2f, halfHeight * 2f);
            }

            cachedFrame = Time.frameCount;
            return cachedRect;
        }

        /// <summary>Kẹp vị trí vào trong arena, chừa lề <paramref name="margin"/> mỗi cạnh.</summary>
        public static Vector2 Clamp(Vector2 position, float margin)
        {
            var rect = Get();
            return new Vector2(
                Mathf.Clamp(position.x, rect.xMin + margin, rect.xMax - margin),
                Mathf.Clamp(position.y, rect.yMin + margin, rect.yMax - margin));
        }

        /// <summary>True nếu điểm đã ra ngoài arena quá <paramref name="margin"/> đơn vị.</summary>
        public static bool IsOutside(Vector2 position, float margin)
        {
            var rect = Get();
            return position.x < rect.xMin - margin || position.x > rect.xMax + margin
                || position.y < rect.yMin - margin || position.y > rect.yMax + margin;
        }

        // Reset cache khi vào Play mode, phòng trường hợp tắt Domain Reload
        // (Enter Play Mode Options) làm biến static giữ giá trị của lần chạy trước.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            cachedFrame = -1;
        }
    }
}
