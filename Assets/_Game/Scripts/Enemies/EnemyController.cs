using System;
using UnityEngine;
using Houkimajo.Bullets;
using Houkimajo.Player;
using Houkimajo.Skills;
using Houkimajo.Systems;

namespace Houkimajo.Enemies
{
    /// <summary>
    /// 1 con quái thường: di chuyển theo kiểu trong EnemyDataSO, có máu, có thể bắn và gây
    /// sát thương va chạm. Gộp vào 1 component vì quái lấy ra từ pool rồi được "nạp" data
    /// mới mỗi lần — tách nhiều component chỉ làm việc reset rắc rối hơn.
    /// Prefab: SpriteRenderer + CircleCollider2D (Is Trigger) + Rigidbody2D (Kinematic)
    /// + BulletEmitter.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D), typeof(BulletEmitter), typeof(EnemySpriteAnimation))]
    public class EnemyController : MonoBehaviour, IDamageable, ISkillStatusTarget, IKnockbackTarget
    {
        /// <summary>Phát khi 1 quái bị hạ (không tính quái Strafe tự bay mất).</summary>
        public static event Action<EnemyDataSO, Vector2> OnEnemyKilled;

        [Tooltip("Thời gian 'hiện hình' sau khi spawn: chưa bắn, chưa gây sát thương va chạm")]
        [SerializeField] private float spawnGraceDuration = 0.5f;

        [Tooltip("Quái Strafe ra khỏi mép arena quá bao xa thì coi như đã bay mất")]
        [SerializeField] private float exitMargin = 1.2f;

        [Tooltip("Thời gian sống tối đa của quái Strafe (giây), phòng trường hợp bay kẹt góc")]
        [SerializeField] private float strafeMaxLifetime = 15f;

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;
        private BulletEmitter emitter;
        private EnemySpriteAnimation spriteAnimation;

        private EnemyDataSO data;
        private EnemySpawner owner;
        private Transform player;
        private PickupPool pickups;

        private int currentHP;
        private float graceRemaining;
        private float aliveTime;
        private Vector2 strafeDirection;
        private bool hasEnteredArena;
        private float orbitDirection;

        private float slowFraction;
        private float slowUntil = -1f;
        private float stunUntil = -1f;

        /// <summary>Vị trí trong danh sách đang hoạt động của pool, -1 = đang nằm trong pool.</summary>
        public int PoolIndex { get; set; } = -1;

        public bool IsAlive => data != null && currentHP > 0;
        public bool IsSlowed => Time.time < slowUntil;
        private bool IsStunned => Time.time < stunUntil;
        private bool IsInGrace => graceRemaining > 0f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();
            emitter = GetComponent<BulletEmitter>();
            spriteAnimation = GetComponent<EnemySpriteAnimation>();
        }

        private void OnEnable()
        {
            TargetRegistry.Register(this, transform);
        }

        private void OnDisable()
        {
            TargetRegistry.Unregister(this);
        }

        /// <summary>Nạp data mới mỗi lần lấy ra khỏi pool — reset toàn bộ trạng thái cũ.</summary>
        public void Initialize(EnemyDataSO enemyData, EnemySpawner spawner, Transform playerTransform,
            BulletPool enemyBullets, PickupPool pickupPool)
        {
            data = enemyData;
            owner = spawner;
            player = playerTransform;
            pickups = pickupPool;

            currentHP = enemyData.maxHP;
            graceRemaining = spawnGraceDuration;
            aliveTime = 0f;
            hasEnteredArena = false;
            slowUntil = -1f;
            stunUntil = -1f;
            orbitDirection = UnityEngine.Random.value < 0.5f ? -1f : 1f;

            if (spriteRenderer != null)
            {
                if (enemyData.sprite != null) spriteRenderer.sprite = enemyData.sprite;
                spriteRenderer.color = WithAlpha(enemyData.tint, 0.35f);
            }
            circleCollider.radius = enemyData.colliderRadius;
            spriteAnimation.Configure(enemyData.animationFrames, enemyData.animationFramesPerSecond);

            // Strafe: lao qua màn hình theo hướng về gần tâm arena (lệch ngẫu nhiên),
            // để đường bay luôn cắt ngang vùng chơi chứ không lướt dọc mép.
            Vector2 center = ArenaBounds.Get().center;
            Vector2 target = center + UnityEngine.Random.insideUnitCircle * 2.5f;
            strafeDirection = (target - (Vector2)transform.position).normalized;

            emitter.SetPool(enemyBullets);
            emitter.SetPattern(null);
            emitter.enabled = false; // bật sau khi hết thời gian hiện hình
        }

        private void Update()
        {
            if (data == null) return;
            float deltaTime = Time.deltaTime;
            aliveTime += deltaTime;

            if (IsInGrace)
            {
                graceRemaining -= deltaTime;
                if (spriteRenderer != null)
                {
                    float progress = 1f - Mathf.Clamp01(graceRemaining / spawnGraceDuration);
                    spriteRenderer.color = WithAlpha(data.tint, Mathf.Lerp(0.35f, 1f, progress));
                }
                if (!IsInGrace) EndGrace();
            }

            // Đang choáng: không di chuyển, không bắn.
            bool stunned = IsStunned;
            if (emitter.enabled == stunned && !IsInGrace && data.bulletPattern != null)
                emitter.enabled = !stunned;
            if (stunned) return;

            Move(deltaTime);

            if (data.movement == EnemyMovement.Strafe && HasLeftArena())
            {
                // Bay mất, không rớt thưởng — nhưng vẫn tính là xong để wave kết thúc được.
                Despawn();
            }
        }

        private void EndGrace()
        {
            if (spriteRenderer != null) spriteRenderer.color = data.tint;
            if (data.bulletPattern != null)
            {
                emitter.SetPattern(data.bulletPattern);
                emitter.enabled = true;
            }
        }

        private void Move(float deltaTime)
        {
            float speed = data.moveSpeed * (IsSlowed ? 1f - slowFraction : 1f);
            Vector2 position = transform.position;
            Vector2 toPlayer = player != null ? (Vector2)player.position - position : Vector2.zero;

            Vector2 velocity;
            switch (data.movement)
            {
                case EnemyMovement.Chase:
                    velocity = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized * speed : Vector2.zero;
                    break;

                case EnemyMovement.Orbit:
                {
                    float distance = toPlayer.magnitude;
                    if (distance < 0.0001f) { velocity = Vector2.zero; break; }
                    Vector2 radial = toPlayer / distance;
                    Vector2 tangent = new Vector2(-radial.y, radial.x) * orbitDirection;
                    // Xa hơn bán kính thì tiến vào, gần hơn thì lùi ra, cộng thêm thành phần
                    // tiếp tuyến để bay vòng.
                    float radialError = Mathf.Clamp((distance - data.orbitRadius) / data.orbitRadius, -1f, 1f);
                    velocity = (radial * radialError + tangent).normalized * speed;
                    break;
                }

                default: // Strafe
                    velocity = strafeDirection * speed;
                    break;
            }

            transform.position = position + velocity * deltaTime;
        }

        private bool HasLeftArena()
        {
            bool outside = ArenaBounds.IsOutside(transform.position, exitMargin);
            if (!outside) hasEnteredArena = true;
            return (hasEnteredArena && outside) || aliveTime > strafeMaxLifetime;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (data == null || !data.contactDamage || IsInGrace || currentHP <= 0) return;
            // PlayerHealth tự bỏ qua khi đang bất tử, nên gọi mỗi frame chạm vẫn chỉ mất 1 mạng.
            if (other.TryGetComponent<PlayerHealth>(out var playerHealth))
                playerHealth.TakeHit();
        }

        // ---------------- IDamageable ----------------

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            currentHP -= amount;
            if (currentHP > 0) return;

            Vector2 position = transform.position;
            if (pickups != null && data.reward.gold > 0)
                pickups.SpawnGold(position, data.reward.gold);

            OnEnemyKilled?.Invoke(data, position);
            Despawn();
        }

        // ---------------- Hiệu ứng từ skill ----------------

        public void ApplySlow(float fraction, float duration)
        {
            slowFraction = Mathf.Clamp01(Mathf.Max(fraction, IsSlowed ? slowFraction : 0f));
            slowUntil = Mathf.Max(slowUntil, Time.time + duration);
        }

        public void ApplyStun(float duration)
        {
            stunUntil = Mathf.Max(stunUntil, Time.time + duration);
        }

        public void ApplyKnockback(Vector2 direction, float distance)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            transform.position += (Vector3)(direction.normalized * distance);
        }

        // ---------------- Thu hồi ----------------

        /// <summary>Trả về pool và báo spawner (để đếm quái còn lại của wave).</summary>
        public void Despawn()
        {
            if (PoolIndex < 0) return;
            currentHP = 0;
            emitter.enabled = false;

            var spawner = owner;
            owner = null;
            data = null;
            if (spawner != null)
                spawner.NotifyEnemyGone(this);
            else
                gameObject.SetActive(false); // quái đặt tay trong scene, không qua spawner
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a *= alpha;
            return color;
        }
    }
}
