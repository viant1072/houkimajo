using UnityEngine;

namespace Houkimajo.Systems
{
    /// <summary>Overlay trắng tạm thời, giữ nguyên tint/fade/animation của renderer gốc.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] private Material flashMaterial;
        [SerializeField, Min(0.01f)] private float duration = 0.07f;
        private SpriteRenderer body;
        private SpriteRenderer overlay;
        private float remaining;
        private int startedFrame;
        public bool IsFlashing => remaining > 0f;

        private void Awake()
        {
            body = GetComponent<SpriteRenderer>();
            if (flashMaterial == null) return;
            var go = new GameObject("HitFlashOverlay");
            go.transform.SetParent(transform, false);
            overlay = go.AddComponent<SpriteRenderer>();
            overlay.sharedMaterial = flashMaterial;
            overlay.enabled = false;
        }
        private void OnEnable() { ResetFlash(); }
        private void OnDisable() { ResetFlash(); }
        private void ResetFlash() { remaining = 0f; if (overlay != null) overlay.enabled = false; }
        public void Flash()
        {
            if (overlay == null) return;
            remaining = Mathf.Max(0.01f, duration);
            startedFrame = Time.frameCount;
            RefreshOverlay();
        }

        private void LateUpdate()
        {
            if (overlay == null || remaining <= 0f || Time.deltaTime <= 0f) return;
            if (startedFrame != Time.frameCount) remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            RefreshOverlay();
        }

        private void RefreshOverlay()
        {
            overlay.sprite = body.sprite;
            overlay.flipX = body.flipX; overlay.flipY = body.flipY;
            overlay.sortingLayerID = body.sortingLayerID;
            overlay.sortingOrder = body.sortingOrder + 1;
            overlay.color = new Color(1f, 0.94f, 1f, body.color.a * 0.85f * remaining / Mathf.Max(0.01f, duration));
            overlay.enabled = remaining > 0f && body.enabled;
        }
    }
}
