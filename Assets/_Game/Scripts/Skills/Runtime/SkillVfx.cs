using Houkimajo.Bullets;
using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Hiệu ứng dùng thời gian gameplay; có thể kiêm vùng lửa gây damage theo tick.</summary>
    public class SkillVfx : MonoBehaviour
    {
        private SpriteRenderer bodyRenderer;
        private SkillVfxPool pool;
        private float age;
        private float duration;
        private float opacity;
        private bool active;
        private float zoneRadius;
        private int tickDamage;
        private float tickInterval;
        private float nextTick;
        private DamageSource zoneSource;
        private Transform follow;
        private Vector2 followOffset;
        private bool holdOpacity;
        private bool moving;
        private Vector2 moveFrom;
        private Vector2 moveTo;
        private float moveDuration;
        public int Generation { get; private set; }
        public bool IsActive => active;

        private void Awake()
        {
            bodyRenderer = GetComponent<SpriteRenderer>();
            bodyRenderer.sortingOrder = 2;
        }

        public bool IsZoneSlot { get; private set; }

        public void Initialize(SkillVfxPool owner, bool reservedZone)
        {
            pool = owner;
            IsZoneSlot = reservedZone;
            bodyRenderer = GetComponent<SpriteRenderer>();
            bodyRenderer.sortingOrder = 2;
        }

        public void Play(Sprite sprite, Vector2 position, Vector2 size, float lifetime,
            float angle, float alpha)
        {
            age = 0f;
            duration = Mathf.Max(0.01f, lifetime);
            opacity = alpha;
            active = true;
            tickDamage = 0;
            Generation++;
            follow = null;
            moving = false;
            holdOpacity = false;
            bodyRenderer.sprite = sprite;
            bodyRenderer.color = new Color(1f, 1f, 1f, alpha);
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            transform.localScale = new Vector3(size.x, size.y, 1f);
            gameObject.SetActive(true);
        }

        public void ConfigureBurningZone(float radius, int damagePerTick, float interval, DamageSource source = DamageSource.Skill)
        {
            zoneRadius = radius;
            tickDamage = damagePerTick;
            tickInterval = Mathf.Max(0.01f, interval);
            nextTick = tickInterval;
            zoneSource = source;
        }

        public void Follow(Transform target, Vector2 offset)
        {
            follow = target; followOffset = offset; holdOpacity = true;
        }

        public void MoveTo(Vector2 from, Vector2 destination, float seconds)
        {
            moving = true; moveFrom = from; moveTo = destination; moveDuration = Mathf.Max(0.01f, seconds);
        }

        public void SetPose(Vector2 position, Vector2 size, float angle)
        {
            transform.position = position;
            transform.localScale = new Vector3(size.x, size.y, 1f);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            holdOpacity = true;
        }

        public void Stop()
        {
            if (!active) return;
            active = false;
            if (pool != null) pool.Return(this);
            else gameObject.SetActive(false);
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        /// <summary>Tiến hiệu ứng bằng delta có scale; tách riêng để kiểm tra tick khi frame dài.</summary>
        public void Advance(float deltaTime)
        {
            if (!active || deltaTime <= 0f) return;
            age += deltaTime;
            if (follow != null) transform.position = (Vector2)follow.position + followOffset;
            else if (moving) transform.position = Vector2.Lerp(moveFrom, moveTo, Mathf.Clamp01(age / moveDuration));
            // Frame dài vẫn đủ tick, không gây tick vượt thời gian tồn tại.
            while (tickDamage > 0 && nextTick <= Mathf.Min(age, duration) + 0.0001f)
            {
                TargetRegistry.DamageInRadius(transform.position, zoneRadius, tickDamage, zoneSource);
                nextTick += tickInterval;
            }
            bodyRenderer.color = new Color(1f, 1f, 1f, opacity * (holdOpacity ? 1f : 1f - Mathf.Clamp01(age / duration)));
            if (age < duration) return;
            Stop();
        }
    }
}
