using UnityEngine;
using Houkimajo.Bullets;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Tên Lửa Ma Thuật: bắn 1 chùm tên lửa xoè quạt rồi tự bẻ lái về phía địch gần con
    /// trỏ nhất. Không cần ngắm chuẩn nên sát thương mỗi quả thấp.
    /// Dùng: count = số quả, damage = sát thương mỗi quả, radius = bán kính nổ (chỉ khi
    /// hasMasteryEffect).
    /// </summary>
    [CreateAssetMenu(fileName = "Skill_HomingMissile", menuName = "Houkimajo/Skills/Homing Missile")]
    public class HomingMissileSkillSO : SkillSO
    {
        public SkillProjectile projectilePrefab;

        [Tooltip("Tốc độ bay (đơn vị/giây)")]
        public float projectileSpeed = 8f;

        [Tooltip("Tốc độ bẻ lái tối đa (độ/giây)")]
        public float turnRate = 360f;

        [Tooltip("Thời gian sống tối đa (giây)")]
        public float lifetime = 3f;

        [Tooltip("Góc xoè của chùm tên lửa lúc mới bắn ra (độ) — để chúng bay vòng cho đẹp, không chồng lên nhau")]
        public float spreadAngle = 120f;

        public override void Prewarm(SkillContext ctx)
        {
            ctx.Pools?.GetPool(projectilePrefab);
        }

        public override void Activate(SkillContext ctx, SkillLevelData data)
        {
            var pool = ctx.Pools?.GetPool(projectilePrefab);
            if (pool == null) return;

            int count = Mathf.Max(1, data.count);
            int damage = ctx.ScaleDamage(data.damage);
            float explodeRadius = data.hasMasteryEffect ? data.radius : 0f;

            float baseAngle = Mathf.Atan2(ctx.AimDirection.y, ctx.AimDirection.x) * Mathf.Rad2Deg;
            float step = count > 1 ? spreadAngle / (count - 1) : 0f;
            float startAngle = baseAngle - (count > 1 ? spreadAngle / 2f : 0f);

            for (int i = 0; i < count; i++)
            {
                float radians = (startAngle + step * i) * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

                var missile = pool.Get(ctx.CasterPosition + direction * 0.3f);
                if (missile == null) continue;
                missile.Launch(direction, projectileSpeed, damage, DamageSource.Skill, lifetime, explodeRadius);
                missile.SetArt(this, ctx.Pools.Effects);
                missile.EnableHoming(ctx.AimPoint, turnRate);
            }
        }
    }
}
