using System;
using Houkimajo.Bullets;
using Houkimajo.Enemies;
using Houkimajo.Systems;
using UnityEditor;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Gắn art vào data/prefab chung và thêm wave thử sáu loại, giữ balance đã chỉnh.</summary>
    public static class EnemyGameplayBuilder
    {
        private const string DataFolder = "Assets/_Game/Data/Enemies/";
        private const string PrefabPath = "Assets/_Game/Prefabs/Enemies/Enemy.prefab";
        private const string WavePath = "Assets/_Game/Data/Waves/Enemy_Showcase.asset";
        private static readonly string[] AssetNames = { "Bat", "Firefly", "Pumpkin", "WispGhost", "CursedGrimoire", "MoonJelly" };
        private static readonly string[] Slugs = { "shadow_bat", "ember_firefly", "flying_pumpkin", "wisp_ghost", "cursed_grimoire", "moon_jelly" };
        private static readonly string[] SpriteNames = { "ShadowBat", "EmberFirefly", "FlyingPumpkin", "WispGhost", "CursedGrimoire", "MoonJelly" };
        private static readonly float[] Rates = { 6f, 10f, 5f, 5f, 6f, 4f };

        [MenuItem("Houkimajo/Apply Enemy Art + Six Enemy Showcase")]
        public static void Apply()
        {
            // StageBootstrapper tạo ba data gốc và prefab trước khi gọi tới đây.
            for (int i = 0; i < 3; i++)
                if (AssetDatabase.LoadAssetAtPath<EnemyDataSO>(DataFolder + "Enemy_" + AssetNames[i] + ".asset") == null)
                    throw new InvalidOperationException("Chạy Setup Stages trước khi gắn art quái.");

            CreateNewEnemy("WispGhost", "Ma Trơi", 25, 3f, EnemyMovement.Chase, 3f, 0.2f, null, 3, 4);
            CreateNewEnemy("CursedGrimoire", "Ma Thư", 45, 1.5f, EnemyMovement.Orbit, 3.5f, 0.24f,
                Pattern("GrimoireAimed", PatternShape.Aimed, 1, 3f, 1.8f), 5, 7);
            CreateNewEnemy("MoonJelly", "Sứa Trăng", 60, 1f, EnemyMovement.Orbit, 3f, 0.25f,
                Pattern("MoonJellyRadial", PatternShape.Radial, 6, 1.8f, 2.5f), 6, 9);

            var enemies = new EnemyDataSO[AssetNames.Length];
            for (int i = 0; i < enemies.Length; i++)
            {
                var data = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(DataFolder + "Enemy_" + AssetNames[i] + ".asset");
                var frames = new Sprite[4];
                string sheet = "Assets/_Game/Art/Sprites/Enemies/enemy_" + Slugs[i] + "_flight_v1.png";
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(sheet))
                    if (asset is Sprite sprite)
                        for (int frame = 0; frame < frames.Length; frame++)
                            if (sprite.name == "Enemy_" + SpriteNames[i] + "_Flight_" + frame) frames[frame] = sprite;
                for (int frame = 0; frame < frames.Length; frame++)
                    if (frames[frame] == null) throw new InvalidOperationException("Thiếu frame " + frame + " trong " + sheet);
                bool hasAnimation = data.animationFrames != null && data.animationFrames.Length > 0;
                data.sprite = frames[0];
                data.animationFrames = frames;
                if (!hasAnimation) data.animationFramesPerSecond = Rates[i];
                EditorUtility.SetDirty(data);
                enemies[i] = data;
            }

            var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                if (prefab.GetComponent<EnemySpriteAnimation>() == null) prefab.AddComponent<EnemySpriteAnimation>();
                prefab.GetComponent<SpriteRenderer>().sprite = enemies[0].sprite;
                PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }

            var wave = AssetDatabase.LoadAssetAtPath<WaveSO>(WavePath);
            if (wave == null)
            {
                wave = ScriptableObject.CreateInstance<WaveSO>();
                foreach (var enemy in enemies)
                    wave.entries.Add(new WaveEntry { enemy = enemy, count = 2, spawnInterval = 0.5f });
                AssetDatabase.CreateAsset(wave, WavePath);
            }
            var stage = AssetDatabase.LoadAssetAtPath<StageSO>("Assets/_Game/Data/Stages/Stage_01.asset");
            if (stage == null) throw new InvalidOperationException("Thiếu Stage_01, chạy Setup Stages trước.");
            bool included = false;
            foreach (var entry in stage.waves) if (entry != null && entry.wave == wave) included = true;
            if (!included)
            {
                stage.waves.Insert(0, new StageWave { wave = wave });
                EditorUtility.SetDirty(stage);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Houkimajo] Đã gắn 6 quái có animation; wave đầu màn 1 gồm hai con mỗi loại.");
        }

        private static void CreateNewEnemy(string name, string displayName, int hp, float speed,
            EnemyMovement movement, float orbit, float radius, BulletPatternSO pattern, int gold, int xp)
        {
            string path = DataFolder + "Enemy_" + name + ".asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path) != null) return;
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            data.id = "enemy_" + name.ToLowerInvariant();
            data.displayName = displayName;
            data.maxHP = hp; data.moveSpeed = speed; data.movement = movement;
            data.orbitRadius = orbit; data.colliderRadius = radius; data.bulletPattern = pattern;
            data.reward = new RewardData { gold = gold, xp = xp };
            AssetDatabase.CreateAsset(data, path);
        }

        private static BulletPatternSO Pattern(string name, PatternShape shape, int count, float speed, float interval)
        {
            string path = "Assets/_Game/Data/BulletPatterns/Enemy_" + name + ".asset";
            var pattern = AssetDatabase.LoadAssetAtPath<BulletPatternSO>(path);
            if (pattern != null) return pattern;
            pattern = ScriptableObject.CreateInstance<BulletPatternSO>();
            pattern.shape = shape; pattern.bulletCount = count; pattern.bulletSpeed = speed; pattern.fireInterval = interval;
            AssetDatabase.CreateAsset(pattern, path);
            return pattern;
        }
    }
}
