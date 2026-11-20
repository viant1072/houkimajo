using System;
using System.Collections.Generic;
using UnityEngine;
using Houkimajo.Boss;
using Houkimajo.Enemies;

namespace Houkimajo.Systems
{
    /// <summary>1 wave trong màn + có mở shop sau wave đó không.</summary>
    [Serializable]
    public class StageWave
    {
        public WaveSO wave;

        [Tooltip("Mở shop sau wave này (chưa dùng — shop làm ở giai đoạn 5)")]
        public bool openShopAfter;
    }

    /// <summary>
    /// Nội dung 1 màn chơi: các wave quái rồi tới boss. Mọi màn dùng chung scene Stage01,
    /// khác nhau ở asset này. Tạo qua Create → Houkimajo → Stage, lưu ở Assets/_Game/Data/Stages.
    /// </summary>
    [CreateAssetMenu(fileName = "Stage", menuName = "Houkimajo/Stage")]
    public class StageSO : ScriptableObject
    {
        [Tooltip("Mã định danh cố định (dùng cho save)")]
        public string id;
        public string stageName = "Stage";

        [Header("Background")]
        [SerializeField, Tooltip("Ảnh nền phủ camera, nằm sau toàn bộ gameplay")]
        private Sprite background;
        public Sprite Background => background;

        public List<StageWave> waves = new List<StageWave>();

        [Header("Boss")]
        public List<BossPhaseSO> bossPhases = new List<BossPhaseSO>();
        public int bossMaxHP = 1000;
        [Tooltip("Màu nhuộm sprite boss để mỗi màn trông khác nhau khi chưa có art riêng")]
        public Color bossTint = Color.white;

        [Tooltip("Sprite boss của màn; để trống để dùng hình sẵn trong scene")]
        public Sprite bossSprite;
        [Tooltip("Bộ frame idle dùng chung, không giữ trạng thái animation trên asset")]
        public Sprite[] bossAnimationFrames;
        [Min(0.1f), Tooltip("Nhịp idle, số frame mỗi giây")]
        public float bossAnimationFramesPerSecond = 6f;
        [Min(0f), Tooltip("Bán kính hitbox thân boss; 0 giữ collider của scene")]
        public float bossColliderRadius;

        [Header("Thưởng")]
        [Tooltip("Vàng cộng thêm khi qua màn")]
        public int clearGold = 100;
    }
}
