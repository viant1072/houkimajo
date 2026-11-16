using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Lướt Chổi: lao nhanh 1 đoạn, bất tử trong lúc lướt. Hướng lướt theo phím di chuyển
    /// đang giữ (tự nhiên khi vừa chạy vừa ngắm ngược lại), không giữ phím thì theo chuột.
    /// Dùng: range = khoảng cách, charges = số lượt tích trữ (Lv5 = 2).
    /// </summary>
    [CreateAssetMenu(fileName = "Skill_Dash", menuName = "Houkimajo/Skills/Dash")]
    public class DashSkillSO : SkillSO
    {
        [Tooltip("Thời gian lướt (giây) — ngắn để không mất quyền điều khiển lâu")]
        public float dashDuration = 0.18f;

        [Tooltip("Bất tử thêm sau khi lướt xong (giây), tránh lướt tới nơi bị trúng ngay")]
        public float extraInvincibility = 0.1f;

        public override void Activate(SkillContext ctx, SkillLevelData data)
        {
            if (ctx.Controller == null) return;

            Vector2 direction = ctx.Controller.MoveInput.sqrMagnitude > 0.01f
                ? ctx.Controller.MoveInput.normalized
                : ctx.AimDirection;

            ctx.Controller.Dash(direction, data.range, dashDuration);
            ctx.Health?.GrantInvincibility(dashDuration + extraInvincibility);
            ctx.Runtime?.ShowDash(this, data, direction);
        }
    }
}
