using System;
using Houkimajo.Player;
using Houkimajo.Systems;
using UnityEngine;

namespace Houkimajo.Progression
{
    /// <summary>Túi một loại bình cho prototype, cấu hình trong scene thay vì tạo asset item.</summary>
    public class HpPotionInventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int price = 60;
        [SerializeField, Min(1)] private int healAmount = 40;
        [SerializeField, Min(1)] private int maxStack = 9;
        [SerializeField, Min(0f)] private float useCooldown = 2f;
        private PlayerHealth health;
        private int count;
        private float remaining;
        private bool busy;
        private int displayedSeconds;

        public int Count => count;
        public int Price => Mathf.Max(1, price);
        public int HealAmount => Mathf.Max(1, healAmount);
        public int MaxStack => Mathf.Max(1, maxStack);
        public float CooldownRemaining => remaining;
        public bool CanUse => !busy && count > 0 && remaining <= 0f && health != null
            && health.CurrentHP > 0 && health.CurrentHP < health.MaxHP;
        public event Action OnChanged;
        public event Action OnCooldownCompleted;

        private void Awake() { health = GetComponent<PlayerHealth>(); }

        public void Restore(SaveData save)
        {
            count = Mathf.Clamp(save.hpPotionCount, 0, MaxStack);
            remaining = float.IsNaN(save.hpPotionCooldown) || float.IsInfinity(save.hpPotionCooldown)
                ? 0f : Mathf.Clamp(save.hpPotionCooldown, 0f, Mathf.Max(0f, useCooldown));
            displayedSeconds = Mathf.CeilToInt(remaining);
            OnChanged?.Invoke();
        }

        public void Capture(SaveData save)
        {
            save.hpPotionCount = count;
            save.hpPotionCooldown = remaining;
        }

        private void Update()
        {
            if (remaining <= 0f || GameManager.Instance == null) return;
            var state = GameManager.Instance.CurrentState;
            float delta = state == GameState.Shop ? Time.unscaledDeltaTime
                : state == GameState.Playing ? Time.deltaTime : 0f;
            if (delta <= 0f) return;
            remaining = Mathf.Max(0f, remaining - delta);
            int seconds = Mathf.CeilToInt(remaining);
            if (seconds == displayedSeconds) return;
            displayedSeconds = seconds;
            OnChanged?.Invoke();
            if (remaining <= 0f) OnCooldownCompleted?.Invoke();
        }

        public bool TryPurchase(Wallet wallet, out string message)
        {
            message = "Chỉ mua được khi cửa hàng đang mở.";
            if (busy || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Shop) return false;
            if (count >= MaxStack) { message = "Túi đã đầy (" + MaxStack + "/" + MaxStack + ")."; return false; }
            if (wallet == null || wallet.Gold < Price) { message = "Không đủ vàng."; return false; }
            busy = true;
            try
            {
                // Khóa giao dịch trước khi event Wallet có thể gọi lại thao tác mua.
                if (!wallet.TrySpend(Price)) { message = "Không đủ vàng."; return false; }
                count++;
                message = "Đã mua 1 Bình HP. Gán vào ô nhanh [1].";
            }
            finally { busy = false; }
            OnChanged?.Invoke();
            return true;
        }

        public bool TryUse(out string message)
        {
            message = "Không thể dùng bình lúc này.";
            if (busy || GameManager.Instance == null) return false;
            var state = GameManager.Instance.CurrentState;
            if (state != GameState.Playing && state != GameState.Shop) return false;
            if (health == null || health.CurrentHP <= 0) { message = "Không thể dùng bình khi đã hết HP."; return false; }
            if (count <= 0) { message = "Chưa có Bình HP. Nhấn B để mở cửa hàng."; return false; }
            if (remaining > 0f) { message = "Bình HP đang hồi dùng."; return false; }
            if (health.CurrentHP >= health.MaxHP) { message = "HP đã đầy, không tiêu hao bình."; return false; }
            busy = true;
            try
            {
                int restored = health.Heal(HealAmount);
                if (restored <= 0) return false;
                count--;
                remaining = Mathf.Max(0f, useCooldown);
                displayedSeconds = Mathf.CeilToInt(remaining);
                message = "Hồi +" + restored + " HP. Còn " + count + " bình.";
            }
            finally { busy = false; }
            OnChanged?.Invoke();
            return true;
        }

        public bool TrySell(Wallet wallet, out string message)
        {
            message = "Chỉ bán được khi cửa hàng đang mở.";
            if (busy || wallet == null || GameManager.Instance == null
                || GameManager.Instance.CurrentState != GameState.Shop) return false;
            if (count <= 0) { message = "Không có Bình HP để bán."; return false; }
            busy = true;
            try
            {
                count--;
                int proceeds = Price / 2;
                wallet.AddGold(proceeds);
                message = "Đã bán 1 Bình HP (+" + proceeds + " vàng).";
            }
            finally { busy = false; }
            OnChanged?.Invoke();
            return true;
        }
    }
}
