using UnityEngine;
using UnityEngine.InputSystem;
using Houkimajo.Systems;

namespace Houkimajo.Player
{
    /// <summary>
    /// Điều khiển di chuyển của cô phù thủy trong arena 1 màn hình. WASD hoặc phím mũi
    /// tên để di chuyển, Left Control để focus. PlayerDash nhận Left Shift để lướt.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Tốc độ di chuyển")]
        [SerializeField] private float normalSpeed = 6f;
        [SerializeField] private float focusSpeed = 2.5f;

        [Tooltip("Khoảng cách tối thiểu tới mép màn hình (đơn vị world), để nhân vật không bị khuất nửa người")]
        [SerializeField] private float edgeMargin = 0.3f;

        private Vector2 dashVelocity;
        private float dashTimeRemaining;
        private PlayerHealth health;

        private void Awake() { health = GetComponent<PlayerHealth>(); }

        public bool IsFocusMode { get; private set; }

        /// <summary>Hướng phím di chuyển đang giữ (chưa chuẩn hoá, có thể = 0).</summary>
        public Vector2 MoveInput { get; private set; }

        public bool IsDashing => dashTimeRemaining > 0f;
        public float SkillSpeedMultiplier { get; set; } = 1f;

        private void Update()
        {
            if (Time.timeScale <= 0f || health.CurrentHP <= 0
                || (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing))
            { MoveInput = Vector2.zero; IsFocusMode = false; return; }
            ReadInput();

            Vector2 delta;
            if (IsDashing)
            {
                float step = Mathf.Min(Time.deltaTime, dashTimeRemaining);
                delta = dashVelocity * step;
                dashTimeRemaining -= Time.deltaTime;
            }
            else
            {
                float speed = IsFocusMode ? focusSpeed : normalSpeed;
                delta = MoveInput.normalized * (speed * SkillSpeedMultiplier * Time.deltaTime);
            }

            transform.position = ArenaBounds.Clamp((Vector2)transform.position + delta, edgeMargin);
        }

        private void ReadInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                MoveInput = Vector2.zero;
                IsFocusMode = false;
                return;
            }

            Vector2 input = Vector2.zero;
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) input.x -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) input.y -= 1f;

            MoveInput = input;
            IsFocusMode = keyboard.leftCtrlKey.isPressed;
        }

        /// <summary>
        /// Lướt đi <paramref name="distance"/> đơn vị theo <paramref name="direction"/> trong
        /// <paramref name="duration"/> giây. Vẫn bị kẹp trong arena (lướt vào tường thì dừng).
        /// </summary>
        public void Dash(Vector2 direction, float distance, float duration)
        {
            if (direction.sqrMagnitude < 0.0001f || duration <= 0f) return;

            dashVelocity = direction.normalized * (distance / duration);
            dashTimeRemaining = duration;
        }

        public void TeleportTo(Vector2 target, float maxDistance)
        {
            Vector2 delta = Vector2.ClampMagnitude(target - (Vector2)transform.position, maxDistance);
            dashTimeRemaining = 0f;
            transform.position = ArenaBounds.Clamp((Vector2)transform.position + delta, edgeMargin);
        }

        public void CancelDash() { dashTimeRemaining = 0f; dashVelocity = Vector2.zero; }
    }
}
