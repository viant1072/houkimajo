using System;
using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Chỉ số của 1 skill ở 1 cấp. Dùng chung cho mọi skill — field nào skill không cần
    /// thì bỏ qua, đổi lại thêm skill mới không phải tạo kiểu dữ liệu mới.
    /// </summary>
    [Serializable]
    public class SkillLevelData
    {
        [Tooltip("Thời gian hồi chiêu (giây)")]
        public float cooldown = 6f;

        [Tooltip("Sát thương mỗi lần trúng")]
        public int damage;

        [Tooltip("Số lượng (đạn, quả tên lửa, lần nhảy...)")]
        public int count = 1;

        [Tooltip("Bán kính vùng tác dụng / vụ nổ (đơn vị world)")]
        public float radius;

        [Tooltip("Thời gian tác dụng (giây)")]
        public float duration;

        [Tooltip("Tầm xa / khoảng cách (đơn vị world)")]
        public float range;

        [Tooltip("Số lượt dùng tích trữ được, hồi từng lượt một")]
        public int charges = 1;

        [Tooltip("Giá vàng để nâng LÊN cấp này (cấp 1 bỏ qua)")]
        public int upgradeCost;

        [Tooltip("Bật hiệu ứng đặc biệt (thường chỉ có ở cấp cao nhất)")]
        public bool hasMasteryEffect;
    }
}
