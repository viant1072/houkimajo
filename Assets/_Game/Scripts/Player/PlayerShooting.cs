using UnityEngine;
using UnityEngine.InputSystem;
using Houkimajo.Bullets;

namespace Houkimajo.Player
{
    /// <summary>
    /// Bắn đạn phép liên tục khi giữ phím Z (chuẩn Touhou), lấy đạn từ BulletPool
    /// thay vì Instantiate trực tiếp.
    /// </summary>
    public class PlayerShooting : MonoBehaviour
    {
        [SerializeField] private BulletPool bulletPool;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.1f;
        [SerializeField] private float bulletSpeed = 12f;

        private float cooldownTimer;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            cooldownTimer -= Time.deltaTime;

            if (keyboard.zKey.isPressed && cooldownTimer <= 0f)
            {
                Fire();
                cooldownTimer = fireRate;
            }
        }

        private void Fire()
        {
            if (bulletPool == null || firePoint == null) return;

            var bullet = bulletPool.Get(firePoint.position);
            bullet.Launch(Vector2.up, bulletSpeed, playerOwned: true);
        }
    }
}
