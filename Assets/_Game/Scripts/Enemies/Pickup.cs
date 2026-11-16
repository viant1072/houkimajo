using UnityEngine;
using Houkimajo.Systems;

namespace Houkimajo.Enemies
{
    /// <summary>
    /// Viên vàng rớt ra khi quái chết. Không dùng physics: mỗi frame chỉ so khoảng cách tới
    /// player (rẻ hơn collider khi có cả trăm viên). Vào bán kính hút thì bay về player,
    /// chạm thì cộng vàng.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        [Tooltip("Player vào gần bao nhiêu thì viên vàng bắt đầu bay về (đơn vị world)")]
        [SerializeField] private float magnetRadius = 2f;

        [Tooltip("Khoảng cách coi như đã nhặt")]
        [SerializeField] private float collectRadius = 0.35f;

        [Tooltip("Tốc độ bay về player (đơn vị/giây), tăng dần theo thời gian bị hút")]
        [SerializeField] private float flySpeed = 9f;

        [Tooltip("Tự biến mất sau bao lâu nếu không nhặt (giây)")]
        [SerializeField] private float lifetime = 10f;

        private PickupPool ownerPool;
        private Transform player;
        private Wallet wallet;
        private int amount;
        private float age;
        private bool magnetized;

        public int PoolIndex { get; set; } = -1;

        public void Initialize(PickupPool pool, Transform playerTransform, Wallet playerWallet, int goldAmount)
        {
            ownerPool = pool;
            player = playerTransform;
            wallet = playerWallet;
            amount = goldAmount;
            age = 0f;
            magnetized = false;
        }

        /// <summary>Ép bay về player ngay (cuối wave), không cần lại gần.</summary>
        public void Attract()
        {
            magnetized = true;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            age += deltaTime;

            if (player == null)
            {
                if (age > lifetime) ReturnToPool();
                return;
            }

            Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
            float sqrDistance = toPlayer.sqrMagnitude;

            if (sqrDistance <= collectRadius * collectRadius)
            {
                if (wallet != null) wallet.AddGold(amount);
                ReturnToPool();
                return;
            }

            if (!magnetized && sqrDistance <= magnetRadius * magnetRadius) magnetized = true;

            if (magnetized)
            {
                // Càng bị hút lâu bay càng nhanh, để không bao giờ "đuổi không kịp" player.
                float speed = flySpeed + age * 4f;
                transform.position += (Vector3)(toPlayer.normalized * (speed * deltaTime));
            }
            else if (age > lifetime)
            {
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (ownerPool != null)
                ownerPool.Return(this);
            else
                gameObject.SetActive(false);
        }
    }
}
