using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Cấu hình skill E/R; trạng thái chạy thuộc SkillEffectController của player.</summary>
    [CreateAssetMenu(fileName = "Skill_Shield", menuName = "Houkimajo/Skills/Shield")]
    public class ShieldSkillSO : SkillSO
    {
        public float breakRadius = 1.5f;
        public float blockInvincibility = 0.5f;
        
        public override void Activate(SkillContext ctx, SkillLevelData data) { ctx.Runtime?.BeginShield(this, data); }
    }
}
