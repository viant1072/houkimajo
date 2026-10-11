using UnityEngine;
using Houkimajo.Bullets;

namespace Houkimajo.Boss
{
    /// <summary>
    /// Định nghĩa 1 pha của boss: ngưỡng % HP để kích hoạt, pattern đạn dùng trong pha đó,
    /// và thời lượng tối đa (hết giờ thì ép chuyển pha kế dù HP chưa tới ngưỡng).
    /// Tạo asset qua: Create → Houkimajo → Boss Phase, lưu vào Assets/_Game/Data/BossPhases.
    /// </summary>
    [CreateAssetMenu(fileName = "BossPhase", menuName = "Houkimajo/Boss Phase")]
    public class BossPhaseSO : ScriptableObject
    {
        public string phaseName = "Phase";

        [Tooltip("Pha này kích hoạt khi HP boss xuống dưới % này (0 = chết, 1 = full máu)")]
        [Range(0f, 1f)] public float hpThreshold = 1f;

        public BulletPatternSO bulletPattern;

        [Tooltip("Thời gian tối đa của pha (giây)")]
        public float maxDuration = 20f;
    }
}
