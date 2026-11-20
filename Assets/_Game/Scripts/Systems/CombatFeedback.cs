using Houkimajo.Bullets;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Houkimajo.Systems
{
    /// <summary>Pool ánh sáng/tia lóe cố định; chỉ phản hồi hình ảnh, không thay luật damage.</summary>
    public class CombatFeedback : MonoBehaviour
    {
        private sealed class Effect
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Light2D Light;
            public float Age;
            public float Duration;
            public float Size;
            public float Intensity;
            public bool Active;
            public int StartedFrame;
        }

        [SerializeField] private Sprite impactSprite;
        [SerializeField, Min(1)] private int capacity = 48;
        [SerializeField, Min(0.01f)] private float impactDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float impactSize = 0.9f;
        [SerializeField, Min(0.01f)] private float muzzleDuration = 0.055f;
        [SerializeField, Min(0.01f)] private float muzzleSize = 0.32f;
        [SerializeField, ColorUsage(false, true)] private Color lightColor = new Color(0.65f, 0.3f, 1f);
        private Effect[] effects;
        private int cursor;
        public static CombatFeedback Instance { get; private set; }
        public int ActiveCount { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { enabled = false; return; }
            Instance = this;
            effects = new Effect[Mathf.Max(1, capacity)];
            for (int i = 0; i < effects.Length; i++)
            {
                var go = new GameObject("CombatSpark_" + i);
                go.transform.SetParent(transform, false);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = impactSprite;
                renderer.sortingOrder = 12;
                var light = go.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Point;
                light.color = lightColor;
                light.pointLightInnerRadius = 0.08f;
                light.pointLightOuterRadius = 0.9f;
                light.falloffIntensity = 0.7f;
                light.intensity = 0f;
                effects[i] = new Effect { Transform = go.transform, Renderer = renderer, Light = light };
                go.SetActive(false);
            }
        }

        private void OnEnable() { if (Instance == this) DamageEvents.OnPlayerHit += HandleHit; }
        private void OnDisable()
        {
            DamageEvents.OnPlayerHit -= HandleHit;
            if (effects == null) return;
            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].Active = false;
                effects[i].Transform.gameObject.SetActive(false);
            }
            ActiveCount = 0;
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance() { Instance = null; }

        private void HandleHit(Vector2 position, DamageSource source)
        {
            // Skill đã có art impact riêng; tránh chồng thêm tia sao cho từng tick beam/burn.
            if (source == DamageSource.BasicShot)
                Play(position, Random.value * 360f, impactSize, impactDuration, 1.5f);
        }

        public void PlayMuzzle(Vector2 position, Vector2 direction)
        {
            Play(position, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f,
                muzzleSize, muzzleDuration, 0.9f);
        }

        private void Play(Vector2 position, float angle, float size, float duration, float intensity)
        {
            if (!isActiveAndEnabled || effects == null || impactSprite == null) return;
            for (int step = 0; step < effects.Length; step++)
            {
                int index = (cursor + step) % effects.Length;
                var effect = effects[index];
                if (effect.Active) continue;
                cursor = (index + 1) % effects.Length;
                effect.Active = true; effect.Age = 0f; effect.Duration = Mathf.Max(0.01f, duration);
                effect.StartedFrame = Time.frameCount;
                effect.Size = size; effect.Intensity = intensity;
                effect.Transform.position = position;
                effect.Transform.rotation = Quaternion.Euler(0f, 0f, angle);
                effect.Transform.localScale = Vector3.one * size * 0.45f;
                effect.Renderer.color = Color.white;
                effect.Light.intensity = intensity;
                effect.Transform.gameObject.SetActive(true);
                ActiveCount++;
                return;
            }
        }

        private void Update()
        {
            if (effects == null || Time.deltaTime <= 0f) return;
            for (int i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (!effect.Active) continue;
                // Hit có thể tới từ FixedUpdate trong frame dài: cho tia lóe hiện ít nhất một frame.
                if (effect.StartedFrame != Time.frameCount) effect.Age += Time.deltaTime;
                float progress = Mathf.Clamp01(effect.Age / effect.Duration);
                effect.Transform.localScale = Vector3.one * effect.Size * Mathf.Lerp(0.45f, 1f, progress);
                effect.Renderer.color = new Color(1f, 1f, 1f, 1f - progress);
                effect.Light.intensity = effect.Intensity * (1f - progress);
                if (progress < 1f) continue;
                effect.Active = false;
                effect.Transform.gameObject.SetActive(false);
                ActiveCount--;
            }
        }
    }
}
