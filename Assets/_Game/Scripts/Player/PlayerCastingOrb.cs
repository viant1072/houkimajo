using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Houkimajo.Player
{
    /// <summary>Quả cầu tụ phép tạo sẵn, theo hướng ngắm và làm điểm xuất phát đạn.</summary>
    [DisallowMultipleComponent]
    public class PlayerCastingOrb : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer orbRenderer;
        [SerializeField] private Light2D orbLight;
        [SerializeField, Min(0.1f)] private float distance = 0.5f;
        [SerializeField, Min(0.01f)] private float holdTime = 0.18f;
        [SerializeField, Min(0.01f)] private float size = 0.36f;
        private PlayerAim aim;
        private float remaining;
        private float phase;
        private float pulse;
        private int lastShotFrame;
        public bool IsCasting => remaining > 0f;

        private void Awake() { aim = GetComponent<PlayerAim>(); }
        public Vector2 GetEmissionPosition(Vector2 direction)
        {
            return (Vector2)transform.position + direction.normalized * distance;
        }
        public void NotifyShot(Vector2 direction)
        {
            if (orbRenderer == null) return;
            remaining = Mathf.Max(0.01f, holdTime); pulse = 1f; lastShotFrame = Time.frameCount;
            orbRenderer.transform.position = GetEmissionPosition(direction);
            orbRenderer.transform.localScale = Vector3.one * size;
            orbRenderer.color = Color.white;
            orbRenderer.gameObject.SetActive(true);
            if (orbLight != null) orbLight.intensity = 1.1f;
        }
        private void LateUpdate()
        {
            if (orbRenderer == null || remaining <= 0f || Time.deltaTime <= 0f) return;
            if (lastShotFrame != Time.frameCount) remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            if (remaining <= 0f) { Hide(); return; }
            phase += Time.deltaTime * 5f;
            pulse = Mathf.Max(0f, pulse - Time.deltaTime * 7f);
            Vector2 direction = aim != null ? aim.AimDirection : Vector2.up;
            orbRenderer.transform.position = GetEmissionPosition(direction);
            orbRenderer.transform.localScale = Vector3.one * size * (1f + 0.06f * Mathf.Sin(phase) + 0.12f * pulse);
            orbRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, phase * 12f);
            float fade = Mathf.Clamp01(remaining / 0.08f);
            orbRenderer.color = new Color(1f, 1f, 1f, fade);
            if (orbLight != null) orbLight.intensity = (0.7f + 0.4f * pulse) * fade;
        }
        private void OnEnable() { remaining = 0f; Hide(); }
        private void OnDisable() { remaining = 0f; Hide(); }
        private void Hide()
        {
            if (orbRenderer != null) orbRenderer.gameObject.SetActive(false);
            if (orbLight != null) orbLight.intensity = 0f;
        }
    }
}
