using UnityEngine;

namespace Houkimajo.Systems
{
    /// <summary>
    /// Màn đang chơi, giữ qua lần load scene. Mở thẳng scene Stage01 trong Editor rồi bấm
    /// Play thì mặc định chơi màn đầu tiên.
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
