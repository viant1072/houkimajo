using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Chỉ quái thường nhận đẩy lùi; boss không implement giao tiếp này.</summary>
    public interface IKnockbackTarget
    {
        void ApplyKnockback(Vector2 direction, float distance);
    }
}
