using UnityEngine;

namespace Houkimajo.Player
{
    /// <summary>Profile bắn thường có thể thay hoặc nâng cấp qua trang bị/shop sau này.</summary>
    [CreateAssetMenu(fileName = "BasicAttack_Arcane", menuName = "Houkimajo/Player/Basic Attack")]
    public class BasicAttackSO : ScriptableObject
    {
        public string id = "basic_arcane";
        public string displayName = "Đạn Ma Thuật";
        public Sprite icon;
        [Tooltip("Nhãn thuộc tính hiển thị; chưa quyết định hiệu ứng damage nguyên tố")]
        public string attributeName = "Phép";
        public Color attributeColor = new Color(0.7f, 0.45f, 1f);
        public Sprite projectileSprite;
        public Color projectileTint = Color.white;
        [Tooltip("Các frame chuyển động của đạn; để trống để dùng sprite tĩnh")]
        public Sprite[] projectileAnimationFrames;
        [Min(0.1f), Tooltip("Tốc độ animation đạn, số frame mỗi giây")]
        public float projectileAnimationFramesPerSecond = 12f;
        public BasicAttackLevelData[] levels = { new BasicAttackLevelData() };

        public int MaxLevel => levels != null ? levels.Length : 0;

        public BasicAttackLevelData GetLevel(int level)
        {
            return MaxLevel > 0 ? levels[Mathf.Clamp(level, 1, MaxLevel) - 1] : null;
        }
    }
}
