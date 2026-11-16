using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Cấu hình skill E/R; trạng thái chạy thuộc SkillEffectController của player.</summary>
    [CreateAssetMenu(fileName = "Skill_TimeStop", menuName = "Houkimajo/Skills/TimeStop")]
    public class TimeStopSkillSO : SkillSO
    {
        public float masteryClearRadius = 3f;
        public override void Activate(SkillContext ctx, SkillLevelData data) { ctx.Runtime?.BeginTimeStop(this, data); }
    }
}
