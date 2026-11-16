using System;
using System.Collections;
using UnityEngine;
using Houkimajo.Bullets;
using Houkimajo.Systems;

namespace Houkimajo.Enemies
{
    /// <summary>
    /// Chạy 1 WaveSO: spawn quái ngẫu nhiên ngay ngoài 4 mép màn hình, đếm số quái còn lại,
    /// phát OnWaveCleared khi đã spawn hết và không còn con nào. Không tự biết thứ tự các
    /// wave — việc đó là của StageDirector.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyPool enemyPool;
        [SerializeField] private PickupPool pickupPool;
        [SerializeField] private BulletPool enemyBulletPool;

        [Tooltip("Số quái tối đa cùng lúc trên màn; đông hơn thì chờ bớt mới spawn tiếp")]
        [SerializeField] private int maxConcurrentEnemies = 30;

        [Tooltip("Quái xuất hiện cách mép màn hình bao xa (đơn vị world)")]
        [SerializeField] private float spawnOutsideMargin = 0.8f;

        public event Action OnWaveCleared;

        private Transform player;
        private Coroutine waveRoutine;
        private bool spawningFinished;
        private int aliveCount;

        public bool IsRunning { get; private set; }
        public int AliveCount => aliveCount;

        private void Awake()
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null) player = playerGo.transform;
        }

        public void StartWave(WaveSO wave)
        {
            StopWave();
            if (wave == null || enemyPool == null)
            {
                Debug.LogWarning("[Enemies] Thiếu WaveSO hoặc EnemyPool — bỏ qua wave.");
                OnWaveCleared?.Invoke();
                return;
            }

            IsRunning = true;
            spawningFinished = false;
            aliveCount = 0;
            waveRoutine = StartCoroutine(RunWave(wave));
        }

        /// <summary>Dừng spawn và thu hồi mọi quái (thua / kết thúc màn).</summary>
        public void StopWave()
        {
            if (waveRoutine != null) StopCoroutine(waveRoutine);
            waveRoutine = null;
            IsRunning = false;
            if (enemyPool != null) enemyPool.DespawnAll();
            aliveCount = 0;
        }

        private IEnumerator RunWave(WaveSO wave)
        {
            if (wave.startDelay > 0f) yield return new WaitForSeconds(wave.startDelay);

            foreach (var entry in wave.entries)
            {
                if (entry == null || entry.enemy == null) continue;

                for (int i = 0; i < entry.count; i++)
                {
                    // Chờ bớt quái (hoặc pool có chỗ) rồi mới spawn tiếp.
                    EnemyController enemy = null;
                    while (true)
                    {
                        if (aliveCount < maxConcurrentEnemies)
                        {
                            enemy = enemyPool.Get(RandomEdgePosition());
                            if (enemy != null) break;
                        }
                        yield return null;
                    }

                    aliveCount++;
                    enemy.Initialize(entry.enemy, this, player, enemyBulletPool, pickupPool);

                    if (entry.spawnInterval > 0f) yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            spawningFinished = true;
            waveRoutine = null;
            CheckCleared();
        }

        /// <summary>Quái gọi khi chết hoặc bay mất.</summary>
        public void NotifyEnemyGone(EnemyController enemy)
        {
            if (enemyPool != null) enemyPool.Return(enemy);
            aliveCount = Mathf.Max(0, aliveCount - 1);
            CheckCleared();
        }

        private void CheckCleared()
        {
            if (!IsRunning || !spawningFinished || aliveCount > 0) return;
            IsRunning = false;
            OnWaveCleared?.Invoke();
        }

        private Vector2 RandomEdgePosition()
        {
            Rect arena = ArenaBounds.Get();
            float m = spawnOutsideMargin;
            switch (UnityEngine.Random.Range(0, 4))
            {
                case 0: return new Vector2(UnityEngine.Random.Range(arena.xMin, arena.xMax), arena.yMax + m); // trên
                case 1: return new Vector2(UnityEngine.Random.Range(arena.xMin, arena.xMax), arena.yMin - m); // dưới
                case 2: return new Vector2(arena.xMin - m, UnityEngine.Random.Range(arena.yMin, arena.yMax)); // trái
                default: return new Vector2(arena.xMax + m, UnityEngine.Random.Range(arena.yMin, arena.yMax)); // phải
            }
        }
    }
}
