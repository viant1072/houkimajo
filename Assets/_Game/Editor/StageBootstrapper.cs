using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Houkimajo.Boss;
using Houkimajo.Bullets;
using Houkimajo.Enemies;
using Houkimajo.Player;
using Houkimajo.Systems;
using Houkimajo.UI;

namespace Houkimajo.EditorTools
{
    /// <summary>
    /// Dựng phần "nhiều màn" theo docs/systems.md + docs/enemies.md:
    /// - asset quái, wave, pattern, pha boss, 3 StageSO + StageCatalog
    /// - prefab quái + viên vàng
    /// - StageDirector/EnemySpawner/pool + chữ thông báo trên HUD trong scene đang mở
    /// - scene MainMenu + Build Settings (MainMenu trước, Stage01 sau)
    /// Asset đã tồn tại thì giữ nguyên, không ghi đè chỉ số người dùng đã chỉnh tay.
    /// </summary>
    public static class StageBootstrapper
    {
        private const string EnemyDataDir = "Assets/_Game/Data/Enemies";
        private const string WaveDir = "Assets/_Game/Data/Waves";
        private const string StageDir = "Assets/_Game/Data/Stages";
        private const string PatternDir = "Assets/_Game/Data/BulletPatterns";
        private const string PhaseDir = "Assets/_Game/Data/BossPhases";
        private const string EnemyPrefabDir = "Assets/_Game/Prefabs/Enemies";
        private const string MainMenuPath = "Assets/_Game/Scenes/MainMenu.unity";
        private const string GameplayPath = "Assets/_Game/Scenes/Stage01.unity";

        [MenuItem("Houkimajo/Setup Stages + Main Menu (3 màn, quái, menu chính)")]
        public static void SetupFromMenu()
        {
            if (Object.FindAnyObjectByType<PlayerController>() == null)
            {
                Debug.LogWarning("[Houkimajo] Mở scene Stage01 đã dựng (có Player) trước, rồi chạy lại.");
                return;
            }

            SetupStageObjects();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            BuildMainMenuScene();
            Debug.Log("[Houkimajo] Xong: 3 màn + quái + menu chính. Mở scene MainMenu rồi bấm Play " +
                "(hoặc Play thẳng Stage01 để test màn 1).");
        }

        // =====================================================================
        // Scene gameplay
        // =====================================================================

        /// <summary>Gắn hệ thống màn chơi vào scene đang mở (gọi từ Setup/Rebuild playtest scene).</summary>
        public static void SetupStageObjects()
        {
            var catalog = CreateOrLoadContent(out var enemyPrefab, out var pickupPrefab);

            var playerController = Object.FindAnyObjectByType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("[Houkimajo] Không thấy Player — bỏ qua phần StageDirector.");
                return;
            }
            var player = playerController.gameObject;
            var wallet = player.GetComponent<Wallet>();
            if (wallet == null) wallet = Undo.AddComponent<Wallet>(player);

            var enemyBulletPool = FindComponent<BulletPool>("EnemyBulletPool");
            if (enemyBulletPool != null)
            {
                // Nhiều quái cùng bắn -> pool đạn địch cần lớn hơn hẳn lúc chỉ có boss.
                var poolSO = new SerializedObject(enemyBulletPool);
                var size = poolSO.FindProperty("initialSize");
                if (size != null && size.intValue < 600) size.intValue = 600;
                poolSO.ApplyModifiedProperties();
            }

            var bossGo = GameObject.Find("Boss");

            // ---- StageSystems: director + spawner + 2 pool ----
            var systemsGo = GameObject.Find("StageSystems");
            if (systemsGo == null)
            {
                systemsGo = new GameObject("StageSystems");
                Undo.RegisterCreatedObjectUndo(systemsGo, "Create StageSystems");
            }

            var enemyPool = EnsureChildComponent<EnemyPool>(systemsGo, "EnemyPool");
            var enemyPoolSO = new SerializedObject(enemyPool);
            enemyPoolSO.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
            enemyPoolSO.ApplyModifiedProperties();

            var pickupPool = EnsureChildComponent<PickupPool>(systemsGo, "PickupPool");
            var pickupPoolSO = new SerializedObject(pickupPool);
            pickupPoolSO.FindProperty("pickupPrefab").objectReferenceValue = pickupPrefab;
            pickupPoolSO.ApplyModifiedProperties();

            var spawner = EnsureComponent<EnemySpawner>(systemsGo);
            var spawnerSO = new SerializedObject(spawner);
            spawnerSO.FindProperty("enemyPool").objectReferenceValue = enemyPool;
            spawnerSO.FindProperty("pickupPool").objectReferenceValue = pickupPool;
            spawnerSO.FindProperty("enemyBulletPool").objectReferenceValue = enemyBulletPool;
            spawnerSO.ApplyModifiedProperties();

            var director = EnsureComponent<StageDirector>(systemsGo);
            var directorSO = new SerializedObject(director);
            directorSO.FindProperty("catalog").objectReferenceValue = catalog;
            directorSO.FindProperty("spawner").objectReferenceValue = spawner;
            directorSO.FindProperty("pickupPool").objectReferenceValue = pickupPool;
            directorSO.FindProperty("enemyBulletPool").objectReferenceValue = enemyBulletPool;
            directorSO.FindProperty("playerHealth").objectReferenceValue = player.GetComponent<PlayerHealth>();
            directorSO.FindProperty("wallet").objectReferenceValue = wallet;
            if (bossGo != null)
            {
                directorSO.FindProperty("bossObject").objectReferenceValue = bossGo;
                directorSO.FindProperty("bossHealth").objectReferenceValue = bossGo.GetComponent<BossHealth>();
                directorSO.FindProperty("bossStateMachine").objectReferenceValue = bossGo.GetComponent<BossStateMachine>();
                directorSO.FindProperty("bossRenderer").objectReferenceValue = bossGo.GetComponent<SpriteRenderer>();
            }
            directorSO.ApplyModifiedProperties();

            SetupHud(director, wallet);
        }

        private static void SetupHud(StageDirector director, Wallet wallet)
        {
            var canvasGo = GameObject.Find("HUDCanvas");
            if (canvasGo == null)
            {
                Debug.LogWarning("[Houkimajo] Không thấy HUDCanvas — bỏ qua chữ thông báo màn.");
                return;
            }

            var infoText = EnsureHudText(canvasGo.transform, "StageInfoText", TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(20f, -52f), 20, new Vector2(480f, 30f));
            var bannerText = EnsureHudText(canvasGo.transform, "StageBannerText", TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), 42, new Vector2(900f, 70f));
            bannerText.fontStyle = FontStyle.Bold;
            bannerText.gameObject.SetActive(false);

            var hintText = EnsureHudText(canvasGo.transform, "EndStateHintText", TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), 22, new Vector2(800f, 40f));
            hintText.gameObject.SetActive(false);

            var hudGo = GameObject.Find("StageHudUI");
            if (hudGo == null)
            {
                hudGo = new GameObject("StageHudUI");
                Undo.RegisterCreatedObjectUndo(hudGo, "Create StageHudUI");
            }
            var stageHud = EnsureComponent<StageHudUI>(hudGo);
            var hudSO = new SerializedObject(stageHud);
            hudSO.FindProperty("director").objectReferenceValue = director;
            hudSO.FindProperty("wallet").objectReferenceValue = wallet;
            hudSO.FindProperty("infoText").objectReferenceValue = infoText;
            hudSO.FindProperty("bannerText").objectReferenceValue = bannerText;
            hudSO.ApplyModifiedProperties();

            var endUi = Object.FindAnyObjectByType<EndStateUI>();
            if (endUi != null)
            {
                var endSO = new SerializedObject(endUi);
                endSO.FindProperty("director").objectReferenceValue = director;
                endSO.FindProperty("hintText").objectReferenceValue = hintText;
                if (endSO.FindProperty("endStateText").objectReferenceValue == null)
                {
                    var endText = canvasGo.transform.Find("EndStateText");
                    if (endText != null)
                        endSO.FindProperty("endStateText").objectReferenceValue = endText.GetComponent<Text>();
                }
                endSO.ApplyModifiedProperties();
            }
        }

        // =====================================================================
        // Asset: quái, pattern, wave, pha boss, màn
        // =====================================================================

        private static StageCatalogSO CreateOrLoadContent(out EnemyController enemyPrefab, out Pickup pickupPrefab)
        {
            foreach (var dir in new[] { EnemyDataDir, WaveDir, StageDir, PatternDir, PhaseDir, EnemyPrefabDir })
                EnsureFolder(dir);

            // ---- Sprite placeholder ----
            var batSprite = SceneBootstrapper.CreatePlaceholderSprite("enemy_bat", new Color(0.55f, 0.35f, 0.75f), 32, true, pixelsPerUnit: 64);
            var fireflySprite = SceneBootstrapper.CreatePlaceholderSprite("enemy_firefly", new Color(1f, 0.75f, 0.25f), 24, true, pixelsPerUnit: 64);
            var pumpkinSprite = SceneBootstrapper.CreatePlaceholderSprite("enemy_pumpkin", new Color(1f, 0.5f, 0.1f), 40, true, pixelsPerUnit: 64);
            var goldSprite = SceneBootstrapper.CreatePlaceholderSprite("pickup_gold", new Color(1f, 0.85f, 0.2f), 12, true, pixelsPerUnit: 64);

            enemyPrefab = CreateOrLoadEnemyPrefab(batSprite);
            pickupPrefab = CreateOrLoadPickupPrefab(goldSprite);

            // ---- Pattern đạn ----
            var fireflyShot = Pattern("Enemy_FireflyAimed", PatternShape.Aimed, 1, 3f, 1.6f, 0f);
            var pumpkinShot = Pattern("Enemy_PumpkinRadial", PatternShape.Radial, 8, 2.2f, 1.8f, 0f);

            var s2Radial = Pattern("S2_RadialDense", PatternShape.Radial, 18, 2.8f, 0.55f, 0f);
            var s2Spiral = Pattern("S2_SpiralFast", PatternShape.Spiral, 8, 3.2f, 0.12f, 13f);
            var s2Aimed = Pattern("S2_AimedFast", PatternShape.Aimed, 1, 5.5f, 0.15f, 0f);

            var s3Spiral = Pattern("S3_Spiral", PatternShape.Spiral, 10, 3f, 0.12f, 11f);
            var s3Radial = Pattern("S3_RadialHeavy", PatternShape.Radial, 26, 3f, 0.5f, 0f);
            var s3Frenzy = Pattern("S3_SpiralFrenzy", PatternShape.Spiral, 12, 3.6f, 0.08f, 17f);

            // ---- Quái (chỉ số theo docs/enemies.md) ----
            var bat = Enemy("Enemy_Bat", "enemy_bat", "Dơi Bóng Đêm", batSprite, Color.white, 0.22f,
                20, 2.4f, EnemyMovement.Chase, 0f, null, 2, 3);
            var firefly = Enemy("Enemy_Firefly", "enemy_firefly", "Đom Đóm Lửa", fireflySprite, Color.white, 0.17f,
                30, 2.2f, EnemyMovement.Orbit, 3.2f, fireflyShot, 3, 5);
            var pumpkin = Enemy("Enemy_Pumpkin", "enemy_pumpkin", "Bí Ngô Bay", pumpkinSprite, Color.white, 0.28f,
                50, 1.3f, EnemyMovement.Strafe, 0f, pumpkinShot, 5, 8);

            // ---- Wave ----
            var s1w1 = Wave("S1_W1", (bat, 8, 0.4f));
            var s1w2 = Wave("S1_W2", (bat, 6, 0.35f), (firefly, 3, 0.8f));

            var s2w1 = Wave("S2_W1", (bat, 10, 0.3f), (firefly, 3, 0.7f));
            var s2w2 = Wave("S2_W2", (pumpkin, 4, 1f), (bat, 8, 0.3f));
            var s2w3 = Wave("S2_W3", (firefly, 5, 0.6f), (pumpkin, 4, 0.9f));

            var s3w1 = Wave("S3_W1", (bat, 14, 0.25f), (firefly, 5, 0.6f));
            var s3w2 = Wave("S3_W2", (pumpkin, 6, 0.8f), (firefly, 6, 0.5f));
            var s3w3 = Wave("S3_W3", (bat, 16, 0.2f), (pumpkin, 6, 0.7f), (firefly, 6, 0.5f));

            // ---- Pha boss ----
            var s1Phases = new List<BossPhaseSO>
            {
                AssetDatabase.LoadAssetAtPath<BossPhaseSO>($"{PhaseDir}/Test_Phase1.asset"),
                AssetDatabase.LoadAssetAtPath<BossPhaseSO>($"{PhaseDir}/Test_Phase2.asset"),
                AssetDatabase.LoadAssetAtPath<BossPhaseSO>($"{PhaseDir}/Test_Phase3.asset"),
            };
            s1Phases.RemoveAll(phase => phase == null);

            var s2Phases = new List<BossPhaseSO>
            {
                Phase("S2_Phase1", "Mưa Hạt Bí", 1f, s2Radial),
                Phase("S2_Phase2", "Xoáy Đầm Lầy", 0.66f, s2Spiral),
                Phase("S2_Phase3", "Ánh Mắt Ma Trơi", 0.33f, s2Aimed),
            };
            var s3Phases = new List<BossPhaseSO>
            {
                Phase("S3_Phase1", "Vòng Xoáy Sao", 1f, s3Spiral),
                Phase("S3_Phase2", "Bức Tường Phép", 0.66f, s3Radial),
                Phase("S3_Phase3", "Cơn Lốc Cuối Cùng", 0.33f, s3Frenzy),
            };

            // ---- Màn ----
            var stage1 = Stage("Stage_01", "stage_01", "Rừng Đêm", new[] { s1w1, s1w2 },
                s1Phases, 1000, Color.white, 100);
            var stage2 = Stage("Stage_02", "stage_02", "Đầm Lầy Bí Ngô", new[] { s2w1, s2w2, s2w3 },
                s2Phases, 1600, new Color(1f, 0.65f, 0.35f), 200);
            var stage3 = Stage("Stage_03", "stage_03", "Tháp Phù Thuỷ", new[] { s3w1, s3w2, s3w3 },
                s3Phases, 2400, new Color(0.55f, 0.75f, 1f), 400);

            string catalogPath = $"{StageDir}/StageCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalogSO>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<StageCatalogSO>();
                catalog.stages = new List<StageSO> { stage1, stage2, stage3 };
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }

            EnemyGameplayBuilder.Apply();
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static EnemyController CreateOrLoadEnemyPrefab(Sprite sprite)
        {
            string path = $"{EnemyPrefabDir}/Enemy.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<EnemyController>();

            var go = new GameObject("Enemy");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 2;
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.22f;
            go.AddComponent<BulletEmitter>();
            go.AddComponent<EnemyController>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<EnemyController>();
        }

        private static Pickup CreateOrLoadPickupPrefab(Sprite sprite)
        {
            string path = $"{EnemyPrefabDir}/GoldPickup.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<Pickup>();

            var go = new GameObject("GoldPickup");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 1;
            go.AddComponent<Pickup>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Pickup>();
        }

        private static BulletPatternSO Pattern(string assetName, PatternShape shape, int count,
            float speed, float interval, float rotation)
        {
            string path = $"{PatternDir}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<BulletPatternSO>(path);
            if (existing != null) return existing;

            var pattern = ScriptableObject.CreateInstance<BulletPatternSO>();
            pattern.shape = shape;
            pattern.bulletCount = count;
            pattern.bulletSpeed = speed;
            pattern.fireInterval = interval;
            pattern.rotationPerShot = rotation;
            AssetDatabase.CreateAsset(pattern, path);
            return pattern;
        }

        private static EnemyDataSO Enemy(string assetName, string id, string displayName, Sprite sprite,
            Color tint, float radius, int hp, float speed, EnemyMovement movement, float orbitRadius,
            BulletPatternSO pattern, int gold, int xp)
        {
            string path = $"{EnemyDataDir}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
            if (existing != null) return existing;

            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            data.id = id;
            data.displayName = displayName;
            data.sprite = sprite;
            data.tint = tint;
            data.colliderRadius = radius;
            data.maxHP = hp;
            data.moveSpeed = speed;
            data.movement = movement;
            if (orbitRadius > 0f) data.orbitRadius = orbitRadius;
            data.bulletPattern = pattern;
            data.reward = new RewardData { gold = gold, xp = xp };
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static WaveSO Wave(string assetName, params (EnemyDataSO enemy, int count, float interval)[] groups)
        {
            string path = $"{WaveDir}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<WaveSO>(path);
            if (existing != null) return existing;

            var wave = ScriptableObject.CreateInstance<WaveSO>();
            foreach (var group in groups)
            {
                wave.entries.Add(new WaveEntry { enemy = group.enemy, count = group.count, spawnInterval = group.interval });
            }
            AssetDatabase.CreateAsset(wave, path);
            return wave;
        }

        private static BossPhaseSO Phase(string assetName, string phaseName, float threshold, BulletPatternSO pattern)
        {
            string path = $"{PhaseDir}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<BossPhaseSO>(path);
            if (existing != null) return existing;

            var phase = ScriptableObject.CreateInstance<BossPhaseSO>();
            phase.phaseName = phaseName;
            phase.hpThreshold = threshold;
            phase.bulletPattern = pattern;
            phase.maxDuration = 9999f; // chỉ chuyển pha theo HP
            AssetDatabase.CreateAsset(phase, path);
            return phase;
        }

        private static StageSO Stage(string assetName, string id, string stageName, WaveSO[] waves,
            List<BossPhaseSO> phases, int bossHP, Color bossTint, int clearGold)
        {
            string path = $"{StageDir}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<StageSO>(path);
            if (existing != null) return existing;

            var stage = ScriptableObject.CreateInstance<StageSO>();
            stage.id = id;
            stage.stageName = stageName;
            foreach (var wave in waves) stage.waves.Add(new StageWave { wave = wave });
            stage.bossPhases = phases;
            stage.bossMaxHP = bossHP;
            stage.bossTint = bossTint;
            stage.clearGold = clearGold;
            AssetDatabase.CreateAsset(stage, path);
            return stage;
        }

        // =====================================================================
        // Scene menu chính + Build Settings
        // =====================================================================

        /// <summary>Tạo scene MainMenu (nếu chưa có) + đặt Build Settings = [MainMenu, Stage01].
        /// Gọi cả từ Setup/Rebuild Playtest Scene, để không ai quên bước này.</summary>
        internal static void BuildMainMenuScene()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalogSO>($"{StageDir}/StageCatalog.asset");
            var previousActive = SceneManager.GetActiveScene();

            bool alreadyExists = File.Exists(MainMenuPath);
            if (!alreadyExists)
            {
                // Tạo additive để không phải đóng scene gameplay đang mở.
                var menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(menuScene);
                PopulateMainMenu(catalog);
                EditorSceneManager.SaveScene(menuScene, MainMenuPath);
                SceneManager.SetActiveScene(previousActive);
                EditorSceneManager.CloseScene(menuScene, true);
            }
            else
            {
                Debug.Log("[Houkimajo] Scene MainMenu đã có — giữ nguyên (xoá file đi nếu muốn dựng lại).");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(GameplayPath, true),
            };
        }

        private static void PopulateMainMenu(StageCatalogSO catalog)
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.06f, 0.14f);
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            // Input System mới: dùng InputSystemUIInputModule, KHÔNG dùng StandaloneInputModule.
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            var canvasGo = new GameObject("MenuCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var title = SceneBootstrapper.CreateHudText(canvasGo.transform, "TitleText", "HOUKIMAJO",
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 250f));
            title.fontSize = 96;
            title.fontStyle = FontStyle.Bold;
            title.rectTransform.sizeDelta = new Vector2(1200f, 140f);

            var subtitle = SceneBootstrapper.CreateHudText(canvasGo.transform, "SubtitleText", "Cô phù thuỷ cưỡi chổi",
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 160f));
            subtitle.fontSize = 32;
            subtitle.rectTransform.sizeDelta = new Vector2(1200f, 50f);

            var continueButton = CreateButton(canvasGo.transform, "ContinueButton", "Chơi tiếp", new Vector2(0f, 10f), out var continueLabel);
            var newGameButton = CreateButton(canvasGo.transform, "NewGameButton", "Chơi mới", new Vector2(0f, -100f), out var newGameLabel);

            var progress = SceneBootstrapper.CreateHudText(canvasGo.transform, "ProgressText", "",
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -210f));
            progress.fontSize = 26;
            progress.color = new Color(0.8f, 0.8f, 0.9f);
            progress.rectTransform.sizeDelta = new Vector2(1200f, 40f);

            var hint = SceneBootstrapper.CreateHudText(canvasGo.transform, "HintText", "Enter: bắt đầu nhanh",
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0f, 60f));
            hint.fontSize = 22;
            hint.color = new Color(0.6f, 0.6f, 0.7f);
            hint.rectTransform.sizeDelta = new Vector2(1200f, 40f);

            var menuGo = new GameObject("MainMenuUI");
            var menu = menuGo.AddComponent<MainMenuUI>();
            var menuSO = new SerializedObject(menu);
            menuSO.FindProperty("catalog").objectReferenceValue = catalog;
            menuSO.FindProperty("continueButton").objectReferenceValue = continueButton;
            menuSO.FindProperty("continueLabel").objectReferenceValue = continueLabel;
            menuSO.FindProperty("newGameButton").objectReferenceValue = newGameButton;
            menuSO.FindProperty("newGameLabel").objectReferenceValue = newGameLabel;
            menuSO.FindProperty("progressText").objectReferenceValue = progress;
            menuSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, out Text labelText)
        {
            var go = DefaultControls.CreateButton(new DefaultControls.Resources());
            go.name = name;
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(520f, 84f);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.32f, 0.2f, 0.5f);

            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1.25f, 1.2f, 1.35f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
            button.colors = colors;

            labelText = go.GetComponentInChildren<Text>();
            labelText.text = label;
            labelText.font = SceneBootstrapper.GetDefaultFont();
            labelText.fontSize = 34;
            labelText.color = Color.white;
            return button;
        }

        // =====================================================================
        // Tiện ích
        // =====================================================================

        private static Text EnsureHudText(Transform canvas, string name, TextAnchor alignment, Vector2 anchor,
            Vector2 position, int fontSize, Vector2 size)
        {
            var existing = canvas.Find(name);
            Text text = existing != null
                ? existing.GetComponent<Text>()
                : SceneBootstrapper.CreateHudText(canvas, name, "", alignment, anchor, position);
            text.fontSize = fontSize;
            text.rectTransform.sizeDelta = size;
            return text;
        }

        private static T FindComponent<T>(string objectName) where T : Component
        {
            var go = GameObject.Find(objectName);
            return go != null ? go.GetComponent<T>() : null;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(go);
        }

        private static T EnsureChildComponent<T>(GameObject parent, string childName) where T : Component
        {
            var child = parent.transform.Find(childName);
            GameObject go;
            if (child != null)
            {
                go = child.gameObject;
            }
            else
            {
                go = new GameObject(childName);
                Undo.RegisterCreatedObjectUndo(go, $"Create {childName}");
                go.transform.SetParent(parent.transform, false);
            }
            return EnsureComponent<T>(go);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}
