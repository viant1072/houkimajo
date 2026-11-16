using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Cấu hình skill E/R; trạng thái chạy thuộc SkillEffectController của player.</summary>
    [CreateAssetMenu(fileName = "Skill_Dispel", menuName = "Houkimajo/Skills/Dispel")]
    public class DispelSkillSO : SkillSO
    {
        public int maximumGold = 30;
        public float knockbackDistance = 2f;
        public override void Activate(SkillContext ctx, SkillLevelData data) { ctx.Runtime?.Dispel(this, data); }
    }
}
