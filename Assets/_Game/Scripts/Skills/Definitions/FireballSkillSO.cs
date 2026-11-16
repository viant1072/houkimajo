using Houkimajo.Bullets;
using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Cầu lửa bay tới con trỏ, nổ vùng khi chạm hoặc tới đích.</summary>
    [CreateAssetMenu(fileName = "Skill_Fireball", menuName = "Houkimajo/Skills/Fireball")]
    public class FireballSkillSO : SkillSO
    {
        public SkillProjectile projectilePrefab;
        public float projectileSpeed = 8f;
        public float lifetime = 5f;
        public float burningDuration = 2f;
        public int burningDamagePerSecond = 20;
        public float burningTickInterval = 0.25f;

        public override void Prewarm(SkillContext ctx) { ctx.Pools?.GetPool(projectilePrefab); }

        public override void Activate(SkillContext ctx, SkillLevelData data)
        {
            var projectile = ctx.Pools?.GetPool(projectilePrefab)?.Get(ctx.CasterPosition);
            if (projectile == null) return;
            Vector2 direction = (ctx.AimPoint - ctx.CasterPosition).normalized;
            projectile.Launch(direction, projectileSpeed, ctx.ScaleDamage(data.damage),
                DamageSource.Skill, lifetime, data.radius);
            projectile.SetArt(this, ctx.Pools.Effects);
            projectile.ConfigureFireball(ctx.AimPoint, data.hasMasteryEffect ? burningDuration : 0f,
                ctx.ScaleDamage(Mathf.RoundToInt(burningDamagePerSecond * burningTickInterval)), burningTickInterval);
        }
    }
}
