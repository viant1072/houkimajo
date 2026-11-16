using System;
using System.IO;
using System.Reflection;
using Houkimajo.Boss;
using Houkimajo.Bullets;
using Houkimajo.Player;
using Houkimajo.Skills;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Houkimajo.EditorTools
{
    /// <summary>Smoke test player loop thật, chỉ dùng Unity batch trên bản sao project.</summary>
    public static class ERPlayModeSmokeChecks
    {
        private const string ActiveKey = "Houkimajo.ERSmoke.Active";
        private const string DoneKey = "Houkimajo.ERSmoke.Done";
        private const string ErrorKey = "Houkimajo.ERSmoke.Error";
        private static SkillCaster caster;
        private static SkillEffectController runtime;
        private static PlayerController controller;
        private static PlayerShooting shooting;
        private static BulletPool enemy;
        private static Keyboard keyboard;
        private static int phase;
        private static double nextAt;
        private static double deadline;
        private static bool ticking;
        private static float pausedRemaining;
        private static BulletBase frozenBullet;
        private static Vector3 frozenPosition;

        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Chỉ chạy trên bản sao project bằng batch.");
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Stage01.unity");
            SkillBootstrapper.AddSkillsToCurrentScene();
            EditorSceneManager.SaveOpenScenes();
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(DoneKey, false);
            SessionState.SetString(ErrorKey, "");
            Resume();
            EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        private static void Resume()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            EditorApplication.playModeStateChanged -= Changed;
            EditorApplication.playModeStateChanged += Changed;
            if (SessionState.GetBool(DoneKey, false) && !EditorApplication.isPlaying)
                EditorApplication.delayCall += Finish;
        }

        private static void Changed(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                phase = 0; ticking = true;
                nextAt = EditorApplication.timeSinceStartup + 0.5;
                deadline = EditorApplication.timeSinceStartup + 35;
                Application.logMessageReceived += CaptureError;
                EditorApplication.update += Tick;
            }
            else if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(DoneKey, false)) Finish();
        }

        private static void CaptureError(string message, string stack, LogType type)
        {
            // Unity Search index của bản sao có thể chưa tồn tại sau domain reload;
            // lỗi editor này không thuộc player loop hay gameplay đang kiểm tra.
            if (stack.Contains("UnityEditor.Search.SearchDatabase")) return;
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                if (SessionState.GetString(ErrorKey, "") == "") SessionState.SetString(ErrorKey, message);
        }

        private static void Tick()
        {
            if (!ticking) return;
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Play smoke timeout at phase " + phase);
                if (SessionState.GetString(ErrorKey, "") != "") throw new InvalidOperationException(SessionState.GetString(ErrorKey, ""));
                if (EditorApplication.timeSinceStartup < nextAt) return;
                nextAt = EditorApplication.timeSinceStartup + 0.15;
                switch (phase++)
                {
                    case 0:
                        caster = UnityEngine.Object.FindAnyObjectByType<SkillCaster>();
                        runtime = caster.GetComponent<SkillEffectController>();
                        controller = caster.GetComponent<PlayerController>();
                        shooting = caster.GetComponent<PlayerShooting>();
                        enemy = GameObject.Find("EnemyBulletPool").GetComponent<BulletPool>();
                        var boss = UnityEngine.Object.FindAnyObjectByType<BossHealth>();
                        typeof(BossHealth).GetField("maxHP", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(boss, 100000);
                        typeof(BossHealth).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(boss, null);
                        keyboard = InputSystem.AddDevice<Keyboard>();
                        keyboard.MakeCurrent();
                        Application.runInBackground = true;
                        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                        caster.GetComponent<PlayerHealth>().GrantInvincibility(30f);
                        foreach (var control in keyboard.allKeys) { _ = control.wasPressedThisFrame; }
                        Time.timeScale = 1f;
                        Send(Key.F2);
                        break;
                    case 1:
                        Check(caster.GetSkill(SkillSlot.E) is ShieldSkillSO, "F2 trong Play mode đổi Dash sang Shield");
                        Send(Key.E);
                        break;
                    case 2:
                        Check(caster.GetComponent<PlayerHealth>().ShieldHits > 0, "E bật khiên trong player loop thật");
                        Send(Key.F3);
                        break;
                    case 3:
                        Check(caster.GetSkill(SkillSlot.R) is FamiliarSummonSkillSO, "F3 trong Play mode đổi Meteor sang Familiar");
                        Send(Key.F4);
                        break;
                    case 4:
                        Check(caster.UltimateEnergy == 100f, "F4 trong Play mode nạp đầy R");
                        Send(Key.R);
                        break;
                    case 5:
                        Check(runtime.ActiveFamiliarCount == 2 && caster.UltimateEnergy == 0f, "R triệu hồi hai mèo và tiêu hao năng lượng");
                        Send();
                        nextAt = EditorApplication.timeSinceStartup + 0.4;
                        break;
                    case 6:
                        var familiar = (FamiliarSummonSkillSO)caster.GetSkill(SkillSlot.R);
                        Check(UnityEngine.Object.FindAnyObjectByType<SkillPools>().GetPool(familiar.projectilePrefab, 96)
                            .GetComponentsInChildren<SkillProjectile>().Length > 0, "Mèo thực sự bắn đạn có pool");
                        caster.Equip(SkillSlot.R, AssetDatabase.LoadAssetAtPath<StarBeamSkillSO>(
                            "Assets/_Game/Data/Skills/Skill_StarBeam.asset"), 1);
                        Send(Key.F4);
                        break;
                    case 7: Send(Key.R); break;
                    case 8:
                        Check(runtime.IsBeamActive && shooting.SkillFireBlocked && controller.SkillSpeedMultiplier == 0.5f,
                            "Beam đang chạy, khoá bắn thường và giảm tốc");
                        Send();
                        nextAt = EditorApplication.timeSinceStartup + 2.2;
                        break;
                    case 9:
                        Check(!runtime.IsBeamActive && !shooting.SkillFireBlocked && controller.SkillSpeedMultiplier == 1f,
                            "Beam tự hết trong Play mode và trả trạng thái");
                        caster.Equip(SkillSlot.R, AssetDatabase.LoadAssetAtPath<TimeStopSkillSO>(
                            "Assets/_Game/Data/Skills/Skill_TimeStop.asset"), 1);
                        Send(Key.F4);
                        break;
                    case 10: Send(Key.R); break;
                    case 11:
                        Check(enemy.IsTimeStopped && runtime.IsTimeStopActive, "R Ngưng Đọng đóng băng enemy pool");
                        Send();
                        var bullets = enemy.GetComponentsInChildren<BulletBase>();
                        frozenBullet = bullets.Length > 0 ? bullets[0] : null;
                        if (frozenBullet != null) frozenPosition = frozenBullet.transform.position;
                        pausedRemaining = (float)typeof(SkillEffectController).GetField("timeRemaining",
                            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(runtime);
                        Time.timeScale = 0f;
                        nextAt = EditorApplication.timeSinceStartup + 0.3;
                        break;
                    case 12:
                        float remaining = (float)typeof(SkillEffectController).GetField("timeRemaining",
                            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(runtime);
                        Check(Mathf.Approximately(remaining, pausedRemaining)
                            && (frozenBullet == null || frozenBullet.transform.position == frozenPosition),
                            "Pause giữ nguyên timer Ngưng Đọng và vị trí đạn");
                        Time.timeScale = 1f;
                        runtime.enabled = false;
                        Check(!enemy.IsTimeStopped && !shooting.SkillFireBlocked && controller.SkillSpeedMultiplier == 1f,
                            "OnDisable thật trả freeze và điều khiển");
                        Stop();
                        break;
                }
            }
            catch (Exception error)
            {
                SessionState.SetString(ErrorKey, error.ToString());
                Stop();
            }
        }

        private static void Send(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        private static void Stop()
        {
            ticking = false;
            Time.timeScale = 1f;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= CaptureError;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            SessionState.SetBool(DoneKey, true);
            EditorApplication.isPlaying = false;
        }

        private static void Finish()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            SessionState.SetBool(ActiveKey, false);
            string error = SessionState.GetString(ErrorKey, "");
            Directory.CreateDirectory("output");
            File.WriteAllText("output/er-playmode-result.txt", error == "" ? "ER_PLAYMODE_SMOKE_PASS" : error);
            if (error == "") Debug.Log("ER_PLAYMODE_SMOKE_PASS: F2/F3/F4, shield, familiar fire, beam expiry, freeze/pause, cleanup; no runtime errors.");
            else Debug.LogError(error);
            EditorApplication.Exit(error == "" ? 0 : 1);
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            Debug.Log("ER_PLAY_PASS: " + message);
        }
    }
}
