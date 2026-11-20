using UnityEngine;

namespace Houkimajo.UI
{
    /// <summary>Mở cửa hàng bằng fade nhẹ theo thời gian thật, vẫn chạy khi gameplay pause.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ArcaneWindowFade : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float duration = 0.18f;
        private CanvasGroup group;
        private float progress;
        private void Awake() { group = GetComponent<CanvasGroup>(); }
        private void OnEnable() { progress = 0f; group.alpha = 0f; }
        private void Update()
        {
            if (progress >= 1f) return;
            progress = Mathf.Min(1f, progress + Time.unscaledDeltaTime / Mathf.Max(0.01f, duration));
            group.alpha = Mathf.SmoothStep(0f, 1f, progress);
        }
    }
}
