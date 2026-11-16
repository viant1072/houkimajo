using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Cấu hình skill E/R; trạng thái chạy thuộc SkillEffectController của player.</summary>
    [CreateAssetMenu(fileName = "Skill_MeteorShower", menuName = "Houkimajo/Skills/MeteorShower")]
    public class MeteorShowerSkillSO : SkillSO
    {
        public float warningDuration = 0.4f;
        public float fallDuration = 0.18f;
        public float fallHeight = 3f;
        public float burningDuration = 1f;
        public int burningDamagePerSecond = 20;
        public float burnTickInterval = 0.25f;
        public override void Activate(SkillContext ctx, SkillLevelData data) { ctx.Runtime?.BeginMeteorShower(this, data); }
    }
}
