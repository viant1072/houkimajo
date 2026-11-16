using Houkimajo.Bullets;
using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Quạt băng xuyên tối đa ba địch, làm chậm quái thường.</summary>
    [CreateAssetMenu(fileName = "Skill_IceArrowRain", menuName = "Houkimajo/Skills/Ice Arrow Rain")]
    public class IceArrowRainSkillSO : SkillSO
    {
        public SkillProjectile projectilePrefab;
        public float projectileSpeed = 10f;
        public float lifetime = 3f;
        public float spreadAngle = 60f;
        [Range(0f, 1f)] public float slowFraction = 0.4f;

        public override void Prewarm(SkillContext ctx) { ctx.Pools?.GetPool(projectilePrefab); }

        public override void Activate(SkillContext ctx, SkillLevelData data)
        {
            var pool = ctx.Pools?.GetPool(projectilePrefab);
            if (pool == null) return;
            int count = Mathf.Max(1, data.count);
            float angle = Mathf.Atan2(ctx.AimDirection.y, ctx.AimDirection.x) * Mathf.Rad2Deg;
            float step = count > 1 ? spreadAngle / (count - 1) : 0f;
            float start = angle - (count > 1 ? spreadAngle * 0.5f : 0f);
            for (int i = 0; i < count; i++)
            {
                float radians = (start + i * step) * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var projectile = pool.Get(ctx.CasterPosition + direction * 0.3f);
                if (projectile == null) continue;
                projectile.Launch(direction, projectileSpeed, ctx.ScaleDamage(data.damage), DamageSource.Skill, lifetime);
                projectile.SetArt(this, ctx.Pools.Effects);
                projectile.ConfigureIce(slowFraction, data.duration, data.hasMasteryEffect);
            }
        }
    }
}
