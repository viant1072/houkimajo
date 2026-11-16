using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Cấu hình skill E/R; trạng thái chạy thuộc SkillEffectController của player.</summary>
    [CreateAssetMenu(fileName = "Skill_StarBeam", menuName = "Houkimajo/Skills/StarBeam")]
    public class StarBeamSkillSO : SkillSO
    {
        public float tickInterval = 0.1f;
        public float bodyWidthCoverage = 0.1f;
        public float bodyLengthCoverage = 0.55f;
        public float capCoverage = 0.32f;
        public int masteryDamage = 200;
        public float masteryRadius = 1.5f;
        public float movementMultiplier = 0.5f;
        public override void Activate(SkillContext ctx, SkillLevelData data) { ctx.Runtime?.BeginBeam(this, data); }
    }
}
