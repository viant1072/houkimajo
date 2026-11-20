using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Định nghĩa 1 skill: thông tin hiển thị + chỉ số theo cấp + hành vi khi dùng.
    /// Mỗi HÀNH VI là 1 subclass (thư mục Definitions/); biến thể cùng hành vi khác chỉ
    /// số = tạo asset mới, không sửa code.
    ///
    /// QUAN TRỌNG: asset dùng chung và giữ giá trị giữa các lần Play, nên subclass
    /// KHÔNG được lưu trạng thái lúc chạy vào field. Trạng thái (khiên còn mấy lần đỡ,
    /// tia đang bắn...) phải nằm ở component trong scene (thư mục Runtime/).
    /// </summary>
    public abstract class SkillSO : ScriptableObject
    {
        [Tooltip("Mã định danh cố định, dùng cho file save. Đổi tên asset không ảnh hưởng.")]
        public string id;

        public string displayName = "Skill";
        public Sprite icon;
        public Sprite castSprite;
        public Sprite loopSprite;
        public Sprite endSprite;
        public Sprite projectileSprite;
        public Sprite warningSprite;
        public Sprite particleSprite;
        public Sprite overlaySprite;
        public string attributeName = "Phép";
        public Sprite impactSprite;
        public Sprite masterySprite;
        [Tooltip("Kích thước VFX trúng theo world unit")]
        public float impactSize = 0.5f;
        [Tooltip("Thời gian VFX trúng (giây)")]
        public float impactDuration = 0.18f;
        [Tooltip("Đường kính hình vòng so với chiều rộng ô sprite")]
        [Range(0.1f, 1f)] public float ringCoverage = 0.8f;
        public SkillSlot slot = SkillSlot.Q;

        [SerializeField, Min(0f), Tooltip("MP cho Q/E; R luôn dùng năng lượng ultimate riêng")]
        private float manaCost = 15f;
        public float ManaCost => slot == SkillSlot.R ? 0f : Mathf.Max(0f, manaCost);

        [Tooltip("Màu chủ đạo của skill (hiệu ứng, icon placeholder)")]
        public Color color = Color.white;

        [TextArea] public string description;

        [Tooltip("Chỉ số từ cấp 1 tới cấp tối đa")]
        public SkillLevelData[] levels = new SkillLevelData[0];

        public int MaxLevel => levels.Length;

        /// <summary>Chỉ số ở cấp <paramref name="level"/> (bắt đầu từ 1), tự kẹp trong khoảng hợp lệ.</summary>
        public SkillLevelData GetLevel(int level)
        {
            if (levels.Length == 0) return null;
            return levels[Mathf.Clamp(level, 1, levels.Length) - 1];
        }

        /// <summary>
        /// Gọi lúc bắt đầu stage để tạo sẵn pool/hiệu ứng, tránh khựng frame lần đầu dùng skill.
        /// </summary>
        public virtual void Prewarm(SkillContext ctx) { }

        /// <summary>Thực hiện skill. Hồi chiêu do SkillCaster lo, skill không tự quản lý.</summary>
        public abstract void Activate(SkillContext ctx, SkillLevelData data);
    }
}
