using UnityEngine;
using UnityEngine.InputSystem;

namespace Houkimajo.Player
{
    /// <summary>
    /// Đọc vị trí chuột, đổi sang world space và tính hướng ngắm từ player tới con trỏ.
    /// Gom về 1 chỗ để bắn thường, skill, hiển thị (lật sprite) dùng chung một hướng
    /// ngắm, không mỗi script tự đọc chuột một kiểu.
    /// </summary>
    public class PlayerAim : MonoBehaviour
    {
        [Tooltip("Sprite tâm ngắm đi theo chuột (thay con trỏ hệ thống khi đang chơi)")]
        [SerializeField] private Transform crosshair;

        private Camera cam;

        /// <summary>Vị trí con trỏ chuột trong world space.</summary>
        public Vector2 AimPoint { get; private set; }

        /// <summary>Hướng (đã chuẩn hoá) từ player tới con trỏ.</summary>
        public Vector2 AimDirection { get; private set; } = Vector2.up;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            UpdateCursorVisibility();

            var mouse = Mouse.current;
            if (mouse == null || cam == null) return;

            Vector3 screenPos = mouse.position.ReadValue();
            AimPoint = cam.ScreenToWorldPoint(screenPos);

            Vector2 toCursor = AimPoint - (Vector2)transform.position;
            // Chuột nằm ngay trên người player -> giữ hướng cũ, tránh chuẩn hoá vector ~0
            // làm hướng bắn giật loạn.
            if (toCursor.sqrMagnitude > 0.01f)
                AimDirection = toCursor.normalized;

            if (crosshair != null)
                crosshair.position = AimPoint;
        }

        /// <summary>
        /// Ẩn con trỏ hệ thống khi đang chơi (đã có tâm ngắm), hiện lại khi game dừng
        /// (game over, pause, shop) để còn bấm nút được.
        /// </summary>
        private void UpdateCursorVisibility()
        {
            bool shouldShowCursor = crosshair == null || Time.timeScale <= 0f;
            if (Cursor.visible != shouldShowCursor)
                Cursor.visible = shouldShowCursor;

            if (crosshair != null && crosshair.gameObject.activeSelf == shouldShowCursor)
                crosshair.gameObject.SetActive(!shouldShowCursor);
        }

        private void OnDisable()
        {
            Cursor.visible = true;
        }
    }
}
