using UnityEngine;

namespace Houkimajo.Enemies
{
    /// <summary>Phát sprite trên phần hình ảnh; không làm lệch collider hay đường bay của quái.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemySpriteAnimation : MonoBehaviour
    {
        [Tooltip("Các frame bay/idle theo thứ tự vòng lặp")]
        [SerializeField] private Sprite[] frames;

        [Tooltip("Số frame mỗi giây; dừng cùng gameplay khi Time.timeScale bằng 0")]
        [SerializeField, Min(0.1f)] private float framesPerSecond = 6f;

        private SpriteRenderer bodyRenderer;
        private float framePosition;

        private void Awake()
        {
            bodyRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            // Quái lấy lại từ pool bắt đầu bằng frame đầu, không giữ pose của lần trước.
            framePosition = 0f;
            SetFrame(0);
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        /// <summary>Dùng trực tiếp mảng frame từ data, không tạo bản sao khi spawn từ pool.</summary>
        public void Configure(Sprite[] animationFrames, float playbackRate)
        {
            frames = animationFrames;
            framesPerSecond = Mathf.Max(0.1f, playbackRate);
            framePosition = 0f;
            SetFrame(0);
        }

        public void Advance(float deltaTime)
        {
            if (frames == null || frames.Length == 0 || deltaTime <= 0f) return;
            framePosition = Mathf.Repeat(framePosition + deltaTime * Mathf.Max(0.1f, framesPerSecond),
                frames.Length);
            SetFrame(Mathf.FloorToInt(framePosition));
        }

        private void SetFrame(int index)
        {
            if (bodyRenderer == null || frames == null || frames.Length == 0) return;
            Sprite frame = frames[index];
            if (frame != null && bodyRenderer.sprite != frame) bodyRenderer.sprite = frame;
        }
    }
}
