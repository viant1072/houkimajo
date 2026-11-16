namespace Houkimajo.Bullets
{
    /// <summary>
    /// Bất cứ thứ gì đạn/skill của player bắn trúng được và mất máu (boss, quái thường...).
    /// Đạn chỉ cần biết interface này, không phải biết chi tiết từng loại kẻ địch —
    /// thêm loại địch mới không phải sửa BulletBase.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>Còn sống không — skill tự tìm mục tiêu dùng để bỏ qua địch đã chết.</summary>
        bool IsAlive { get; }

        void TakeDamage(int amount);
    }
}
