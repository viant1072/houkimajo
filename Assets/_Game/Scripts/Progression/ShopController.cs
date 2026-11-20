using System;
using System.Collections.Generic;
using Houkimajo.Player;
using Houkimajo.Skills;
using Houkimajo.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Houkimajo.Progression
{
    /// <summary>Điều phối shop thử, input và lưu tiền/túi; không sở hữu UI hay asset item.</summary>
    [RequireComponent(typeof(Wallet), typeof(HpPotionInventory), typeof(PlayerHealth))]
    public class ShopController : MonoBehaviour
    {
        [SerializeField, Tooltip("Seed vàng một lần cho tài khoản thử; tắt khi kết thúc prototype")]
        private bool seedTestGold = true;
        [SerializeField, Min(0)] private int testGold = 10000;
        [SerializeField] private bool openAtStageStart = true;
        private Wallet wallet;
        private PlayerHealth health;
        private HpPotionInventory inventory;
        private SkillCaster caster;
        private readonly Dictionary<string, int> purchasedLevels = new Dictionary<string, int>();
        private readonly string[] equippedIds = new string[3];
        private bool upgrading;
        private bool restoringLevel;
        private bool initialized;
        public bool IsOpen { get; private set; }
        public bool OpenAtStageStart => openAtStageStart;
        public bool IsInitialized => initialized;
        public string Message { get; private set; } = "Bình HP hồi ngay 40 HP. Nhấn B để mở/đóng cửa hàng.";
        public event Action OnChanged;

        private void Awake()
        {
            wallet = GetComponent<Wallet>();
            health = GetComponent<PlayerHealth>();
            inventory = GetComponent<HpPotionInventory>();
            caster = GetComponent<SkillCaster>();
        }

        private void OnEnable()
        {
            inventory.OnCooldownCompleted += Persist;
            if (caster != null) caster.OnLoadoutChanged += HandleLoadoutChanged;
        }

        private void OnDisable()
        {
            inventory.OnCooldownCompleted -= Persist;
            if (caster != null) caster.OnLoadoutChanged -= HandleLoadoutChanged;
            if (IsOpen && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Shop)
                GameManager.Instance.SetState(GameState.Playing);
            IsOpen = false;
        }

        public void Initialize()
        {
            if (initialized) return;
            var save = SaveSystem.Load();
            inventory.Restore(save);
            purchasedLevels.Clear();
            if (save.skillUpgrades != null)
                foreach (var entry in save.skillUpgrades)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.skillId)) continue;
                    purchasedLevels.TryGetValue(entry.skillId, out int previous);
                    purchasedLevels[entry.skillId] = Mathf.Max(previous, Mathf.Clamp(entry.level, 1, 5));
                }
            for (int i = 0; i < 3; i++) RestoreLevel((SkillSlot)i);
            if (seedTestGold && !save.shopTestGoldSeeded)
            {
                wallet.SetGold(Mathf.Max(wallet.Gold, testGold));
                save.shopTestGoldSeeded = true;
            }
            initialized = true;
            Persist();
            OnChanged?.Invoke();
        }

        public void Capture(SaveData save)
        {
            if (!initialized) return;
            inventory.Capture(save);
            CaptureLevels(save);
        }

        private void Persist()
        {
            if (!initialized) return;
            var save = SaveSystem.Load();
            save.gold = wallet.Gold;
            inventory.Capture(save);
            CaptureLevels(save);
            if (!SaveSystem.Save(save))
            {
                Message = "Chưa lưu được thay đổi xuống đĩa. Hãy kiểm tra quyền ghi file.";
                OnChanged?.Invoke();
            }
        }

        private void Update()
        {
            if (!initialized || Keyboard.current == null || GameManager.Instance == null) return;
            var keyboard = Keyboard.current;
            if (IsOpen)
            {
                if (keyboard.bKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame) Close();
                return;
            }
            if (GameManager.Instance.CurrentState != GameState.Playing || Time.timeScale <= 0f) return;
            if (keyboard.bKey.wasPressedThisFrame) { Open(); return; }
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) UsePotion();
        }

        public void Open()
        {
            if (!initialized || IsOpen || health.CurrentHP <= 0 || GameManager.Instance == null
                || GameManager.Instance.CurrentState != GameState.Playing) return;
            IsOpen = true;
            Message = "Chọn một ô trong cửa hàng hoặc túi đồ để xem thông tin.";
            GameManager.Instance.SetState(GameState.Shop);
            Cursor.visible = true;
            OnChanged?.Invoke();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Persist();
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Shop)
                GameManager.Instance.SetState(GameState.Playing);
            OnChanged?.Invoke();
        }

        public void BuyPotion()
        {
            if (!initialized || !IsOpen) return;
            bool changed = inventory.TryPurchase(wallet, out string message);
            Message = message;
            if (changed) Persist();
            OnChanged?.Invoke();
        }

        public void UsePotion()
        {
            if (!initialized) return;
            bool changed = inventory.TryUse(out string message);
            Message = message;
            if (changed) Persist();
            OnChanged?.Invoke();
        }

        public void SellPotion()
        {
            if (!initialized || !IsOpen) return;
            bool changed = inventory.TrySell(wallet, out string message);
            Message = message;
            if (changed) Persist();
            OnChanged?.Invoke();
        }

        public bool GetUpgradeQuote(SkillSlot slot, out int cost)
        {
            cost = 0;
            if (caster == null) return false;
            var skill = caster.GetSkill(slot);
            int level = caster.GetLevel(slot);
            if (skill == null || skill is DashSkillSO || string.IsNullOrEmpty(skill.id) || level >= skill.MaxLevel) return false;
            var next = skill.GetLevel(level + 1);
            if (next == null || next.upgradeCost < 0) return false;
            cost = next.upgradeCost;
            return true;
        }

        public void UpgradeSkill(SkillSlot slot)
        {
            if (!initialized || !IsOpen || upgrading || GameManager.Instance == null
                || GameManager.Instance.CurrentState != GameState.Shop) return;
            if (!GetUpgradeQuote(slot, out int cost))
            {
                Message = "Kỹ năng chưa lắp hoặc đã đạt cấp tối đa.";
                OnChanged?.Invoke(); return;
            }
            var skill = caster.GetSkill(slot);
            upgrading = true;
            try
            {
                if (!wallet.TrySpend(cost)) Message = "Không đủ vàng để nâng cấp kỹ năng.";
                else if (!caster.UpgradeEquippedSkill(slot))
                {
                    wallet.AddGold(cost);
                    Message = "Không thể nâng cấp kỹ năng lúc này.";
                }
                else
                {
                    purchasedLevels[skill.id] = caster.GetLevel(slot);
                    Message = "Đã nâng " + skill.displayName + " lên cấp " + caster.GetLevel(slot) + ".";
                    Persist();
                }
            }
            finally { upgrading = false; }
            OnChanged?.Invoke();
        }

        private void HandleLoadoutChanged(SkillSlot slot)
        {
            if (!initialized || restoringLevel || caster == null) return;
            var skill = caster.GetSkill(slot);
            if (equippedIds[(int)slot] != (skill != null ? skill.id : null)) RestoreLevel(slot);
            OnChanged?.Invoke();
        }

        private void RestoreLevel(SkillSlot slot)
        {
            if (caster == null) return;
            var skill = caster.GetSkill(slot);
            equippedIds[(int)slot] = skill != null ? skill.id : null;
            if (skill == null || string.IsNullOrEmpty(skill.id) || skill.MaxLevel <= 0
                || !purchasedLevels.TryGetValue(skill.id, out int level)) return;
            restoringLevel = true;
            try { caster.Equip(slot, skill, Mathf.Clamp(level, 1, skill.MaxLevel)); }
            finally { restoringLevel = false; }
        }

        private void CaptureLevels(SaveData save)
        {
            if (save.skillUpgrades == null) save.skillUpgrades = new List<SkillUpgradeSave>();
            save.skillUpgrades.Clear();
            foreach (var pair in purchasedLevels)
                save.skillUpgrades.Add(new SkillUpgradeSave { skillId = pair.Key, level = pair.Value });
        }
    }
}
