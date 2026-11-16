using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Houkimajo.Bullets;

namespace Houkimajo.Player
{
    /// <summary>
    /// Bắn đạn phép liên tục khi giữ Space, bay về phía con trỏ chuột (lấy hướng từ
    /// PlayerAim). Đạn lấy từ BulletPool thay vì Instantiate trực tiếp.
    /// </summary>
    [RequireComponent(typeof(PlayerAim))]
    public class PlayerShooting : MonoBehaviour
    {
        [SerializeField] private BulletPool bulletPool;
        [SerializeField] private BasicAttackSO basicAttack;
        [SerializeField, Min(1)] private int attackLevel = 1;

        public event Action OnAttackChanged;
        public bool CanFire { get; set; } = true;
        public bool SkillFireBlocked { get; set; }
        public BasicAttackSO Attack => basicAttack;
        public int AttackLevel => basicAttack != null ? Mathf.Clamp(attackLevel, 1, Mathf.Max(1, basicAttack.MaxLevel)) : 1;
        public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
        public float CooldownTotal => CurrentData != null ? Mathf.Max(0.02f, CurrentData.fireInterval) : Mathf.Max(0.02f, fireRate);
        public int CurrentDamage => CurrentData != null ? CurrentData.damage : bulletDamage;
        public float CurrentBulletSpeed => CurrentData != null ? CurrentData.bulletSpeed : bulletSpeed;
        private BasicAttackLevelData CurrentData => basicAttack != null ? basicAttack.GetLevel(AttackLevel) : null;

        [Tooltip("Thời gian giữa 2 viên đạn (giây)")]
        [SerializeField] private float fireRate = 0.1f;

        [SerializeField] private float bulletSpeed = 12f;

        [Tooltip("Sát thương mỗi viên. Sau này sẽ lấy từ PlayerStats (trang bị, level)")]
        [SerializeField] private int bulletDamage = 10;

        [Tooltip("Khoảng cách từ tâm player tới điểm đạn xuất hiện, theo hướng ngắm")]
        [SerializeField] private float muzzleOffset = 0.35f;

        private PlayerAim aim;
        private float cooldownTimer;

        private void Awake()
        {
            aim = GetComponent<PlayerAim>();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
            if (!CanFire || SkillFireBlocked) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.spaceKey.isPressed && cooldownTimer <= 0f)
            {
                if (Fire()) cooldownTimer = CooldownTotal;
            }
        }

        /// <summary>Đổi profile từ shop/trang bị, giữ mọi thay đổi trong instance player.</summary>
        public void EquipAttack(BasicAttackSO profile, int level = 1)
        {
            basicAttack = profile;
            attackLevel = profile != null ? Mathf.Clamp(level, 1, Mathf.Max(1, profile.MaxLevel)) : 1;
            cooldownTimer = 0f;
            OnAttackChanged?.Invoke();
        }

        public void SetAttackLevel(int level)
        {
            attackLevel = basicAttack != null ? Mathf.Clamp(level, 1, Mathf.Max(1, basicAttack.MaxLevel)) : 1;
            // Nâng cấp không cho bắn thêm ngay để tránh reset nhịp liên tục từ UI.
            cooldownTimer = Mathf.Min(cooldownTimer, CooldownTotal);
            OnAttackChanged?.Invoke();
        }

        private bool Fire()
        {
            if (bulletPool == null) return false;

            // Scene cũ dựng trước khi có PlayerAim sẽ thiếu component này -> bắn thẳng lên tạm.
            Vector2 direction = aim != null ? aim.AimDirection : Vector2.up;
            Vector2 spawnPos = (Vector2)transform.position + direction * muzzleOffset;

            var bullet = bulletPool.Get(spawnPos);
            if (bullet == null) return false;
            bullet.Launch(direction, CurrentBulletSpeed, playerOwned: true, damageOverride: CurrentDamage);
            bullet.SetAppearance(basicAttack != null ? basicAttack.projectileSprite : null,
                basicAttack != null ? basicAttack.projectileTint : Color.white);
            return true;
        }
    }
}
