using System;
using UnityEngine;

namespace Houkimajo.Systems
{
    /// <summary>Vàng của player trong stage, để progression/shop nối vào sau.</summary>
    public class Wallet : MonoBehaviour
    {
        [SerializeField] private int gold;
        public int Gold => gold;
        public event Action<int> OnGoldChanged;
        /// <summary>Nạp số vàng từ file save lúc vào màn.</summary>
        public void SetGold(int amount)
        {
            gold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(gold);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            gold += amount;
            OnGoldChanged?.Invoke(gold);
        }
    }
}
