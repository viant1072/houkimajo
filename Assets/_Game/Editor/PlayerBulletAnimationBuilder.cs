using System;
using Houkimajo.Player;
using UnityEditor;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Gắn animation vào profile bắn thường, không thay balance, icon hay sprite fallback.</summary>
    public static class PlayerBulletAnimationBuilder
    {
        [MenuItem("Houkimajo/Apply Player Bullet Animation")]
        public static void Apply()
        {
            var attack = AssetDatabase.LoadAssetAtPath<BasicAttackSO>("Assets/_Game/Data/Skills/BasicAttack_Arcane.asset");
            if (attack == null)
            {
                Debug.LogWarning("[PlayerBullet] Chạy Add Skills To Current Scene trước để tạo profile bắn thường.");
                return;
            }
            var frames = new Sprite[4];
            const string sheet = "Assets/_Game/Art/Sprites/Bullets/bullet_player_arcane_flight_v1.png";
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(sheet))
                if (asset is Sprite sprite)
                    for (int i = 0; i < frames.Length; i++)
                        if (sprite.name == "PlayerBullet_Arcane_Flight_" + i) frames[i] = sprite;
            for (int i = 0; i < frames.Length; i++)
                if (frames[i] == null) throw new InvalidOperationException("Thiếu frame đạn player: " + i);
            bool hasFrames = attack.projectileAnimationFrames != null && attack.projectileAnimationFrames.Length > 0;
            attack.projectileAnimationFrames = frames;
            if (!hasFrames) attack.projectileAnimationFramesPerSecond = 12f;
            EditorUtility.SetDirty(attack);
            AssetDatabase.SaveAssets();
            Debug.Log("[PlayerBullet] Đã gắn 4 frame đạn bắn thường, giữ nguyên chỉ số và hitbox.");
        }
    }
}
