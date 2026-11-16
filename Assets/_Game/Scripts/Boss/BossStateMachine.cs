using System.Collections.Generic;
using UnityEngine;
using Houkimajo.Bullets;

namespace Houkimajo.Boss
{
    /// <summary>
    /// Quản lý chuyển pha của boss dựa theo % máu còn lại (hoặc hết giờ pha).
    /// Mỗi pha đổi kiểu bắn thông qua BulletEmitter.SetPattern() — thêm pha mới = thêm
    /// phần tử vào list "phases" trong Inspector, không cần sửa code.
    /// </summary>
    public class BossStateMachine : MonoBehaviour
    {
        [SerializeField] private List<BossPhaseSO> phases = new List<BossPhaseSO>();
        [SerializeField] private BulletEmitter emitter;
        [SerializeField] private BossHealth health;

        private int currentPhaseIndex = -1;
        private float phaseTimer;

        private bool started;

        private void Start()
        {
            started = true;
            if (phases.Count > 0 && currentPhaseIndex < 0)
                EnterPhase(0);
        }

        /// <summary>
        /// Nạp bộ pha của màn hiện tại (StageDirector gọi lúc boss xuất hiện). Thay list
        /// pha bằng data, nên mỗi màn có boss khác nhau mà không sửa code.
        /// </summary>
        public void Configure(List<BossPhaseSO> newPhases)
        {
            phases = newPhases != null ? new List<BossPhaseSO>(newPhases) : new List<BossPhaseSO>();
            currentPhaseIndex = -1;

            // Start() đã chạy rồi thì tự vào pha đầu; chưa chạy thì để Start() lo.
            if (started && phases.Count > 0)
                EnterPhase(0);
        }

        private void Update()
        {
            if (currentPhaseIndex < 0 || currentPhaseIndex >= phases.Count) return;

            phaseTimer -= Time.deltaTime;

            int nextIndex = currentPhaseIndex + 1;
            bool hasNextPhase = nextIndex < phases.Count;
            if (!hasNextPhase) return;

            float hpRatio = health.MaxHP > 0 ? health.CurrentHP / (float)health.MaxHP : 0f;
            bool reachedHpThreshold = hpRatio <= phases[nextIndex].hpThreshold;
            bool ranOutOfTime = phaseTimer <= 0f;

            if (reachedHpThreshold || ranOutOfTime)
            {
                EnterPhase(nextIndex);
            }
        }

        private void EnterPhase(int index)
        {
            currentPhaseIndex = index;
            var phase = phases[index];
            phaseTimer = phase.maxDuration;
            emitter.SetPattern(phase.bulletPattern);

            Debug.Log($"[Boss] Chuyển sang {phase.phaseName}");
        }
    }
}
