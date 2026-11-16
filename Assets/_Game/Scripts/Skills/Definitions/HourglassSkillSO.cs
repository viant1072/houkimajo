using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Cấu hình skill E/R; trạng thái chạy thuộc SkillEffectController của player.</summary>
    [CreateAssetMenu(fileName = "Skill_Hourglass", menuName = "Houkimajo/Skills/Hourglass")]
    public class HourglassSkillSO : SkillSO
    {
        [Range(0f, 1f)] public float bulletSlow = 0.5f;
        [Range(0f, 1f)] public float masteryBulletSlow = 0.7f;
        [Range(0f, 1f)] public float masteryEnemySlow = 0.3f;
        public override void Activate(SkillContext ctx, SkillLevelData data) { ctx.Runtime?.BeginSlowField(this, data); }
    }
}
