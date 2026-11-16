using System;
using UnityEngine;
using Houkimajo.Bullets;

namespace Houkimajo.Enemies
{
    public enum EnemyMovement
    {
        Chase,  // đuổi thẳng theo player
        Orbit,  // tiến tới bán kính cố định rồi bay vòng quanh player
        Strafe  // bay xuyên qua màn hình theo 1 hướng rồi đi mất
    }

    /// <summary>Phần thưởng khi hạ 1 kẻ địch.</summary>
    [Serializable]
    public struct RewardData
    {
        public int gold;

        [Tooltip("Chưa dùng tới giai đoạn 3 (chưa có hệ level)")]
        public int xp;
    }

    /// <summary>
    /// Định nghĩa 1 loại quái thường. Tạo qua Create → Houkimajo → Enemy Data, lưu ở
    /// Assets/_Game/Data/Enemies. Thêm quái mới = tạo asset mới, không sửa code.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy", menuName = "Houkimajo/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Tooltip("Mã định danh cố định (dùng cho save / thống kê)")]
        public string id;
        public string displayName = "Enemy";

        [Header("Hình ảnh")]
        public Sprite sprite;
        [Tooltip("Các frame bay/idle; để trống để dùng sprite tĩnh")]
        public Sprite[] animationFrames;
        [Tooltip("Tốc độ animation, số frame mỗi giây")]
        [Min(0.1f)] public float animationFramesPerSecond = 6f;
        public Color tint = Color.white;
        [Tooltip("Bán kính collider (đơn vị world)")]
        public float colliderRadius = 0.25f;

        [Header("Chỉ số")]
        public int maxHP = 20;
        [Tooltip("Tốc độ bay (đơn vị/giây)")]
        public float moveSpeed = 2f;
        public EnemyMovement movement = EnemyMovement.Chase;
        [Tooltip("Chỉ dùng cho Orbit: khoảng cách giữ với player")]
        public float orbitRadius = 3f;
        [Tooltip("Đụng người player thì player mất 1 mạng")]
        public bool contactDamage = true;

        [Header("Bắn")]
        [Tooltip("Để trống = quái không bắn")]
        public BulletPatternSO bulletPattern;

        [Header("Phần thưởng")]
        public RewardData reward;
    }
}
