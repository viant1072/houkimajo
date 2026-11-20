using System;
using Houkimajo.Skills;
using Houkimajo.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Houkimajo.Player
{
    /// <summary>Lướt mặc định bằng Left Shift, độc lập MP và ba ô kỹ năng.</summary>
    [DefaultExecutionOrder(10)]
    [RequireComponent(typeof(PlayerController), typeof(PlayerHealth), typeof(PlayerAim))]
    public class PlayerDash : MonoBehaviour
    {
        [SerializeField, Tooltip("Cấu hình/VFX Lướt Chổi cũ, chỉ dùng làm profile cho động tác mặc định")]
        private DashSkillSO dashProfile;
        [SerializeField, Range(1, 5)] private int level = 1;
        private PlayerController controller;
        private PlayerHealth health;
        private PlayerAim aim;
        private SkillEffectController effects;
        private int charges;
        private float remaining;
        private int lastTenths = -1;
        private readonly SkillLevelData fallback = new SkillLevelData { range = 3f, cooldown = 6f, charges = 1 };
        private SkillLevelData Data => dashProfile != null ? dashProfile.GetLevel(level) ?? fallback : fallback;
        public int Charges => charges;
        public int MaxCharges => Mathf.Max(1, Data.charges);
        public float CooldownRemaining => remaining;
        public float CooldownTotal => Mathf.Max(0.01f, Data.cooldown);
        public int Level => level;
        public event Action OnChanged;

        private void Awake()
        {
            controller = GetComponent<PlayerController>(); health = GetComponent<PlayerHealth>();
            aim = GetComponent<PlayerAim>(); effects = GetComponent<SkillEffectController>();
            if (dashProfile != null)
            {
                var upgrades = SaveSystem.Load().skillUpgrades;
                if (upgrades != null)
                    foreach (var entry in upgrades)
                        if (entry != null && entry.skillId == dashProfile.id) level = Mathf.Max(level, entry.level);
                level = Mathf.Clamp(level, 1, Mathf.Max(1, dashProfile.MaxLevel));
            }
            charges = MaxCharges;
        }

        private void Update()
        {
            if (!CanAct()) return;
            if (charges < MaxCharges)
            {
                remaining -= Time.deltaTime;
                while (remaining <= 0f && charges < MaxCharges)
                {
                    charges++;
                    remaining = charges < MaxCharges ? remaining + CooldownTotal : 0f;
                }
                int tenths = Mathf.CeilToInt(remaining * 10f);
                if (tenths != lastTenths) { lastTenths = tenths; OnChanged?.Invoke(); }
            }
            if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame) TryDash();
        }

        private bool CanAct()
        {
            return Time.timeScale > 0f && health.CurrentHP > 0
                && (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing);
        }

        public bool TryDash()
        {
            if (!CanAct() || controller.IsDashing || charges <= 0) return false;
            Vector2 direction = controller.MoveInput.sqrMagnitude > 0.01f ? controller.MoveInput.normalized : aim.AimDirection;
            float duration = dashProfile != null ? Mathf.Max(0.01f, dashProfile.dashDuration) : 0.18f;
            float extra = dashProfile != null ? Mathf.Max(0f, dashProfile.extraInvincibility) : 0.1f;
            controller.Dash(direction, Mathf.Max(0f, Data.range), duration);
            health.GrantInvincibility(duration + extra);
            if (dashProfile != null && effects != null) effects.ShowDash(dashProfile, Data, direction);
            charges--;
            if (remaining <= 0f) remaining = CooldownTotal;
            OnChanged?.Invoke();
            return true;
        }

        private void OnDisable()
        {
            if (controller != null) controller.CancelDash();
        }
    }
}
