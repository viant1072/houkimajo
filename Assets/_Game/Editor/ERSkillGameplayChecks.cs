using System;
using System.Collections.Generic;
using System.Reflection;
using Houkimajo.Boss;
using Houkimajo.Bullets;
using Houkimajo.Player;
using Houkimajo.Skills;
using Houkimajo.Systems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Houkimajo.EditorTools
{
    /// <summary>Kiểm tra E/R bằng Unity trên bản sao project; nhánh đổi/nạp debug, input thật kiểm tra ở ERPlayModeSmokeChecks.</summary>
    public static class ERSkillGameplayChecks
    {

        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Chỉ chạy trên project kiểm tra bằng batch mode.");
            try
            {
                EditorSceneManager.OpenScene("Assets/_Game/Scenes/Stage01.unity");
                SkillBootstrapper.AddSkillsToCurrentScene();
                SkillBootstrapper.AddSkillsToCurrentScene();
                EditorSceneManager.SaveOpenScenes();
                var caster = UnityEngine.Object.FindAnyObjectByType<SkillCaster>();
                var player = caster.gameObject;
                var runtime = player.GetComponent<SkillEffectController>();
                var health = player.GetComponent<PlayerHealth>();
                var controller = player.GetComponent<PlayerController>();
                var shooting = player.GetComponent<PlayerShooting>();
                var wallet = player.GetComponent<Wallet>();
                var pools = UnityEngine.Object.FindAnyObjectByType<SkillPools>();
                var enemy = GameObject.Find("EnemyBulletPool").GetComponent<BulletPool>();
                var library = new SerializedObject(caster).FindProperty("debugSkillLibrary");
                int eCount = 0, rCount = 0;
                for (int i = 0; i < library.arraySize; i++)
                {
                    var skill = (SkillSO)library.GetArrayElementAtIndex(i).objectReferenceValue;
                    Check(skill != null && skill.icon != null && skill.MaxLevel == 5, "Asset đủ icon/cấp: " + skill.name);
                    if (skill.slot == SkillSlot.E) eCount++;
                    if (skill.slot == SkillSlot.R) rCount++;
                }
                Check(library.arraySize == 13 && eCount == 5 && rCount == 4, "Thư viện đủ 13 skill / 5 E / 4 R, không trùng sau setup");
                Call(pools, "Awake"); Call(pools.Effects, "Awake");
                Call(enemy, "Awake");
                Call(runtime, "Awake");
                Call(caster, "Awake"); Call(caster, "OnEnable"); Call(caster, "Start");
                typeof(TargetRegistry).GetMethod("ResetRegistry", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
                var target = MakeTarget(player.transform.position + Vector3.up * 2f);
                Time.timeScale = 1f;
                var originalQ = caster.GetSkill(SkillSlot.Q);
                var types = new HashSet<Type>();
                for (int i = 0; i < 5; i++) { types.Add(caster.GetSkill(SkillSlot.E).GetType()); Press(caster, Key.F2); }
                Check(types.Count == 5 && caster.GetSkill(SkillSlot.Q) == originalQ, "Nhánh F2 đổi vòng đúng 5 E, giữ Q");
                types.Clear();
                for (int i = 0; i < 4; i++) { types.Add(caster.GetSkill(SkillSlot.R).GetType()); Press(caster, Key.F3); }
                Check(types.Count == 4, "Nhánh F3 đổi vòng đúng 4 R");
                DamageEvents.DealPlayerDamage(target, 100, DamageSource.BasicShot);
                DamageEvents.DealPlayerDamage(target, 100, DamageSource.Skill);
                Check(Mathf.Approximately(caster.UltimateEnergy, 10f), "Damage thường/Q/E nạp R đúng 0.05/damage");
                DamageEvents.DealPlayerDamage(target, 100, DamageSource.Ultimate);
                Check(Mathf.Approximately(caster.UltimateEnergy, 10f), "Ultimate không tự nạp R");
                var bullet = enemy.Get(player.transform.position + Vector3.right * 0.4f);
                bullet.Launch(Vector2.up, 2f);
                var graze = player.GetComponentInChildren<GrazeDetector>();
                Call(graze, "OnTriggerEnter2D", bullet.GetComponent<Collider2D>());
                Call(graze, "OnTriggerEnter2D", bullet.GetComponent<Collider2D>());
                Check(Mathf.Approximately(caster.UltimateEnergy, 11f), "Graze mỗi viên chỉ cộng 1 R");
                Press(caster, Key.F4);
                Press(caster, Key.F3);
                Check(caster.UltimateEnergy == 100f, "F4 đầy R; đổi F3 giữ năng lượng");
                caster.Equip(SkillSlot.R, Load<TimeStopSkillSO>("TimeStop"), 1);
                Check(caster.TryCast(SkillSlot.R) && caster.UltimateEnergy == 0f && !caster.TryCast(SkillSlot.R), "R cần 100, dùng xong tiêu hao hết");
                runtime.Advance(4.1f);
                enemy.ClearAll();

                var shield = Load<ShieldSkillSO>("MagicShield");
                runtime.BeginShield(shield, shield.GetLevel(5));
                for (int i = 0; i < 3; i++)
                {
                    Set(health, "skillInvincibleUntil", -1f);
                    health.TakeHit();
                }
                Check(health.Lives == 3 && health.ShieldHits == 0, "Khiên Lv5 đỡ 3 hit không mất mạng");
                runtime.BeginShield(shield, shield.GetLevel(5));
                Spawn(enemy, player.transform.position);
                for (int i = 0; i < 3; i++) { Set(health, "skillInvincibleUntil", -1f); health.TakeHit(); }
                Check(enemy.ActiveCount == 0, "Khiên vỡ Lv5 xoá đạn trong vùng");
                for (int i = 0; i < 40; i++) Spawn(enemy, player.transform.position);
                var dispel = Load<DispelSkillSO>("Dispel");
                runtime.Dispel(dispel, dispel.GetLevel(5));
                Check(enemy.ActiveCount == 0 && wallet.Gold == 30, "Hoá Giải xóa 40 viên, thưởng tối đa 30 vàng");

                var hourglass = Load<HourglassSkillSO>("Hourglass");
                var near = Spawn(enemy, player.transform.position + Vector3.right);
                var far = Spawn(enemy, player.transform.position + Vector3.right * 5f);
                runtime.BeginSlowField(hourglass, hourglass.GetLevel(1));
                runtime.Advance(0.01f);
                Check(Mathf.Approximately(Get<float>(near, "fieldScale"), 0.5f)
                    && Get<int>(far, "slowFrame") == -1 && near.LocalSpeedScale == 1f, "Vùng cát chỉ làm chậm đạn ở trong, giữ local speed gốc");
                runtime.Advance(3.1f);
                var time = Load<TimeStopSkillSO>("TimeStop");
                enemy.SpeedScale = 0.8f;
                runtime.BeginTimeStop(time, time.GetLevel(1));
                runtime.Advance(0f);
                Check(enemy.MotionScale == 0f && runtime.IsTimeStopActive, "Ngưng Đọng dừng đạn và timer đứng yên khi pause");
                runtime.Advance(4.1f);
                Check(!enemy.IsTimeStopped && Mathf.Approximately(enemy.MotionScale, 0.8f), "Hết Ngưng Đọng trả SpeedScale trước đó");
                enemy.ClearAll();
                Spawn(enemy, player.transform.position);
                Spawn(enemy, player.transform.position + Vector3.right * 4f);
                runtime.BeginTimeStop(time, time.GetLevel(5));
                runtime.Advance(6.1f);
                Check(enemy.ActiveCount == 1, "Ngưng Đọng Lv5 chỉ xoá trong radius 3 khi hết");
                enemy.ClearAll();

                var teleport = Load<TeleportSkillSO>("Teleport");
                Vector2 old = player.transform.position;
                target.transform.position = old;
                int before = target.CurrentHP;
                runtime.Teleport(teleport, teleport.GetLevel(5));
                Check(Vector2.Distance(old, player.transform.position) <= 6.01f && health.IsInvincible
                    && target.CurrentHP == before - 50, "Teleport giữ tầm, bất tử, nổ tại điểm rời đi");
                player.transform.position = old;
                controller.TeleportTo(new Vector2(10000f, 10000f), 6f);
                Check(Vector2.Distance(old, player.transform.position) <= 6.01f && !ArenaBounds.IsOutside(player.transform.position, 0f),
                    "Teleport không vượt tầm/arena");
                player.transform.position = old;

                var meteor = Load<MeteorShowerSkillSO>("MeteorShower");
                runtime.BeginMeteorShower(meteor, new SkillLevelData { count = 4, duration = 0.25f, radius = 1f, damage = 80 });
                runtime.Advance(0.25f);
                Check(runtime.PendingMeteorCount == 1, "Meteor tạo lịch cảnh báo trước khi impact");
                var pending = (Array)Get<object>(runtime, "meteors");
                Vector2 impact = Vector2.zero;
                foreach (object item in pending)
                    if ((UnityEngine.Object)item.GetType().GetField("Skill").GetValue(item) != null)
                        impact = (Vector2)item.GetType().GetField("Position").GetValue(item);
                target.transform.position = impact;
                before = target.CurrentHP;
                runtime.Advance(0.39f);
                Check(target.CurrentHP == before, "Meteor không damage trước 0.4s cảnh báo");
                runtime.Advance(0.2f);
                Check(target.CurrentHP == before - 80 && runtime.PendingMeteorCount == 0, "Meteor impact đúng 80 damage sau cảnh báo/rơi");

                var familiar = Load<FamiliarSummonSkillSO>("FamiliarSummon");
                runtime.BeginFamiliars(familiar, new SkillLevelData { count = 4, duration = 0.2f, damage = 8, hasMasteryEffect = true });
                runtime.Advance(0.001f);
                var cats = Get<SpriteRenderer[]>(runtime, "cats");
                for (int i = 0; i < 7; i++) Spawn(enemy, cats[0].transform.position);
                runtime.Advance(0.001f);
                Check(enemy.ActiveCount == 2 && runtime.ActiveFamiliarCount == 4, "Mèo Lv5 chặn tối đa 5 viên/con");
                runtime.Advance(0.1981f);
                var familiarPool = pools.GetPool(familiar.projectilePrefab, 96);
                var projectiles = familiarPool.GetComponentsInChildren<SkillProjectile>();
                Check(projectiles.Length == 4 && runtime.ActiveFamiliarCount == 0, "4 mèo bắn 1 lượt đúng nhịp, hết giờ trả pool");
                foreach (var projectile in projectiles)
                    Check(Get<DamageSource>(projectile, "source") == DamageSource.Ultimate, "Đạn mèo có nguồn Ultimate");
                enemy.ClearAll();

                var beam = Load<StarBeamSkillSO>("StarBeam");
                target.transform.position = player.transform.position + Vector3.up * 2f;
                before = target.CurrentHP;
                Spawn(enemy, player.transform.position + Vector3.up);
                Spawn(enemy, player.transform.position + new Vector3(1f, 1f, 0f));
                float energy = caster.UltimateEnergy;
                runtime.BeginBeam(beam, beam.GetLevel(1));
                Check(shooting.SkillFireBlocked && controller.SkillSpeedMultiplier == 0.5f, "Beam khoá bắn thường và giảm tốc");
                runtime.Advance(0.1f);
                Check(target.CurrentHP == before - 30 && enemy.ActiveCount == 1, "Beam tick 10Hz, chỉ xoá đạn trên đường tia");
                runtime.Advance(1.9f);
                Check(target.CurrentHP == before - 600 && !shooting.SkillFireBlocked && controller.SkillSpeedMultiplier == 1f,
                    "Beam đủ 20 tick/2s và trả tốc độ/bắn thường");
                Check(caster.UltimateEnergy == energy, "Beam không tự nạp năng lượng ultimate");
                runtime.BeginBeam(beam, beam.GetLevel(5));
                runtime.BeginTimeStop(time, time.GetLevel(5));
                Call(runtime, "OnDisable");
                Check(!enemy.IsTimeStopped && !shooting.SkillFireBlocked && controller.SkillSpeedMultiplier == 1f,
                    "Huỷ component luôn trả freeze, di chuyển và bắn thường");
                Debug.Log("ER_GAMEPLAY_CHECKS_PASS: assets, debug switching, energy, shields, dispel, slow, freeze, teleport, meteors, familiars, beam, cleanup.");
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Time.timeScale = 1f;
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
        }

        private static void Press(SkillCaster caster, Key key)
        {
            // Nhánh đổi/nạp dùng bởi phím debug; input trong player loop được kiểm tra riêng.
            if (key == Key.F2) Call(caster, "CycleDebugSkill", SkillSlot.E);
            else if (key == Key.F3) Call(caster, "CycleDebugSkill", SkillSlot.R);
            else if (key == Key.F4) caster.AddUltimateEnergy(100f);
        }
        private static T Load<T>(string name) where T : SkillSO
        { return AssetDatabase.LoadAssetAtPath<T>("Assets/_Game/Data/Skills/Skill_" + name + ".asset"); }
        private static BulletBase Spawn(BulletPool pool, Vector2 position)
        { var bullet = pool.Get(position); if (bullet == null) throw new InvalidOperationException("Pool exhausted"); bullet.Launch(Vector2.up, 2f); return bullet; }
        private static BossHealth MakeTarget(Vector2 position)
        {
            var go = new GameObject("ERTestBoss"); go.transform.position = position;
            go.AddComponent<CircleCollider2D>().isTrigger = true;
            var health = go.AddComponent<BossHealth>();
            var so = new SerializedObject(health); so.FindProperty("maxHP").intValue = 10000; so.ApplyModifiedPropertiesWithoutUndo();
            Call(health, "Awake"); TargetRegistry.Register(health, go.transform); return health;
        }
        private static T Get<T>(object instance, string field)
        { return (T)instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance); }
        private static void Set(object instance, string field, object value)
        { instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value); }
        private static void Call(object instance, string method, params object[] arguments)
        {
            instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(instance, arguments);
        }

        private static void Check(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); Debug.Log("ER_CHECK_PASS: " + message); }
    }
}
