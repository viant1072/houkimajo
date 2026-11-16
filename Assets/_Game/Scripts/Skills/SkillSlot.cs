namespace Houkimajo.Skills
{
    /// <summary>
    /// 3 ô skill. Giá trị int dùng làm chỉ số mảng trong SkillCaster nên giữ 0..2 liên tục.
    /// </summary>
    public enum SkillSlot
    {
        Q = 0, // tấn công
        E = 1, // phòng thủ / di chuyển
        R = 2  // ultimate
    }
}
