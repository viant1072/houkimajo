using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Cấu hình skill E/R; trạng thái chạy thuộc SkillEffectController của player.</summary>
    [CreateAssetMenu(fileName = "Skill_FamiliarSummon", menuName = "Houkimajo/Skills/FamiliarSummon")]
    public class FamiliarSummonSkillSO : SkillSO
    {
        public SkillProjectile projectilePrefab;
        public float fireInterval = 0.2f;
        public float bulletSpeed = 10f;
        public float bulletLifetime = 3f;
        public float orbitRadius = 0.85f;
        public float orbitSpeed = 60f;
        public int maximumBlocks = 5;
        public float blockRadius = 0.3f;
        public override void Prewarm(SkillContext ctx) { ctx.Pools?.GetPool(projectilePrefab, 96); }
        public override void Activate(SkillContext ctx, SkillLevelData data) { ctx.Runtime?.BeginFamiliars(this, data); }
    }
}
