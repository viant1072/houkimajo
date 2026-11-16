using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Houkimajo.Bullets;
using Houkimajo.Player;
using Houkimajo.Skills;
using Houkimajo.UI;
using Houkimajo.Systems;

namespace Houkimajo.EditorTools
{
    /// <summary>
    /// Tạo asset skill mẫu (theo chỉ số trong docs/skills.md), prefab projectile, gắn
    /// SkillCaster lên Player và thanh skill lên HUD. Gọi tự động từ Setup/Rebuild, hoặc
    /// chạy riêng qua menu để thêm skill vào scene đã dựng sẵn mà không phải Rebuild.
    /// Asset đã tồn tại được cập nhật hình ảnh, giữ chỉ số người dùng đã chỉnh tay.
    /// </summary>
    public static class SkillBootstrapper
    {
        private const string SkillDataDir = "Assets/_Game/Data/Skills";
        private const string SkillPrefabDir = "Assets/_Game/Prefabs/Skills";

        [MenuItem("Houkimajo/Add Skills To Current Scene")]
        public static void AddSkillsToCurrentScene()
        {
            var controller = Object.FindAnyObjectByType<PlayerController>();
            if (controller == null)
            {
                Debug.LogWarning("[Houkimajo] Không thấy Player trong scene — chạy Setup Playtest Scene trước.");
                return;
            }

            var enemyPoolGo = GameObject.Find("EnemyBulletPool");
            Setup(controller.gameObject, enemyPoolGo != null ? enemyPoolGo.GetComponent<BulletPool>() : null);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Debug.Log("[Houkimajo] Đã gắn 13 skill: F1/F2/F3 đổi Q/E/R, F4 nạp R, F5 tăng cấp. Ctrl+S để lưu.");
        }

        public static void Setup(GameObject player, BulletPool enemyPool)
        {
            EnsureFolder(SkillDataDir);
            EnsureFolder(SkillPrefabDir);

            var missilePrefab = CreateOrLoadMissilePrefab();
            var missileSkill = CreateOrLoadMissileSkill(missilePrefab);
            var dashSkill = CreateOrLoadDashSkill();
            var fireball = CreateOrLoadFireball();
            var ice = CreateOrLoadIce();
            var lightning = CreateOrLoadLightning();
            var erSkills = ERSkillBootstrapper.CreateAssets(dashSkill);
            ApplyArt(missileSkill, "magic_missile", "Q_MagicMissile", "MasteryExplosion");
            ApplyArt(fireball, "fireball", "Q_Fireball", "MasteryBurningZone");
            ApplyArt(ice, "ice_arrow_rain", "Q_IceArrowRain", "MasteryShatter");
            ApplyArt(lightning, "chain_lightning", "Q_ChainLightning", "MasteryStun");
            missileSkill.attributeName = "Phép";
            fireball.attributeName = "Lửa";
            ice.attributeName = "Băng";
            lightning.attributeName = "Sét";
            dashSkill.attributeName = "Cơ động";
            EditorUtility.SetDirty(dashSkill);
            missileSkill.projectilePrefab = missilePrefab;
            EditorUtility.SetDirty(missileSkill);
            AssetDatabase.SaveAssets();

            var pools = EnsureRoot<SkillPools>("SkillPools");
            if (player.GetComponent<Wallet>() == null) Undo.AddComponent<Wallet>(player);
            if (player.GetComponent<SkillEffectController>() == null) Undo.AddComponent<SkillEffectController>(player);
            if (player.GetComponentInChildren<GrazeDetector>() == null)
            {
                var zone = new GameObject("GrazeZone");
                zone.transform.SetParent(player.transform, false);
                var collider = zone.AddComponent<CircleCollider2D>();
                collider.isTrigger = true; collider.radius = 0.5f;
                zone.AddComponent<GrazeDetector>();
            }
            if (enemyPool != null)
            {
                var poolSO = new SerializedObject(enemyPool);
                var capacity = poolSO.FindProperty("initialSize");
                capacity.intValue = Mathf.Max(512, capacity.intValue);
                poolSO.ApplyModifiedProperties();
            }

            var caster = player.GetComponent<SkillCaster>();
            if (caster == null) caster = Undo.AddComponent<SkillCaster>(player);

            var casterSO = new SerializedObject(caster);
            casterSO.FindProperty("pools").objectReferenceValue = pools;
            casterSO.FindProperty("enemyBulletPool").objectReferenceValue = enemyPool;
            if (casterSO.FindProperty("loadout.qSkill").objectReferenceValue == null)
            {
                casterSO.FindProperty("loadout.qSkill").objectReferenceValue = missileSkill;
                casterSO.FindProperty("loadout.qLevel").intValue = 1;
            }
            if (casterSO.FindProperty("loadout.eSkill").objectReferenceValue == null)
            {
                casterSO.FindProperty("loadout.eSkill").objectReferenceValue = dashSkill;
                casterSO.FindProperty("loadout.eLevel").intValue = 1;
            }
            if (casterSO.FindProperty("loadout.rSkill").objectReferenceValue == null)
            {
                casterSO.FindProperty("loadout.rSkill").objectReferenceValue = erSkills[5];
                casterSO.FindProperty("loadout.rLevel").intValue = 1;
            }

            var library = casterSO.FindProperty("debugSkillLibrary");
            SkillSO[] skills = new SkillSO[4 + erSkills.Length];
            skills[0] = missileSkill; skills[1] = fireball; skills[2] = lightning; skills[3] = ice;
            System.Array.Copy(erSkills, 0, skills, 4, erSkills.Length);
            library.arraySize = skills.Length;
            for (int i = 0; i < skills.Length; i++)
                library.GetArrayElementAtIndex(i).objectReferenceValue = skills[i];
            casterSO.ApplyModifiedProperties();

            CreateSkillBar(caster);
            AssetDatabase.SaveAssets();
        }

        // ---------------- Prefab ----------------

        private static SkillProjectile CreateOrLoadMissilePrefab()
        {
            return CreateProjectile("MagicMissile", "magic_missile", "Q_MagicMissile_Projectile", 0.65f);
        }

        internal static Sprite LoadArt(string slug, string name, string group = "Q")
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(
                $"Assets/_Game/Art/Sprites/Skills/{group}/{group.ToLower()}_{slug}_v1.png"))
                if (asset is Sprite sprite && sprite.name == name) return sprite;
            Debug.LogError($"[Skills] Thiếu sprite {name}.");
            return null;
        }

        private static void ApplyArt(SkillSO skill, string slug, string prefix, string mastery)
        {
            skill.icon = LoadArt(slug, prefix + "_Icon");
            skill.impactSprite = LoadArt(slug, prefix + "_Impact");
            skill.masterySprite = LoadArt(slug, prefix + "_" + mastery);
            EditorUtility.SetDirty(skill);
        }

        internal static SkillProjectile CreateProjectile(string name, string slug, string spriteName, float scale, string group = "Q")
        {
            string path = $"{SkillPrefabDir}/{name}.prefab";
            bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            var go = exists ? PrefabUtility.LoadPrefabContents(path) : new GameObject(name);
            try
            {
                if (go.GetComponent<SkillProjectile>() == null)
                {
                    go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                    var collider = go.AddComponent<CircleCollider2D>();
                    collider.isTrigger = true;
                    collider.radius = 0.12f;
                    go.AddComponent<SkillProjectile>();
                }
                var oldRenderer = go.GetComponent<SpriteRenderer>();
                if (oldRenderer != null) oldRenderer.enabled = false;
                var visual = go.transform.Find("Visual");
                if (visual == null)
                {
                    visual = new GameObject("Visual").transform;
                    visual.SetParent(go.transform, false);
                }
                visual.localScale = new Vector3(scale, scale, 1f);
                var renderer = visual.GetComponent<SpriteRenderer>();
                if (renderer == null) renderer = visual.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = LoadArt(slug, spriteName, group);
                renderer.sortingOrder = 3;
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                return prefab.GetComponent<SkillProjectile>();
            }
            finally
            {
                if (exists) PrefabUtility.UnloadPrefabContents(go);
                else Object.DestroyImmediate(go);
            }
        }

        private static SkillLevelData[] CreateLevels(int damage, int damageStep, int count, int countStep,
            float cooldown, float cooldownStep)
        {
            var levels = new SkillLevelData[5];
            for (int i = 0; i < levels.Length; i++)
                levels[i] = new SkillLevelData
                {
                    damage = damage + i * damageStep, count = count + i * countStep,
                    cooldown = cooldown + i * cooldownStep, upgradeCost = UpgradeCosts[i],
                    hasMasteryEffect = i == 4
                };
            return levels;
        }

        private static FireballSkillSO CreateOrLoadFireball()
        {
            var prefab = CreateProjectile("Fireball", "fireball", "Q_Fireball_Projectile", 0.85f);
            string path = $"{SkillDataDir}/Skill_Fireball.asset";
            var skill = AssetDatabase.LoadAssetAtPath<FireballSkillSO>(path);
            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<FireballSkillSO>();
                skill.id = "skill_fireball"; skill.displayName = "Cầu Lửa";
                skill.color = new Color(1f, 0.35f, 0.05f);
                skill.levels = CreateLevels(60, 15, 1, 0, 8f, -0.5f);
                for (int i = 0; i < 5; i++) skill.levels[i].radius = 1.5f + 0.2f * i;
                AssetDatabase.CreateAsset(skill, path);
            }
            skill.projectilePrefab = prefab;
            return skill;
        }

        private static IceArrowRainSkillSO CreateOrLoadIce()
        {
            var prefab = CreateProjectile("IceArrow", "ice_arrow_rain", "Q_IceArrowRain_Projectile", 0.8f);
            string path = $"{SkillDataDir}/Skill_IceArrowRain.asset";
            var skill = AssetDatabase.LoadAssetAtPath<IceArrowRainSkillSO>(path);
            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<IceArrowRainSkillSO>();
                skill.id = "skill_ice_arrow_rain"; skill.displayName = "Mưa Băng Tiễn";
                skill.color = new Color(0.2f, 0.85f, 1f);
                skill.levels = CreateLevels(12, 2, 7, 1, 7f, -0.5f);
                for (int i = 0; i < 5; i++) skill.levels[i].duration = 2f + i * 0.5f;
                AssetDatabase.CreateAsset(skill, path);
            }
            skill.projectilePrefab = prefab;
            return skill;
        }

        private static ChainLightningSkillSO CreateOrLoadLightning()
        {
            string path = $"{SkillDataDir}/Skill_ChainLightning.asset";
            var skill = AssetDatabase.LoadAssetAtPath<ChainLightningSkillSO>(path);
            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<ChainLightningSkillSO>();
                skill.id = "skill_chain_lightning"; skill.displayName = "Sét Lan";
                skill.color = new Color(1f, 0.85f, 0.1f);
                skill.levels = CreateLevels(35, 5, 3, 1, 6f, -0.375f);
                foreach (var level in skill.levels) level.range = 4f;
                AssetDatabase.CreateAsset(skill, path);
            }
            skill.arcSprite = LoadArt("chain_lightning", "Q_ChainLightning_Arc");
            return skill;
        }

        // ---------------- Asset skill (chỉ số theo docs/skills.md) ----------------

        private static readonly int[] UpgradeCosts = { 0, 100, 250, 500, 1000 };

        private static HomingMissileSkillSO CreateOrLoadMissileSkill(SkillProjectile prefab)
        {
            string path = $"{SkillDataDir}/Skill_MagicMissile.asset";
            var existing = AssetDatabase.LoadAssetAtPath<HomingMissileSkillSO>(path);
            if (existing != null) return existing;

            var skill = ScriptableObject.CreateInstance<HomingMissileSkillSO>();
            skill.id = "skill_magic_missile";
            skill.displayName = "Tên Lửa Ma Thuật";
            skill.slot = SkillSlot.Q;
            skill.color = new Color(0.75f, 0.4f, 1f);
            skill.description = "Bắn chùm tên lửa tự đuổi theo địch gần con trỏ nhất.";
            skill.projectilePrefab = prefab;

            skill.levels = new SkillLevelData[5];
            for (int i = 0; i < 5; i++)
            {
                skill.levels[i] = new SkillLevelData
                {
                    count = 5 + i,                 // 5 → 9 quả
                    damage = 15 + 3 * i,           // 15 → 27
                    cooldown = 7f - 0.5f * i,      // 7 → 5s
                    radius = 0.5f,                 // bán kính nổ, chỉ dùng ở Lv5
                    upgradeCost = UpgradeCosts[i],
                    hasMasteryEffect = i == 4
                };
            }

            AssetDatabase.CreateAsset(skill, path);
            return skill;
        }

        private static DashSkillSO CreateOrLoadDashSkill()
        {
            string path = $"{SkillDataDir}/Skill_BroomDash.asset";
            var existing = AssetDatabase.LoadAssetAtPath<DashSkillSO>(path);
            if (existing != null) return existing;

            var skill = ScriptableObject.CreateInstance<DashSkillSO>();
            skill.id = "skill_broom_dash";
            skill.displayName = "Lướt Chổi";
            skill.slot = SkillSlot.E;
            skill.color = new Color(0.4f, 0.8f, 1f);
            skill.description = "Lao nhanh theo hướng đang di chuyển, bất tử trong lúc lướt.";

            float[] ranges = { 3f, 3.4f, 3.8f, 4.2f, 4.5f };
            float[] cooldowns = { 6f, 5.4f, 4.8f, 4.2f, 3.5f };

            skill.levels = new SkillLevelData[5];
            for (int i = 0; i < 5; i++)
            {
                skill.levels[i] = new SkillLevelData
                {
                    range = ranges[i],
                    cooldown = cooldowns[i],
                    charges = i == 4 ? 2 : 1,      // Lv5: 2 lượt lướt
                    upgradeCost = UpgradeCosts[i],
                    hasMasteryEffect = i == 4
                };
            }

            AssetDatabase.CreateAsset(skill, path);
            return skill;
        }

        // ---------------- HUD ----------------

        private static void CreateSkillBar(SkillCaster caster)
        {
            var canvasGo = GameObject.Find("HUDCanvas");
            if (canvasGo == null) return;
            SkillHudBuilder.Build(caster, canvasGo.GetComponent<Canvas>());
        }

        // ---------------- Tiện ích ----------------

        private static T EnsureRoot<T>(string name) where T : Component
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            }

            var component = go.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(go);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}
