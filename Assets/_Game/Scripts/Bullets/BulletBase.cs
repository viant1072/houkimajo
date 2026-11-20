using UnityEngine;
using UnityEngine.Serialization;
using Houkimajo.Player;
using Houkimajo.Systems;

namespace Houkimajo.Bullets
{
    /// <summary>
    /// Hành vi của 1 viên đạn — dùng chung cho đạn của player lẫn đạn của enemy/boss.
    /// Tự trả về pool khi bay ra khỏi arena hoặc va chạm, thay vì Destroy (xem BulletPool).
    /// Prefab cần có Collider2D (Is Trigger = true) và Rigidbody2D (Body Type = Kinematic)
    /// để OnTriggerEnter2D hoạt động.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BulletBase : MonoBehaviour
    {
        [Tooltip("Sát thương mặc định nếu lúc Launch() không truyền sát thương riêng")]
        [FormerlySerializedAs("damage")]
        [SerializeField] private int defaultDamage = 10;

        [Tooltip("Ra khỏi mép arena quá bao nhiêu đơn vị thì thu hồi về pool")]
        [SerializeField] private float despawnMargin = 1f;

        [Tooltip("Xoay sprite có đầu hướng +Y theo hướng bay mỗi lần lấy đạn từ pool")]
        [SerializeField] private bool alignToDirection;

        private Vector2 direction;
        private float speed;
        private int damage;
        private bool isPlayerBullet;
        private BulletPool ownerPool;
        private SpriteRenderer bodyRenderer;
        private Sprite defaultSprite;
        private Color defaultTint;
        private Quaternion defaultRotation;
        private SpriteRenderer frozenRenderer;
        private int slowFrame = -1;
        private float fieldScale = 1f;
        private Sprite[] animationFrames;
        private float animationFramesPerSecond;
        private float animationPosition;

        private void Awake()
        {
            bodyRenderer = GetComponent<SpriteRenderer>();
            if (bodyRenderer != null)
            {
                defaultSprite = bodyRenderer.sprite;
                defaultTint = bodyRenderer.color;
            }
            defaultRotation = transform.localRotation;
            var overlay = new GameObject("TimeStopSeal");
            overlay.transform.SetParent(transform, false);
            overlay.transform.localScale = Vector3.one * 1.6f;
            frozenRenderer = overlay.AddComponent<SpriteRenderer>();
            frozenRenderer.sortingOrder = 4;
            frozenRenderer.color = new Color(1f, 1f, 1f, 0.65f);
            frozenRenderer.enabled = false;
        }

        /// <summary>Profile đổi hình/tint, còn collider và prefab pool giữ nguyên.</summary>
        public void SetAppearance(Sprite sprite, Color tint)
        {
            animationFrames = null;
            animationPosition = 0f;
            if (bodyRenderer == null) return;
            bodyRenderer.sprite = sprite != null ? sprite : defaultSprite;
            bodyRenderer.color = tint;
        }

        /// <summary>Dùng mảng frame của profile; mỗi lần bắn bắt đầu lại, không cấp phát trong pool.</summary>
        public void SetAnimatedAppearance(Sprite[] frames, float framesPerSecond)
        {
            animationFrames = frames;
            animationFramesPerSecond = Mathf.Max(0.1f, framesPerSecond);
            animationPosition = 0f;
            if (bodyRenderer != null && frames != null && frames.Length > 0 && frames[0] != null)
                bodyRenderer.sprite = frames[0];
        }

        public void SetPatternAppearance(Sprite sprite, Color tint, bool orientToDirection)
        {
            if (sprite != null) SetAppearance(sprite, tint);
            if (orientToDirection && direction.sqrMagnitude > 0f)
                transform.rotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        }

        /// <summary>Vị trí trong danh sách đạn đang bay của pool, -1 = đang nằm trong pool.</summary>
        public int PoolIndex { get; set; } = -1;

        public bool IsPlayerBullet => isPlayerBullet;

        /// <summary>Đã tính graze cho viên này chưa — mỗi viên chỉ được tính 1 lần.</summary>
        public bool HasBeenGrazed { get; private set; }

        /// <summary>
        /// Hệ số tốc độ riêng của viên này (vùng làm chậm của skill). Nhân thêm với
        /// SpeedScale của pool.
        /// </summary>
        public float LocalSpeedScale { get; set; } = 1f;

        /// <param name="damageOverride">Sát thương của viên này; âm = dùng defaultDamage của prefab.</param>
        public void Launch(Vector2 dir, float spd, bool playerOwned = false, int damageOverride = -1)
        {
            direction = dir.normalized;
            speed = spd;
            isPlayerBullet = playerOwned;
            damage = damageOverride >= 0 ? damageOverride : defaultDamage;
            SetAppearance(null, defaultTint);
            transform.localRotation = defaultRotation;

            if (alignToDirection && direction.sqrMagnitude > 0f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Đạn lấy lại từ pool còn giữ trạng thái của lần bắn trước -> reset hết ở đây.
            HasBeenGrazed = false;
            LocalSpeedScale = 1f;
            slowFrame = -1;
        }

        public void SetOwnerPool(BulletPool pool)
        {
            ownerPool = pool;
        }

        public void MarkGrazed()
        {
            HasBeenGrazed = true;
        }

        private void Update()
        {
            if (Time.deltaTime > 0f && (ownerPool == null || !ownerPool.IsTimeStopped)
                && bodyRenderer != null && animationFrames != null && animationFrames.Length > 0)
            {
                animationPosition = Mathf.Repeat(animationPosition + Time.deltaTime * animationFramesPerSecond,
                    animationFrames.Length);
                Sprite frame = animationFrames[Mathf.FloorToInt(animationPosition)];
                if (frame != null && bodyRenderer.sprite != frame) bodyRenderer.sprite = frame;
            }
            float poolScale = ownerPool != null ? ownerPool.MotionScale : 1f;
            float zoneScale = slowFrame == Time.frameCount ? fieldScale : 1f;
            transform.position += (Vector3)(direction * (speed * LocalSpeedScale * zoneScale * poolScale * Time.deltaTime));

            if (ArenaBounds.IsOutside(transform.position, despawnMargin))
            {
                ReturnToPool();
            }
        }

        public void ApplyFieldSlow(float scale)
        {
            fieldScale = slowFrame == Time.frameCount ? Mathf.Min(fieldScale, scale) : scale;
            slowFrame = Time.frameCount;
        }

        public void ShowFrozen(bool show, Sprite sprite)
        {
            if (frozenRenderer == null) return;
            frozenRenderer.sprite = sprite;
            frozenRenderer.enabled = show && sprite != null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (ownerPool != null && PoolIndex < 0) return;
            if (isPlayerBullet)
            {
                if (other.TryGetComponent<IDamageable>(out var target))
                {
                    // Đi qua DamageEvents để điểm / năng lượng ultimate nghe chung 1 chỗ
                    // với sát thương của skill.
                    DamageEvents.DealPlayerDamage(target, damage, DamageSource.BasicShot,
                        other.ClosestPoint(transform.position));
                    ReturnToPool();
                }
            }
            else
            {
                if (other.TryGetComponent<PlayerHealth>(out var playerHealth))
                {
                    playerHealth.TakeHit();
                    ReturnToPool();
                }
            }
        }

        private void ReturnToPool()
        {
            if (ownerPool != null)
                ownerPool.Return(this);
            else
                gameObject.SetActive(false); // đạn đặt tay trong scene, không có pool
        }
    }
}
