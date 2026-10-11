using UnityEngine;
using UnityEngine.InputSystem;

namespace Houkimajo.Player
{
    /// <summary>
    /// Điều khiển di chuyển của cô phù thủy. Giữ Shift (focus mode) để bay chậm lại
    /// và né đạn chính xác hơn — đúng kiểu Touhou. WASD hoặc phím mũi tên để di chuyển.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Tốc độ di chuyển")]
        [SerializeField] private float normalSpeed = 6f;
        [SerializeField] private float focusSpeed = 2.5f;

        [Header("Giới hạn vùng chơi (world space)")]
        [SerializeField] private Vector2 boundsMin = new Vector2(-4f, -4.5f);
        [SerializeField] private Vector2 boundsMax = new Vector2(4f, 4.5f);

        public bool IsFocusMode { get; private set; }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            Vector2 input = Vector2.zero;
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) input.x -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) input.y -= 1f;

            IsFocusMode = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

            float speed = IsFocusMode ? focusSpeed : normalSpeed;
            Vector3 move = (Vector3)(input.normalized * speed * Time.deltaTime);

            Vector3 newPos = transform.position + move;
            newPos.x = Mathf.Clamp(newPos.x, boundsMin.x, boundsMax.x);
            newPos.y = Mathf.Clamp(newPos.y, boundsMin.y, boundsMax.y);
            transform.position = newPos;
        }
    }
}
