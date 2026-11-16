using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Houkimajo.Bullets;
using Houkimajo.Player;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Gắn trên Player: đọc phím Q/E/R, quản lý hồi chiêu + số lượt tích trữ của từng ô,
    /// gọi SkillSO.Activate(). Skill chỉ lo "làm gì", còn "khi nào được dùng" nằm hết ở đây
    /// để luật chung (global cooldown, dừng khi pause...) áp dụng đồng đều cho mọi skill.
    /// </summary>
    [RequireComponent(typeof(PlayerAim), typeof(PlayerController), typeof(PlayerHealth))]
    public class SkillCaster : MonoBehaviour
    {
        private const int SlotCount = 3;

        [SerializeField] private SkillLoadout loadout = new SkillLoadout();
        [SerializeField] private SkillPools pools;
        [SerializeField] private BulletPool enemyBulletPool;

        [Tooltip("Khoảng nghỉ tối thiểu giữa 2 skill bất kỳ (giây), tránh hiệu ứng chồng nhau khó đọc")]
        [SerializeField] private float globalCooldown = 0.15f;

        [Header("Debug (chỉ chạy trong Editor)")]
        [Tooltip("Danh sách skill để F1/F2/F3 đổi vòng ở ô Q/E/R khi test")]
        [SerializeField] private SkillSO[] debugSkillLibrary = new SkillSO[0];

        /// <summary>Dùng skill thành công.</summary>
        public event Action<SkillSlot> OnSkillCast;

        /// <summary>Bấm skill nhưng không dùng được (đang hồi, chưa lắp...) — HUD nháy đỏ.</summary>
        public event Action<SkillSlot> OnSkillFailed;

        /// <summary>Skill hoặc cấp ở 1 ô thay đổi — HUD cập nhật tên/cấp.</summary>
        public event Action<SkillSlot> OnLoadoutChanged;

        private readonly float[] cooldownRemaining = new float[SlotCount];
        private readonly int[] chargesAvailable = new int[SlotCount];
        private float globalCooldownRemaining;
        private SkillContext context;
        private GrazeDetector graze;
        private float ultimateEnergy;
        public float UltimateEnergy => ultimateEnergy;
        public event Action<float> OnUltimateEnergyChanged;

        private void Awake()
        {
            var runtime = GetComponent<SkillEffectController>();
            if (runtime == null) runtime = gameObject.AddComponent<SkillEffectController>();
            context = new SkillContext(transform, GetComponent<PlayerAim>(),
                GetComponent<PlayerController>(), GetComponent<PlayerHealth>(), pools, enemyBulletPool);
            runtime.Bind(context);
            graze = GetComponentInChildren<GrazeDetector>();
        }

        private void OnEnable()
        {
            DamageEvents.OnPlayerDamageDealt += HandleDamage;
            if (graze != null) graze.OnGraze += HandleGraze;
        }

        private void OnDisable()
        {
            DamageEvents.OnPlayerDamageDealt -= HandleDamage;
            if (graze != null) graze.OnGraze -= HandleGraze;
        }

        private void HandleDamage(int amount, DamageSource source)
        {
            if (source != DamageSource.Ultimate) AddUltimateEnergy(amount * 0.05f);
        }

        private void HandleGraze() { AddUltimateEnergy(1f); }

        public void AddUltimateEnergy(float amount)
        {
            ultimateEnergy = Mathf.Clamp(ultimateEnergy + amount, 0f, 100f);
            OnUltimateEnergyChanged?.Invoke(ultimateEnergy);
        }

        private void Start()
        {
            // F1 đổi skill không tạo pool mới giữa trận.
            foreach (var skill in debugSkillLibrary)
            {
                if (skill != null) skill.Prewarm(context);
            }
            for (int i = 0; i < SlotCount; i++)
            {
                ResetSlot((SkillSlot)i);
            }
        }

        private void Update()
        {
            // Game dừng (shop, pause, game over) -> không nhận phím skill.
            if (Time.timeScale <= 0f) return;

            TickCooldowns(Time.deltaTime);

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.qKey.wasPressedThisFrame) TryCast(SkillSlot.Q);
            if (keyboard.eKey.wasPressedThisFrame) TryCast(SkillSlot.E);
            if (keyboard.rKey.wasPressedThisFrame) TryCast(SkillSlot.R);

#if UNITY_EDITOR
            HandleDebugKeys(keyboard);
#endif
        }

        // ---------------- Truy vấn cho HUD ----------------

        public SkillSO GetSkill(SkillSlot slot) => loadout.GetSkill(slot);
        public int GetLevel(SkillSlot slot) => loadout.GetLevel(slot);
        public int GetCharges(SkillSlot slot) => slot == SkillSlot.R ? (ultimateEnergy >= 100f ? 1 : 0) : chargesAvailable[(int)slot];

        public int GetMaxCharges(SkillSlot slot)
        {
            var data = GetLevelData(slot);
            return data != null ? Mathf.Max(1, data.charges) : 0;
        }

        /// <summary>Số giây còn lại tới khi hồi thêm 1 lượt (0 = đã đầy lượt).</summary>
        public float GetCooldownRemaining(SkillSlot slot) => slot == SkillSlot.R ? 0f : cooldownRemaining[(int)slot];

        public float GetCooldownTotal(SkillSlot slot)
        {
            if (slot == SkillSlot.R) return 0f;
            var data = GetLevelData(slot);
            return data != null ? data.cooldown * (context != null ? context.CooldownMultiplier : 1f) : 0f;
        }

        // ---------------- Dùng skill ----------------

        public bool TryCast(SkillSlot slot)
        {
            if (Time.timeScale <= 0f) return false;
            int index = (int)slot;
            var skill = loadout.GetSkill(slot);
            var data = GetLevelData(slot);

            if (skill == null || data == null || GetCharges(slot) <= 0 || globalCooldownRemaining > 0f)
            {
                OnSkillFailed?.Invoke(slot);
                return false;
            }

            skill.Activate(context, data);

            if (slot == SkillSlot.R)
            {
                ultimateEnergy = 0f;
                OnUltimateEnergyChanged?.Invoke(ultimateEnergy);
            }
            else chargesAvailable[index]--;
            // Chỉ bắt đầu đếm hồi khi chưa đếm (đang hồi lượt khác thì giữ nguyên tiến độ).
            if (slot != SkillSlot.R && cooldownRemaining[index] <= 0f)
                cooldownRemaining[index] = GetCooldownTotal(slot);

            globalCooldownRemaining = globalCooldown;
            OnSkillCast?.Invoke(slot);
            return true;
        }

        /// <summary>Lắp skill vào ô (shop / debug). Ô được hồi đầy luôn.</summary>
        public void Equip(SkillSlot slot, SkillSO skill, int level)
        {
            loadout.Set(slot, skill, level);
            ResetSlot(slot);
            OnLoadoutChanged?.Invoke(slot);
        }

        private void TickCooldowns(float deltaTime)
        {
            if (globalCooldownRemaining > 0f) globalCooldownRemaining -= deltaTime;

            for (int i = 0; i < SlotCount; i++)
            {
                var slot = (SkillSlot)i;
                if (slot == SkillSlot.R) continue;
                int maxCharges = GetMaxCharges(slot);
                if (chargesAvailable[i] >= maxCharges) continue;

                cooldownRemaining[i] -= deltaTime;
                if (cooldownRemaining[i] > 0f) continue;

                chargesAvailable[i]++;
                // Còn thiếu lượt -> đếm tiếp lượt kế, cộng dồn phần thời gian dư của frame này.
                cooldownRemaining[i] = chargesAvailable[i] < maxCharges
                    ? cooldownRemaining[i] + GetCooldownTotal(slot)
                    : 0f;
            }
        }

        private void ResetSlot(SkillSlot slot)
        {
            int index = (int)slot;
            chargesAvailable[index] = GetMaxCharges(slot);
            cooldownRemaining[index] = 0f;

            var skill = loadout.GetSkill(slot);
            if (skill != null) skill.Prewarm(context);
        }

        private SkillLevelData GetLevelData(SkillSlot slot)
        {
            var skill = loadout.GetSkill(slot);
            return skill != null ? skill.GetLevel(loadout.GetLevel(slot)) : null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// F1/F2/F3: đổi vòng skill ở ô Q/E/R trong debugSkillLibrary.
        /// F5: tăng cấp mọi skill đang lắp thêm 1 (quá cấp tối đa thì quay về 1).
        /// Để test skill trước khi có shop/level.
        /// </summary>
        private void HandleDebugKeys(Keyboard keyboard)
        {
            if (keyboard.f1Key.wasPressedThisFrame) CycleDebugSkill(SkillSlot.Q);
            if (keyboard.f2Key.wasPressedThisFrame) CycleDebugSkill(SkillSlot.E);
            if (keyboard.f3Key.wasPressedThisFrame) CycleDebugSkill(SkillSlot.R);
            if (keyboard.f4Key.wasPressedThisFrame) AddUltimateEnergy(100f);

            if (keyboard.f5Key.wasPressedThisFrame)
            {
                for (int i = 0; i < SlotCount; i++)
                {
                    var slot = (SkillSlot)i;
                    var skill = loadout.GetSkill(slot);
                    if (skill == null) continue;

                    int next = loadout.GetLevel(slot) + 1;
                    if (next > skill.MaxLevel) next = 1;
                    Equip(slot, skill, next);
                }
                Debug.Log("[Skills] Debug: tăng cấp tất cả skill đang lắp.");
            }
        }

        private void CycleDebugSkill(SkillSlot slot)
        {
            var current = loadout.GetSkill(slot);
            int currentIndex = Array.IndexOf(debugSkillLibrary, current);

            // Tìm skill kế tiếp cùng ô trong thư viện, quay vòng.
            for (int step = 1; step <= debugSkillLibrary.Length; step++)
            {
                var candidate = debugSkillLibrary[(currentIndex + step + debugSkillLibrary.Length) % debugSkillLibrary.Length];
                if (candidate == null || candidate.slot != slot || candidate == current) continue;

                Equip(slot, candidate, loadout.GetLevel(slot));
                Debug.Log($"[Skills] Debug: ô {slot} = {candidate.displayName}");
                return;
            }

            Debug.Log($"[Skills] Debug: không có skill nào khác cho ô {slot}.");
        }
#endif
    }
}
