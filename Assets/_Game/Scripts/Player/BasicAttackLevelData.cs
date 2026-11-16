using System;
using UnityEngine;

namespace Houkimajo.Player
{
    /// <summary>Chỉ số bắn thường theo cấp, tách khỏi HUD và điều khiển.</summary>
    [Serializable]
    public class BasicAttackLevelData
    {
        [Min(1)] public int damage = 10;
        [Tooltip("Khoảng nghỉ giữa hai viên (giây)")]
        [Min(0.02f)] public float fireInterval = 0.1f;
        [Tooltip("Tốc độ đạn, đơn vị world/giây")]
        [Min(0.1f)] public float bulletSpeed = 12f;
    }
}
