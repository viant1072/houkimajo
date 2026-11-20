using UnityEngine;

namespace Houkimajo.Bullets
{
    public enum PatternShape
    {
        Radial,  // tỏa tròn đều quanh điểm bắn
        Spiral,  // như Radial nhưng xoay thêm 1 góc mỗi lượt bắn -> tạo xoắn ốc
        Aimed,   // bắn thẳng vào vị trí player
        AimedFan // chùm đạn đối xứng quanh hướng tới player
    }

    /// <summary>
    /// Định nghĩa 1 kiểu bắn đạn. Tạo asset mới qua chuột phải trong Project window:
    /// Create → Houkimajo → Bullet Pattern, lưu vào Assets/_Game/Data/BulletPatterns.
    /// Thiết kế pattern mới = tạo asset mới, không cần sửa code.
    /// </summary>
    [CreateAssetMenu(fileName = "BulletPattern", menuName = "Houkimajo/Bullet Pattern")]
    public class BulletPatternSO : ScriptableObject
    {
        public PatternShape shape = PatternShape.Radial;

        [Tooltip("Số viên đạn bắn ra mỗi lượt")]
        public int bulletCount = 12;

        [Tooltip("Tốc độ bay của đạn")]
        public float bulletSpeed = 3f;

        [Tooltip("Thời gian giữa 2 lượt bắn (giây)")]
        public float fireInterval = 0.5f;

        [Tooltip("Góc xoay thêm mỗi lượt bắn, chỉ dùng cho kiểu Spiral")]
        public float rotationPerShot = 10f;

        [Tooltip("Góc mở toàn bộ chùm AimedFan, tính bằng độ")]
        [Range(0f, 180f)] public float spreadAngle = 30f;

        [Header("Hình ảnh đạn (trống = dùng hình mặc định của pool)")]
        public Sprite bulletSprite;
        public Color bulletTint = Color.white;
        [Tooltip("Sprite có đầu hướng +Y; xoay hình theo hướng bay")]
        public bool orientBulletToDirection;
    }
}
