using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Cấu hình skill E/R; trạng thái chạy thuộc SkillEffectController của player.</summary>
    [CreateAssetMenu(fileName = "Skill_Teleport", menuName = "Houkimajo/Skills/Teleport")]
    public class TeleportSkillSO : SkillSO
    {
        public float invincibility = 0.3f;
        public int masteryDamage = 50;
        public float masteryRadius = 1f;
        public override void Activate(SkillContext ctx, SkillLevelData data) { ctx.Runtime?.Teleport(this, data); }
    }
}
