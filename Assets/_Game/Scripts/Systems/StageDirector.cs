using System;
using System.Collections;
using UnityEngine;
using Houkimajo.Boss;
using Houkimajo.Bullets;
using Houkimajo.Enemies;
using Houkimajo.Player;

namespace Houkimajo.Systems
{
    /// <summary>
    /// Điều phối 1 màn chơi: thông báo màn → các wave quái → boss → thắng (lưu + tự sang
    /// màn kế) hoặc thua (lưu vàng, chờ người chơi chọn chơi lại / về menu).
    /// Các module khác chỉ làm phần của mình; thứ tự diễn ra nằm hết ở đây.
    /// </summary>
    public class StageDirector : MonoBehaviour
    {
        [SerializeField] private StageCatalogSO catalog;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private PickupPool pickupPool;
        [SerializeField] private BulletPool enemyBulletPool;

        [Header("Boss")]
        [SerializeField] private GameObject bossObject;
        [SerializeField] private BossHealth bossHealth;
        [SerializeField] private BossStateMachine bossStateMachine;
        [SerializeField] private SpriteRenderer bossRenderer;

        [Header("Player")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Wallet wallet;

        [Header("Nhịp độ (giây)")]
        [SerializeField] private float stageIntroDuration = 2f;
        [SerializeField] private float waveIntroDuration = 1.5f;
        [SerializeField] private float pauseBetweenWaves = 1.5f;
        [SerializeField] private float bossIntroDuration = 1.5f;
        [SerializeField] private float delayBeforeNextStage = 3f;
        [SerializeField] private float delayAfterFinalStage = 4f;

        /// <summary>(màn, chỉ số màn, tổng số màn)</summary>
        public event Action<StageSO, int, int> OnStageStarted;

        /// <summary>(wave thứ mấy bắt đầu từ 1, tổng số wave)</summary>
        public event Action<int, int> OnWaveStarted;

        public event Action OnBossStarted;

        /// <summary>(có màn kế tiếp không)</summary>
        public event Action<bool> OnStageCleared;

        public event Action OnStageFailed;

        private StageSO stage;
        private bool waveCleared;
        private bool bossDefeated;
        private bool finished;

        public StageSO CurrentStage => stage;
        public int StageIndex { get; private set; }
        public int StageCount => catalog != null ? catalog.Count : 0;
        public int CurrentWave { get; private set; }
        public int WaveCount => stage != null ? stage.waves.Count : 0;
        public bool IsBossPhase { get; private set; }

        private void Awake()
        {
            // Boss nằm sẵn trong scene để chỉnh tay cho dễ, nhưng ẩn tới lượt boss.
            if (bossObject != null) bossObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (spawner != null) spawner.OnWaveCleared += HandleWaveCleared;
            if (bossHealth != null) bossHealth.OnDefeated += HandleBossDefeated;
            if (playerHealth != null) playerHealth.OnPlayerDied += HandlePlayerDied;
        }

        private void OnDisable()
        {
            if (spawner != null) spawner.OnWaveCleared -= HandleWaveCleared;
            if (bossHealth != null) bossHealth.OnDefeated -= HandleBossDefeated;
            if (playerHealth != null) playerHealth.OnPlayerDied -= HandlePlayerDied;
        }

        private void Start()
        {
            if (catalog == null || catalog.Count == 0)
            {
                Debug.LogError("[Stage] Chưa gán StageCatalog cho StageDirector — chạy menu Houkimajo → Setup Stages.");
                return;
            }

            StageIndex = Mathf.Clamp(GameSession.CurrentStageIndex, 0, catalog.Count - 1);
            stage = catalog.Get(StageIndex);

            if (wallet != null) wallet.SetGold(SaveSystem.Load().gold);
            GameManager.Instance?.SetState(GameState.Playing);

            StartCoroutine(RunStage());
        }

        private IEnumerator RunStage()
        {
            OnStageStarted?.Invoke(stage, StageIndex, catalog.Count);
            yield return new WaitForSeconds(stageIntroDuration);

            for (int i = 0; i < stage.waves.Count; i++)
            {
                var stageWave = stage.waves[i];
                if (stageWave == null || stageWave.wave == null) continue;

                CurrentWave = i + 1;
                OnWaveStarted?.Invoke(CurrentWave, stage.waves.Count);
                yield return new WaitForSeconds(waveIntroDuration);

                waveCleared = false;
                spawner.StartWave(stageWave.wave);
                while (!waveCleared) yield return null;

                // Hết wave: dọn đạn còn bay, gom vàng còn rơi rồi nghỉ một nhịp.
                CleanUpBattlefield();
                yield return new WaitForSeconds(pauseBetweenWaves);
            }

            yield return RunBoss();
            if (finished) yield break;

            yield return CompleteStage();
        }

        private IEnumerator RunBoss()
        {
            IsBossPhase = true;
            OnBossStarted?.Invoke();
            yield return new WaitForSeconds(bossIntroDuration);

            bossDefeated = false;
            if (bossObject != null)
            {
                bossObject.SetActive(true);
                if (bossRenderer != null) bossRenderer.color = stage.bossTint;
                bossHealth.ResetHealth(stage.bossMaxHP);
                bossStateMachine.Configure(stage.bossPhases);
            }
            else
            {
                Debug.LogWarning("[Stage] Không có boss trong scene — coi như đã hạ boss.");
                bossDefeated = true;
            }

            while (!bossDefeated && !finished) yield return null;
        }

        private IEnumerator CompleteStage()
        {
            finished = true;

            if (bossObject != null) bossObject.SetActive(false);
            CleanUpBattlefield();
            // Cho viên vàng cuối kịp bay về ví trước khi lưu.
            yield return new WaitForSeconds(0.8f);

            if (wallet != null) wallet.AddGold(stage.clearGold);

            var save = SaveSystem.Load();
            save.highestStageCleared = Mathf.Max(save.highestStageCleared, StageIndex);
            if (wallet != null) save.gold = wallet.Gold;
            SaveSystem.Save(save);

            bool hasNextStage = StageIndex + 1 < catalog.Count;
            GameManager.Instance?.SetState(GameState.Victory);
            OnStageCleared?.Invoke(hasNextStage);

            // timeScale = 0 lúc Victory -> phải đợi bằng thời gian thực.
            yield return new WaitForSecondsRealtime(hasNextStage ? delayBeforeNextStage : delayAfterFinalStage);

            if (hasNextStage)
                SceneFlow.LoadStage(StageIndex + 1);
            else
                SceneFlow.LoadMainMenu();
        }

        private void CleanUpBattlefield()
        {
            if (enemyBulletPool != null) enemyBulletPool.ClearAll();
            if (pickupPool != null) pickupPool.AttractAll();
        }

        private void HandleWaveCleared()
        {
            waveCleared = true;
        }

        private void HandleBossDefeated()
        {
            bossDefeated = true;
        }

        private void HandlePlayerDied()
        {
            if (finished) return;
            finished = true;
            StopAllCoroutines();

            if (spawner != null) spawner.StopWave();

            // Thua vẫn giữ vàng đã nhặt (kiểu RPG, không phạt nặng).
            var save = SaveSystem.Load();
            if (wallet != null) save.gold = wallet.Gold;
            SaveSystem.Save(save);

            GameManager.Instance?.SetState(GameState.GameOver);
            OnStageFailed?.Invoke();
        }

        /// <summary>Chơi lại màn hiện tại (từ màn Game Over).</summary>
        public void RetryStage()
        {
            SceneFlow.LoadStage(StageIndex);
        }
    }
}
