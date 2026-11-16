using System;
using System.Reflection;
using Houkimajo.Boss;
using Houkimajo.Bullets;
using Houkimajo.Player;
using Houkimajo.Skills;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Kiểm tra tích hợp trên bản sao project bằng Unity batch, không thay scene đang mở của người dùng.</summary>
    public static class QSkillGameplayChecks
    {
        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Chỉ chạy trên project kiểm tra bằng batch mode.");
            try
            {
                EditorSceneManager.OpenScene("Assets/_Game/Scenes/Stage01.unity");
                SkillBootstrapper.AddSkillsToCurrentScene();
                EditorSceneManager.SaveOpenScenes();
                string[] names = { "Skill_MagicMissile", "Skill_Fireball", "Skill_ChainLightning", "Skill_IceArrowRain" };
                foreach (string name in names)
                {
                    var skill = AssetDatabase.LoadAssetAtPath<SkillSO>($"Assets/_Game/Data/Skills/{name}.asset");
                    Check(skill != null && skill.icon != null && skill.impactSprite != null && skill.masterySprite != null,
                        "Asset và 3 sprite: " + name);
                    Check(skill.MaxLevel == 5, "Đủ 5 cấp: " + name);
                }

                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                typeof(TargetRegistry).GetMethod("ResetRegistry", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                var caster = new GameObject("TestCaster");
                var pools = new GameObject("TestPools").AddComponent<SkillPools>();
                Call(pools, "Awake");
                Call(pools.Effects, "Awake");
                var context = new SkillContext(caster.transform, null, null, null, pools, null);
                var a = Target(new Vector2(0f, 1f));
                var b = Target(new Vector2(0f, 2f));
                var c = Target(new Vector2(0f, 3f));
                var far = Target(new Vector2(0f, 10f));

                var lightning = Load<ChainLightningSkillSO>("Skill_ChainLightning");
                lightning.Activate(context, lightning.GetLevel(1));
                Check(a.CurrentHP == 965 && b.CurrentHP == 965 && c.CurrentHP == 965 && far.CurrentHP == 1000,
                    "Sét chỉ trúng ba mục tiêu khác nhau trong tầm");
                Check(!(a is ISkillStatusTarget), "Boss miễn nhiễm slow/stun");

                var missile = Load<HomingMissileSkillSO>("Skill_MagicMissile");
                missile.Prewarm(context);
                missile.Activate(context, missile.GetLevel(1));
                var missilePool = pools.GetPool(missile.projectilePrefab);
                var missiles = missilePool.GetComponentsInChildren<SkillProjectile>();
                Check(missiles.Length == 5, "Tên lửa Lv1 phóng 5 quả");
                foreach (var item in missiles)
                {
                    Check(item.GetComponentInChildren<SpriteRenderer>().sprite.name == "Q_MagicMissile_Projectile",
                        "Tên lửa dùng sprite Q mới");
                    missilePool.Return(item);
                }

                var fireball = Load<FireballSkillSO>("Skill_Fireball");
                fireball.Prewarm(context);
                fireball.Activate(context, fireball.GetLevel(1));
                var firePool = pools.GetPool(fireball.projectilePrefab);
                var fire = firePool.GetComponentInChildren<SkillProjectile>();
                fire.transform.position = new Vector2(0f, 1f);
                Call(fire, "OnTriggerEnter2D", a.GetComponent<Collider2D>());
                Call(fire, "OnTriggerEnter2D", a.GetComponent<Collider2D>());
                Check(a.CurrentHP == 905 && b.CurrentHP == 905 && c.CurrentHP == 965,
                    "Cầu lửa nổ vùng đúng một lần, không cộng thêm hit trực tiếp");

                var ice = Load<IceArrowRainSkillSO>("Skill_IceArrowRain");
                ice.Prewarm(context);
                ice.Activate(context, ice.GetLevel(1));
                var icePool = pools.GetPool(ice.projectilePrefab);
                var arrows = icePool.GetComponentsInChildren<SkillProjectile>();
                Check(arrows.Length == 7, "Băng Lv1 phóng 7 mũi");
                var arrow = arrows[0];
                Call(arrow, "OnTriggerEnter2D", a.GetComponent<Collider2D>());
                Call(arrow, "OnTriggerEnter2D", a.GetComponent<Collider2D>());
                Call(arrow, "OnTriggerEnter2D", b.GetComponent<Collider2D>());
                Call(arrow, "OnTriggerEnter2D", c.GetComponent<Collider2D>());
                Check(a.CurrentHP == 893 && b.CurrentHP == 893 && c.CurrentHP == 953 && arrow.PoolIndex == -1,
                    "Băng xuyên 3 mục tiêu, không trúng lặp collider");
                foreach (var item in arrows) if (item.PoolIndex >= 0) icePool.Return(item);
                int childCount = icePool.transform.childCount;
                var rented = new SkillProjectile[32];
                for (int i = 0; i < rented.Length; i++) rented[i] = icePool.Get(Vector2.zero);
                Check(icePool.Get(Vector2.zero) == null && icePool.transform.childCount == childCount,
                    "Hết pool không Instantiate");
                foreach (var item in rented) icePool.Return(item);
                var reused = icePool.Get(Vector2.zero);
                reused.Launch(Vector2.up, 8f, 12, DamageSource.Skill, 3f);
                reused.ConfigureIce(0.4f, 2f, false);
                Call(reused, "OnTriggerEnter2D", a.GetComponent<Collider2D>());
                Check(a.CurrentHP == 881, "Pool reuse xóa danh sách mục tiêu cũ");

                var zone = pools.Effects.PlayBurningZone(fireball.masterySprite, Vector2.up, 3.75f, 2f, 1.5f, 5, 0.25f);
                zone.Advance(0f);
                Check(a.CurrentHP == 881, "Pause không chạy tick vùng lửa");
                zone.Advance(2.1f);
                Check(a.CurrentHP == 841 && b.CurrentHP == 853 && c.CurrentHP == 953,
                    "Vùng lửa 2 giây gây đúng 40 damage kể cả frame dài");

                // Hai mục tiêu chết và tự Unregister trong cùng một chuỗi sét.
                SetHealth(a, 10); SetHealth(b, 10);
                lightning.Activate(context, lightning.GetLevel(1));
                Check(a.CurrentHP == 0 && b.CurrentHP == 0 && c.CurrentHP == 918,
                    "Sét vẫn đúng chuỗi khi target registry thay đổi");
                Debug.Log("Q_SKILLS_CHECKS_PASS: assets, chain, explosion, piercing, reuse, capacity, pause, burn ticks, target death.");
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
        }

        private static T Load<T>(string name) where T : SkillSO
        {
            return AssetDatabase.LoadAssetAtPath<T>($"Assets/_Game/Data/Skills/{name}.asset");
        }

        private static BossHealth Target(Vector2 position)
        {
            var go = new GameObject("TestBoss");
            go.transform.position = position;
            go.AddComponent<CircleCollider2D>().isTrigger = true;
            var health = go.AddComponent<BossHealth>();
            SetHealth(health, 1000);
            TargetRegistry.Register(health, go.transform);
            return health;
        }

        private static void SetHealth(BossHealth health, int hp)
        {
            var so = new SerializedObject(health);
            so.FindProperty("maxHP").intValue = hp;
            so.ApplyModifiedPropertiesWithoutUndo();
            Call(health, "Awake");
        }

        private static void Call(object instance, string method, params object[] arguments)
        {
            instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(instance, arguments);
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            Debug.Log("Q_CHECK_PASS: " + message);
        }
    }
}
