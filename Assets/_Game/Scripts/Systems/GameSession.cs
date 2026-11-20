using UnityEngine;

namespace Houkimajo.Systems
{
    /// <summary>
    /// Màn đang chơi, giữ qua lần load scene. Khi test trực tiếp Stage01, chỉ số mặc định là 0.
    /// </summary>
    public static class GameSession
    {
        public static int CurrentStageIndex { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            CurrentStageIndex = 0;
        }
    }
}
