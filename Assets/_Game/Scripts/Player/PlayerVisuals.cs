using UnityEngine;

namespace Houkimajo.Player
{
    /// <summary>
    /// Phản hồi hình ảnh cho người chơi: hiện hitbox khi giữ Shift, nhấp nháy khi đang
    /// bất tử sau khi trúng đạn, lật sprite theo hướng ngắm. Tách riêng khỏi logic để
    /// sau này thay bằng animation thật không phải đụng vào code điều khiển/máu.
    /// </summary>
    public class PlayerVisuals : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bodyRenderer;

        [Tooltip("Chấm nhỏ thể hiện hitbox thật, chỉ hiện khi focus mode")]
        [SerializeField] private GameObject hitboxIndicator;

        [Tooltip("Số lần nhấp nháy mỗi giây khi đang bất tử")]
        [SerializeField] private float blinkFrequency = 10f;

        private PlayerController controller;
        private PlayerHealth health;
        private PlayerAim aim;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();
            aim = GetComponent<PlayerAim>();
        }

        private void Update()
        {
            if (hitboxIndicator != null && controller != null
                && hitboxIndicator.activeSelf != controller.IsFocusMode)
            {
                hitboxIndicator.SetActive(controller.IsFocusMode);
            }

            if (bodyRenderer == null) return;

            // Nhấp nháy bằng cách bật/tắt renderer theo thời gian — rẻ, không cần coroutine.
            // Chỉ nhấp nháy khi bất tử do trúng đạn, không nhấp nháy khi bất tử do skill.
            bool visible = health == null || !health.IsRecoveringFromHit
                || Mathf.Repeat(Time.time * blinkFrequency, 1f) < 0.5f;
            bodyRenderer.enabled = visible;

            if (aim != null)
                bodyRenderer.flipX = aim.AimDirection.x < 0f;
        }
    }
}
