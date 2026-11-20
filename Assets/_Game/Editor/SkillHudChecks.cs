using System;
using System.IO;
using System.Reflection;
using Houkimajo.Player;
using Houkimajo.Skills;
using Houkimajo.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Houkimajo.EditorTools
{
    /// <summary>Kiểm tra dock/profile và xuất ảnh render thật, chỉ chạy trên bản sao project.</summary>
    public static class SkillHudChecks
    {
        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Chỉ dùng Unity batch trên bản sao project.");
            try
            {
                EditorSceneManager.OpenScene("Assets/_Game/Scenes/Stage01.unity");
                SkillBootstrapper.AddSkillsToCurrentScene();
                SkillBootstrapper.AddSkillsToCurrentScene();
                EditorSceneManager.SaveOpenScenes();
                var shooting = UnityEngine.Object.FindAnyObjectByType<PlayerShooting>();
                var bar = UnityEngine.Object.FindAnyObjectByType<SkillBarUI>();
                var canvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
                var views = canvas.GetComponentsInChildren<SkillHudSlot>();
                Check(views.Length == 4, "Build lại vẫn đúng 4 ô LMB/Q/E/R");
                var dock = (RectTransform)canvas.transform.Find("SkillDock");
                Check(dock.anchorMin == Vector2.zero && dock.anchorMax == Vector2.zero && dock.pivot == Vector2.zero,
                    "HUD neo góc trái dưới");
                Check(dock.rect.height == 132f, "HUD đáy gọn với avatar nhỏ nhô lên");
                bar.FitToSafeArea(new Rect(24f, 18f, 296f, 342f), 1f);
                Check(dock.anchoredPosition == new Vector2(24f, 18f)
                    && dock.rect.width * dock.localScale.x <= 296.01f, "Neo đúng safe area và co khi màn hình hẹp");
                bar.FitToSafeArea(new Rect(0f, 0f, 1100f, 360f), 1f);
                Check(views[0].transform.Find("Surface/Level").GetComponent<Text>().text == "I", "Cấp hiển thị số La Mã");
                Check(dock.Find("AvatarRim/AvatarClip").GetComponent<Mask>() != null,
                    "avatar tròn cắt bằng Mask UI");
                Check(canvas.transform.Find("SkillTextQ") == null && canvas.transform.Find("SkillIcon0") == null,
                    "Đã dọn HUD dọc cũ");
                Check(shooting.Attack != null && shooting.Attack.icon != null, "Profile bắn thường có icon");
                Check(shooting.CurrentDamage == 10 && Mathf.Approximately(shooting.CooldownTotal, 0.1f)
                    && shooting.CurrentBulletSpeed == 12f, "Lv1 giữ nguyên chỉ số bắn thường");
                var original = shooting.Attack;
                var test = ScriptableObject.CreateInstance<BasicAttackSO>();
                test.attributeName = "Lửa";
                test.levels = new[] { new BasicAttackLevelData(), new BasicAttackLevelData
                    { damage = 23, fireInterval = 0.2f, bulletSpeed = 9f } };
                int events = 0;
                shooting.OnAttackChanged += () => events++;
                shooting.EquipAttack(test, 99);
                Check(shooting.AttackLevel == 2 && shooting.CurrentDamage == 23
                    && Mathf.Approximately(shooting.CooldownTotal, 0.2f) && shooting.CurrentBulletSpeed == 9f,
                    "Đổi profile/cấp dùng đúng chỉ số và kẹp cấp tối đa");
                shooting.SetAttackLevel(1);
                Check(events == 2 && test.levels[1].damage == 23, "Phát event, không sửa dữ liệu profile");
                typeof(PlayerShooting).GetField("cooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shooting, 0.08f);
                Time.timeScale = 0f;
                Call(shooting, "Update");
                Check(Mathf.Approximately(shooting.CooldownRemaining, 0.08f), "Pause giữ nhịp bắn thường");
                Time.timeScale = 1f;
                shooting.EquipAttack(original, 1);
                UnityEngine.Object.DestroyImmediate(test);
                Call(bar, "RefreshIdentity");

                views[0].SetState(0.06f, 0.1f, 0, 1, true, true, 0f, false);
                views[1].SetState(4.2f, 7f, 0, 1, true, false, 0f, false);
                views[2].SetState(0f, 6f, 1, 1, true, false, 0f, false);
                views[3].SetState(65f, 100f, 0, 1, true, false, 0f, false, true);
                Check(views[3].transform.Find("Surface/Cooldown").GetComponent<Text>().text == "35%",
                    "Ô R hiển thị năng lượng thay cho giây hồi");
                Check(views[0].transform.Find("Surface/Cooldown").GetComponent<Text>().text == "",
                    "Bắn thường không nhấp nháy số hồi 0.1s");
                Check(views[1].transform.Find("Surface/Cooldown").GetComponent<Text>().text == "4.2",
                    "Ô Q hiển thị đúng số giây hồi");
                var recharge = views[0].transform.Find("Surface/Recharge").GetComponent<Image>();
                Check(recharge.sprite != null && Mathf.Approximately(recharge.fillAmount, 0.4f), "Thanh nạp có sprite và tiến độ thật");
                views[2].SetState(2f, 6f, 1, 2, true, false, 0f, false);
                Check(views[2].transform.Find("Surface/Charges").GetComponent<Text>().text == "1/2"
                    && views[2].transform.Find("Surface/Cooldown").GetComponent<Text>().text == "",
                    "Còn lượt dùng vẫn sáng, số lượt độc lập hồi chiêu");
                views[2].SetState(0f, 6f, 1, 1, true, false, 0f, false);
                Export(canvas, 1100, 360, "output/hud-preview.png");
                Export(canvas, 480, 360, "output/hud-preview-narrow.png");
                Debug.Log("SKILL_HUD_CHECKS_PASS: 4 slots, idempotence, real cooldown, profile swapping, upgrades, pause, charges, renders.");
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Time.timeScale = 1f;
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
        }

        private static void Export(Canvas canvas, int width, int height, string path)
        {
            GraphicsSettings.defaultRenderPipeline = null;
            QualitySettings.renderPipeline = null;
            var camera = new GameObject("HudPreviewCamera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.015f, 0.04f);
            camera.cullingMask = 1 << 5;
            camera.orthographic = true;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            foreach (var transform in canvas.GetComponentsInChildren<Transform>()) transform.gameObject.layer = 5;
            var dock = (RectTransform)canvas.transform.Find("SkillDock");
            UnityEngine.Object.FindAnyObjectByType<SkillBarUI>().FitToSafeArea(new Rect(0f, 0f, width, height), 1f);
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());
            RenderTexture.active = null;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(texture);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(camera.gameObject);
        }

        private static void Call(object instance, string method, params object[] arguments)
        {
            instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(instance, arguments);
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            Debug.Log("HUD_CHECK_PASS: " + message);
        }
    }
}
