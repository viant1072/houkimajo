using System.Collections.Generic;
using UnityEngine;
using Houkimajo.Systems;

namespace Houkimajo.Enemies
{
    /// <summary>Pool viên vàng rớt ra từ quái — không Instantiate/Destroy giữa trận.</summary>
    public class PickupPool : MonoBehaviour
    {
        [SerializeField] private Pickup pickupPrefab;
        [SerializeField] private int initialSize = 120;

        [Tooltip("Mỗi viên vàng tối đa bao nhiêu đồng — quái rớt nhiều thì tách thành nhiều viên cho đã mắt")]
        [SerializeField] private int maxGoldPerPickup = 3;

        private readonly Queue<Pickup> pool = new Queue<Pickup>();
        private readonly List<Pickup> active = new List<Pickup>();
        private Transform player;
        private Wallet wallet;

        private void Awake()
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
            {
                player = playerGo.transform;
                wallet = playerGo.GetComponent<Wallet>();
            }

            if (pickupPrefab == null) return;
            for (int i = 0; i < initialSize; i++)
            {
                var pickup = Instantiate(pickupPrefab, transform);
                pickup.gameObject.SetActive(false);
                pool.Enqueue(pickup);
            }
        }

        public void SpawnGold(Vector2 position, int gold)
        {
            while (gold > 0)
            {
                int amount = Mathf.Min(gold, maxGoldPerPickup);
                gold -= amount;

                if (pool.Count == 0)
                {
                    // Pool cạn: cộng thẳng vào ví thay vì mất tiền của người chơi.
                    if (wallet != null) wallet.AddGold(amount);
                    continue;
                }

                var pickup = pool.Dequeue();
                pickup.transform.position = position + Random.insideUnitCircle * 0.3f;
                pickup.PoolIndex = active.Count;
                active.Add(pickup);
                pickup.Initialize(this, player, wallet, amount);
                pickup.gameObject.SetActive(true);
            }
        }

        /// <summary>Mọi viên vàng còn trên màn bay về player (cuối wave).</summary>
        public void AttractAll()
        {
            for (int i = 0; i < active.Count; i++) active[i].Attract();
        }

        public void Return(Pickup pickup)
        {
            if (pickup.PoolIndex < 0) return;

            int index = pickup.PoolIndex;
            int lastIndex = active.Count - 1;
            var last = active[lastIndex];
            active[index] = last;
            last.PoolIndex = index;
            active.RemoveAt(lastIndex);

            pickup.PoolIndex = -1;
            pickup.gameObject.SetActive(false);
            pool.Enqueue(pickup);
        }
    }
}
