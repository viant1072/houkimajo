using System;
using System.Collections.Generic;
using UnityEngine;

namespace Houkimajo.Enemies
{
    /// <summary>1 nhóm quái cùng loại trong wave.</summary>
    [Serializable]
    public class WaveEntry
    {
        public EnemyDataSO enemy;
        [Min(1)] public int count = 5;
        [Tooltip("Khoảng cách giữa 2 con (giây)")]
        public float spawnInterval = 0.6f;
    }

    /// <summary>
    /// 1 đợt quái: các nhóm spawn lần lượt theo thứ tự trong list. Tạo qua
    /// Create → Houkimajo → Wave, lưu ở Assets/_Game/Data/Waves.
    /// </summary>
    [CreateAssetMenu(fileName = "Wave", menuName = "Houkimajo/Wave")]
    public class WaveSO : ScriptableObject
    {
        [Tooltip("Chờ bao lâu (giây) rồi mới bắt đầu spawn")]
        public float startDelay = 0.5f;

        public List<WaveEntry> entries = new List<WaveEntry>();

        public int TotalCount
        {
            get
            {
                int total = 0;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i] != null && entries[i].enemy != null) total += entries[i].count;
                }
                return total;
            }
        }
    }
}
