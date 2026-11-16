using UnityEngine;
using Houkimajo.Bullets;
using Houkimajo.Systems;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Projectile dùng chung cho skill (tên lửa, cầu lửa, mũi băng, thiên thạch...).
    /// Hành vi bật/tắt qua tham số lúc Launch: tự đuổi mục tiêu, nổ khi trúng. Gom vào 1
    /// component để mỗi skill mới chỉ cần cấu hình, không viết projectile mới.
    /// Prefab cần Collider2D (Is Trigger) + Rigidbody2D (Kinematic), giống đạn thường.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SkillProjectile : MonoBehaviour
    {
        [Tooltip("Ra khỏi mép arena quá bao nhiêu đơn vị thì thu hồi")]
        [SerializeField] private float despawnMargin = 1f;

        private SkillProjectilePool ownerPool;

        private Vector2 velocity;
        private float speed;
        private int damage;
        private DamageSource source;
        private float lifetimeRemaining;

        private bool isHoming;
        private float turnRateDegrees;
        private IDamageable homingTarget;
        private Transform homingTargetTransform;

        private float explosionRadius;
        private SkillSO art;
        private SkillVfxPool effects;
        private bool isFireball;
        private Vector2 destination;
        private float burningDuration;
        private int burningTickDamage;
        private float burningTickInterval;
        private int pierceLimit;
        private int hitCount;
        private readonly IDamageable[] hitTargets = new IDamageable[3];
        private float slowFraction;
        private float slowDuration;
        private bool bonusOnSlowed;

        /// <summary>Vị trí trong danh sách đang hoạt động của pool, -1 = đang nằm trong pool.</summary>
        public int PoolIndex { get; set; } = -1;

        public void SetOwnerPool(SkillProjectilePool pool)
        {
            ownerPool = pool;
        }

        /// <summary>Bắn thẳng, không đuổi mục tiêu.</summary>
        public void Launch(Vector2 direction, float projectileSpeed, int projectileDamage,
            DamageSource damageSource, float lifetime, float explodeRadius = 0f)
        {
            speed = projectileSpeed;
            velocity = direction.normalized * projectileSpeed;
            damage = projectileDamage;
            source = damageSource;
            lifetimeRemaining = lifetime;
            explosionRadius = explodeRadius;

            // Lấy lại từ pool còn giữ trạng thái lần trước -> reset hết.
            isHoming = false;
            homingTarget = null;
            homingTargetTransform = null;
            art = null;
            effects = null;
            isFireball = false;
            burningDuration = 0f;
            pierceLimit = 1;
            hitCount = 0;
            slowFraction = 0f;
            bonusOnSlowed = false;
            System.Array.Clear(hitTargets, 0, hitTargets.Length);

            AlignToVelocity();
        }

        public void SetArt(SkillSO skill, SkillVfxPool vfx)
        {
            art = skill;
            effects = vfx;
        }

        public void ConfigureFireball(Vector2 aimPoint, float burnDuration, int tickDamage, float interval)
        {
            isFireball = true;
            destination = aimPoint;
            burningDuration = burnDuration;
            burningTickDamage = tickDamage;
            burningTickInterval = interval;
        }

        public void ConfigureIce(float slow, float duration, bool mastery)
        {
            pierceLimit = hitTargets.Length;
            slowFraction = slow;
            slowDuration = duration;
            bonusOnSlowed = mastery;
        }

        /// <summary>
        /// Bật chế độ tự đuổi: chọn địch gần <paramref name="aimPoint"/> nhất, mục tiêu chết
        /// giữa đường thì tìm lại. Gọi ngay sau Launch().
        /// </summary>
        public void EnableHoming(Vector2 aimPoint, float turnRate)
        {
            isHoming = true;
            turnRateDegrees = turnRate;
            AcquireTarget(aimPoint);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f || PoolIndex < 0) return;

            if (isHoming) SteerTowardsTarget(deltaTime);

            if (isFireball && Vector2.Distance(transform.position, destination) <= speed * deltaTime)
            {
                transform.position = destination;
                Explode();
                return;
            }

            transform.position += (Vector3)(velocity * deltaTime);

            lifetimeRemaining -= deltaTime;
            if (lifetimeRemaining <= 0f || ArenaBounds.IsOutside(transform.position, despawnMargin))
            {
                Despawn();
            }
        }

        private void SteerTowardsTarget(float deltaTime)
        {
            if (homingTarget == null || !homingTarget.IsAlive || homingTargetTransform == null
                || !homingTargetTransform.gameObject.activeInHierarchy)
            {
                // Mục tiêu cũ chết -> tìm địch gần chính quả tên lửa nhất.
                AcquireTarget(transform.position);
                if (homingTarget == null) return; // không còn địch: bay thẳng tiếp
            }

            Vector2 desired = ((Vector2)homingTargetTransform.position - (Vector2)transform.position).normalized;
            Vector2 current = velocity.sqrMagnitude > 0f ? velocity.normalized : desired;

            float maxRadians = turnRateDegrees * Mathf.Deg2Rad * deltaTime;
            Vector2 newDirection = Vector3.RotateTowards(current, desired, maxRadians, 0f);
            velocity = newDirection * speed;
            AlignToVelocity();
        }

        private void AcquireTarget(Vector2 point)
        {
            int index = TargetRegistry.FindNearest(point);
            if (index < 0)
            {
                homingTarget = null;
                homingTargetTransform = null;
                return;
            }

            homingTarget = TargetRegistry.GetTarget(index);
            homingTargetTransform = TargetRegistry.GetTransform(index);
        }

        private void AlignToVelocity()
        {
            if (velocity.sqrMagnitude <= 0f) return;
            // Sprite projectile quy ước đầu hướng +Y (giống đạn player).
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (PoolIndex < 0) return; // đã thu hồi trong frame này
            if (!other.TryGetComponent<IDamageable>(out var target) || !target.IsAlive) return;

            if (isFireball)
            {
                Explode();
                return;
            }
            for (int i = 0; i < hitCount; i++)
            {
                if (hitTargets[i] == target) return;
            }
            hitTargets[hitCount++] = target;
            var status = target as ISkillStatusTarget;
            bool bonus = bonusOnSlowed && status != null && status.IsSlowed;
            if (slowFraction > 0f && status != null) status.ApplySlow(slowFraction, slowDuration);

            DamageEvents.DealPlayerDamage(target, bonus ? damage * 2 : damage, source);
            PlayImpact(bonus);

            if (explosionRadius > 0f)
            {
                // Vụ nổ lan ra xung quanh, mục tiêu chính đã ăn sát thương trực tiếp rồi.
                TargetRegistry.DamageInRadius(transform.position, explosionRadius, damage / 2, source, target);
                PlayRing(art != null ? art.masterySprite : null, explosionRadius, 0.3f);
            }

            if (hitCount >= pierceLimit) Despawn();
        }

        private void PlayImpact(bool mastery)
        {
            if (art == null || effects == null) return;
            Sprite sprite = mastery ? art.masterySprite : art.impactSprite;
            effects.Play(sprite, transform.position, Vector2.one * art.impactSize, art.impactDuration);
        }

        private SkillVfx PlayRing(Sprite sprite, float radius, float duration, float opacity = 0.7f)
        {
            if (art == null || effects == null) return null;
            float size = 2f * radius / Mathf.Max(0.1f, art.ringCoverage);
            return effects.Play(sprite, transform.position, Vector2.one * size, duration, opacity: opacity);
        }

        private void Explode()
        {
            TargetRegistry.DamageInRadius(transform.position, explosionRadius, damage, source);
            PlayRing(art != null ? art.impactSprite : null, explosionRadius, 0.3f);
            if (burningDuration > 0f)
            {
                float size = 2f * explosionRadius / (art != null ? Mathf.Max(0.1f, art.ringCoverage) : 0.8f);
                effects?.PlayBurningZone(art != null ? art.masterySprite : null, transform.position,
                    size, burningDuration, explosionRadius, burningTickDamage, burningTickInterval);
            }
            Despawn();
        }

        private void Despawn()
        {
            if (ownerPool != null)
                ownerPool.Return(this);
            else
                gameObject.SetActive(false);
        }
    }
}
