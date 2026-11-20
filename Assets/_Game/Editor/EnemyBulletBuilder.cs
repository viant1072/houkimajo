using System;
using Houkimajo.Bullets;
using Houkimajo.Enemies;
using UnityEditor;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Gắn sprite đạn riêng theo loài, bổ sung pattern cho quái chưa có đạn.</summary>
    public static class EnemyBulletBuilder
    {
        private static readonly string[] Enemies = { "Bat", "Firefly", "Pumpkin", "WispGhost", "CursedGrimoire", "MoonJelly" };
        private static readonly string[] Slugs = { "shadow_bat", "ember_firefly", "flying_pumpkin", "wisp_ghost", "cursed_grimoire", "moon_jelly" };

        [MenuItem("Houkimajo/Apply Enemy Bullet Art + Patterns")]
        public static void Apply()
        {
            for (int i = 0; i < Enemies.Length; i++)
            {
                var enemy = AssetDatabase.LoadAssetAtPath<EnemyDataSO>("Assets/_Game/Data/Enemies/Enemy_" + Enemies[i] + ".asset");
                if (enemy == null) throw new InvalidOperationException("Gắn bộ 6 quái trước khi gắn đạn: " + Enemies[i]);
                string artPath = "Assets/_Game/Art/Sprites/Bullets/bullet_enemy_" + Slugs[i] + "_v1.png";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(artPath);
                if (sprite == null) throw new InvalidOperationException("Thiếu sprite đạn: " + artPath);

                var pattern = enemy.bulletPattern;
                if (pattern == null)
                {
                    if (i == 0) pattern = LoadOrCreate("BatAimed", PatternShape.Aimed, 1, 3.2f, 2.5f, 0f);
                    else if (i == 3) pattern = LoadOrCreate("WispFan", PatternShape.AimedFan, 3, 2.6f, 2.3f, 30f);
                    else throw new InvalidOperationException("Thiếu pattern gốc của " + Enemies[i]);
                    enemy.bulletPattern = pattern;
                }

                // Chỉ migrate Ma Thư một lần từ phát đơn sang chùm; giữ chỉnh sửa sau đó.
                if (i == 4 && pattern.bulletSprite == null && pattern.shape == PatternShape.Aimed)
                {
                    pattern.shape = PatternShape.AimedFan;
                    pattern.bulletCount = 3;
                    pattern.spreadAngle = 36f;
                }
                pattern.bulletSprite = sprite;
                pattern.bulletTint = Color.white;
                pattern.orientBulletToDirection = i != 5;
                EditorUtility.SetDirty(pattern);
                EditorUtility.SetDirty(enemy);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Houkimajo] Đã gắn 6 sprite đạn riêng; Dơi/Ma Trơi có pattern bắn, Ma Thư bắn chùm.");
        }

        private static BulletPatternSO LoadOrCreate(string name, PatternShape shape, int count, float speed, float interval, float spread)
        {
            string path = "Assets/_Game/Data/BulletPatterns/Enemy_" + name + ".asset";
            var pattern = AssetDatabase.LoadAssetAtPath<BulletPatternSO>(path);
            if (pattern != null) return pattern;
            pattern = ScriptableObject.CreateInstance<BulletPatternSO>();
            pattern.shape = shape; pattern.bulletCount = count; pattern.bulletSpeed = speed;
            pattern.fireInterval = interval; pattern.spreadAngle = spread;
            AssetDatabase.CreateAsset(pattern, path);
            return pattern;
        }
    }
}
