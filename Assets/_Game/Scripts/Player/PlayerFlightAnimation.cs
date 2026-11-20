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

        [Tooltip("Các frame giơ tay niệm phép, dùng khi bắn thường thành công")]
        [SerializeField] private Sprite[] castingFrames;
        [SerializeField, Min(0.1f)] private float castingFramesPerSecond = 10f;
        [SerializeField, Min(0.01f)] private float castHoldTime = 0.18f;

        [Tooltip("Biên độ nhún hình ảnh theo trục Y cục bộ, đơn vị world khi scale = 1")]
        [SerializeField, Min(0f)] private float bobAmplitude = 0.025f;

        [Tooltip("Biên độ nghiêng chổi khi bay, tính bằng độ")]
        [SerializeField, Range(0f, 5f)] private float rockAngle = 1.5f;

        private SpriteRenderer bodyRenderer;
        public Sprite PreviewFrame(float seconds)
        {
            if (frames == null || frames.Length == 0) return bodyRenderer.sprite;
            return frames[Mathf.FloorToInt(seconds * framesPerSecond) % frames.Length];
        }
        private PlayerController controller;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private float framePosition;
        private bool canMoveVisual;
        private float castRemaining;
        private int lastShotFrame;
        public bool IsCasting => castRemaining > 0f && castingFrames != null && castingFrames.Length > 0;

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
            castRemaining = 0f;
            SetFrame(frames, 0);
        }

        public void NotifyShot()
        {
            bool wasCasting = IsCasting;
            castRemaining = Mathf.Max(0.01f, castHoldTime);
            lastShotFrame = Time.frameCount;
            if (!wasCasting)
            {
                framePosition = 0f;
                if (IsCasting) SetFrame(castingFrames, 0);
            }
        }

        private void LateUpdate()
        {
            if (Time.deltaTime <= 0f) return;
            bool wasCasting = IsCasting;
            if (lastShotFrame != Time.frameCount) castRemaining = Mathf.Max(0f, castRemaining - Time.deltaTime);
            bool casting = IsCasting;
            if (wasCasting && !casting) framePosition = 0f;
            Sprite[] activeFrames = casting ? castingFrames : frames;
            if (activeFrames == null || activeFrames.Length == 0) return;

            // Chỉ giơ tay một lần mỗi đợt bắn, giữ frame cuối thay vì lặp động tác.
            float nextFramePosition = framePosition
                + Time.deltaTime * (casting ? castingFramesPerSecond : framesPerSecond);
            framePosition = casting
                ? Mathf.Min(nextFramePosition, activeFrames.Length - 1)
                : Mathf.Repeat(nextFramePosition, activeFrames.Length);
            SetFrame(activeFrames, Mathf.FloorToInt(framePosition));

            if (!canMoveVisual) return;

            float phase = framePosition / activeFrames.Length * Mathf.PI * 2f;
            float strength = controller != null && controller.IsFocusMode ? 0f : 1f;
            transform.localPosition = restPosition
                + Vector3.up * (Mathf.Sin(phase) * bobAmplitude * strength);
            float facing = bodyRenderer.flipX ? -1f : 1f;
            transform.localRotation = restRotation
                * Quaternion.Euler(0f, 0f, Mathf.Sin(phase) * rockAngle * strength * facing);
        }

        private void SetFrame(Sprite[] source, int index)
        {
            if (bodyRenderer == null || source == null || source.Length == 0) return;
            Sprite frame = source[index];
            if (frame != null && bodyRenderer.sprite != frame)
                bodyRenderer.sprite = frame;
        }

        private void OnDisable()
        {
            castRemaining = 0f;
            if (!canMoveVisual) return;
            transform.localPosition = restPosition;
            transform.localRotation = restRotation;
        }
    }
}
