using UnityEngine;

namespace Houkimajo.Player
{
    /// <summary>
    /// Phát sprite bay và nhún nhẹ trên object hình ảnh, không tác động hitbox của root.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerFlightAnimation : MonoBehaviour
    {
        [Tooltip("Các frame bay theo thứ tự phát vòng lặp")]
        [SerializeField] private Sprite[] frames;

        [Tooltip("Số frame mỗi giây; dùng thời gian có scale để dừng cùng gameplay")]
        [SerializeField, Min(0.1f)] private float framesPerSecond = 6f;

        [Tooltip("Biên độ nhún hình ảnh theo trục Y cục bộ, đơn vị world khi scale = 1")]
        [SerializeField, Min(0f)] private float bobAmplitude = 0.025f;

        [Tooltip("Biên độ nghiêng chổi khi bay, tính bằng độ")]
        [SerializeField, Range(0f, 5f)] private float rockAngle = 1.5f;

        private SpriteRenderer bodyRenderer;
        private PlayerController controller;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private float framePosition;
        private bool canMoveVisual;

        private void Awake()
        {
            bodyRenderer = GetComponent<SpriteRenderer>();
            controller = GetComponentInParent<PlayerController>();
            restPosition = transform.localPosition;
            restRotation = transform.localRotation;

            // Cấu hình nhầm trên root vẫn phát frame nhưng không được làm lệch collider.
            canMoveVisual = transform.parent != null
                && GetComponentInParent<Rigidbody2D>()?.transform != transform;
        }

        private void OnEnable()
        {
            framePosition = 0f;
            SetFrame(0);
        }

        private void LateUpdate()
        {
            if (frames == null || frames.Length == 0 || Time.deltaTime <= 0f) return;

            framePosition = Mathf.Repeat(framePosition + Time.deltaTime * framesPerSecond,
                frames.Length);
            SetFrame(Mathf.FloorToInt(framePosition));

            if (!canMoveVisual) return;

            float phase = framePosition / frames.Length * Mathf.PI * 2f;
            float strength = controller != null && controller.IsFocusMode ? 0f : 1f;
            transform.localPosition = restPosition
                + Vector3.up * (Mathf.Sin(phase) * bobAmplitude * strength);
            float facing = bodyRenderer.flipX ? -1f : 1f;
            transform.localRotation = restRotation
                * Quaternion.Euler(0f, 0f, Mathf.Sin(phase) * rockAngle * strength * facing);
        }

        private void SetFrame(int index)
        {
            if (bodyRenderer == null || frames == null || frames.Length == 0) return;
            Sprite frame = frames[index];
            if (frame != null && bodyRenderer.sprite != frame)
                bodyRenderer.sprite = frame;
        }

        private void OnDisable()
        {
            if (!canMoveVisual) return;
            transform.localPosition = restPosition;
            transform.localRotation = restRotation;
        }
    }
}
