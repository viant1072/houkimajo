using System.Collections.Generic;
using UnityEngine;
using Houkimajo.Bullets;

namespace Houkimajo.Skills
{
    /// <summary>Pool VFX cố định, hết chỗ thì bỏ hình ảnh thay vì tạo object giữa trận.</summary>
    public class SkillVfxPool : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 128;
        [SerializeField, Min(1)] private int zoneCapacity = 32;
        private readonly Queue<SkillVfx> available = new Queue<SkillVfx>();
        private readonly Queue<SkillVfx> zones = new Queue<SkillVfx>();
        private bool initialized;

        private void Awake()
        {
            if (initialized) return;
            initialized = true;
            for (int i = 0; i < capacity + zoneCapacity; i++)
            {
                var go = new GameObject("SkillVfx");
                go.transform.SetParent(transform, false);
                go.AddComponent<SpriteRenderer>();
                var effect = go.AddComponent<SkillVfx>();
                effect.Initialize(this, i >= capacity);
                go.SetActive(false);
                if (i >= capacity) zones.Enqueue(effect);
                else available.Enqueue(effect);
            }
        }

        public SkillVfx Play(Sprite sprite, Vector2 position, Vector2 size, float duration,
            float angle = 0f, float opacity = 1f)
        {
            if (sprite == null || available.Count == 0) return null;
            var effect = available.Dequeue();
            effect.Play(sprite, position, size, duration, angle, opacity);
            return effect;
        }

        public void Return(SkillVfx effect)
        {
            effect.gameObject.SetActive(false);
            if (effect.IsZoneSlot) zones.Enqueue(effect);
            else available.Enqueue(effect);
        }

        public SkillVfx PlayBurningZone(Sprite sprite, Vector2 position, float size, float duration,
            float radius, int tickDamage, float interval, DamageSource source = DamageSource.Skill)
        {
            // Vùng gây damage có pool riêng, không bị các tia sáng chiếm hết chỗ.
            if (zones.Count == 0) return null;
            var effect = zones.Dequeue();
            effect.Play(sprite, position, Vector2.one * size, duration, 0f, 0.5f);
            effect.ConfigureBurningZone(radius, tickDamage, interval, source);
            return effect;
        }
    }
}
