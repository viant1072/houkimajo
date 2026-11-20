using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Houkimajo.Player;
using Houkimajo.Bullets;
using Houkimajo.Boss;
using Houkimajo.Systems;
using Houkimajo.UI;

namespace Houkimajo.EditorTools
{
    /// <summary>
    /// Tự động dựng 1 scene chơi thử được: Player, 2 BulletPool, 2 Bullet prefab,
    /// Boss, 1 BulletPattern + 1 BossPhase test — tất cả dùng sprite hình khối màu
    /// tạm (placeholder), chưa có art thật. Mục đích: có vertical slice chạy được
    /// ngay để test cảm giác gameplay trước khi đầu tư vẽ art (xem docs/).
    ///
    /// Dùng: mở scene Assets/_Game/Scenes/Stage01.unity, vào menu
    /// Houkimajo → Setup Playtest Scene. Chạy lại an toàn (có check trùng).
    /// </summary>
    public static class SceneBootstrapper
    {
        private const float PlayerVisualScale = 1.2f;

        [MenuItem("Houkimajo/Setup Playtest Scene (Placeholder Art)")]
        public static void SetupScene()
        {
            if (GameObject.Find("Player") != null || GameObject.Find("Boss") != null)
            {
                Debug.LogWarning("[Houkimajo] Đã thấy Player hoặc Boss trong scene — " +
                    "huỷ để tránh tạo trùng. Xoá thủ công trước nếu muốn dựng lại từ đầu.");
                return;
            }

            EnsureTagExists("Player");

            // ---- Sprite placeholder ----
            var playerSprite = CreatePlaceholderSprite("player_witch", new Color(0.4f, 0.8f, 1f), 48, false);
            var playerBulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/_Game/Art/Sprites/Bullets/bullet_player_arcane_v1.png")
                ?? CreatePlaceholderSprite("bullet_player", Color.yellow, 16, true);
            var enemyBulletSprite = CreatePlaceholderSprite("bullet_enemy", new Color(1f, 0.2f, 0.2f), 16, true);
            var bossSprite = CreatePlaceholderSprite("boss", new Color(0.8f, 0.2f, 0.8f), 96, false);
            // PPU = 64 cho 2 sprite dưới -> hitbox ~0.25 đơn vị (khớp collider 0.12), tâm ngắm ~0.5
            var hitboxSprite = CreatePlaceholderSprite("hitbox", Color.white, 16, true, pixelsPerUnit: 64);
            var crosshairSprite = CreatePlaceholderSprite("crosshair", new Color(0.3f, 1f, 0.4f), 32, true,
                pixelsPerUnit: 64, innerRadiusRatio: 0.7f);

            // ---- Bullet prefab + pool ----
            var playerBulletPrefab = CreateBulletPrefab("PlayerBullet", playerBulletSprite, true);
            var enemyBulletPrefab = CreateBulletPrefab("EnemyBullet", enemyBulletSprite);

            var playerPool = CreatePool("PlayerBulletPool", playerBulletPrefab);
            var enemyPool = CreatePool("EnemyBulletPool", enemyBulletPrefab);

            // ---- Player ----
            var player = new GameObject("Player");
            player.tag = "Player";
            var playerRenderer = CreatePlayerVisual(player, playerSprite);
            player.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            var playerCollider = player.AddComponent<CircleCollider2D>();
            playerCollider.isTrigger = true;
            playerCollider.radius = 0.12f;

            var playerHealth = player.AddComponent<PlayerHealth>();
            var aim = player.AddComponent<PlayerAim>();
            var shooting = player.AddComponent<PlayerShooting>();
            player.AddComponent<PlayerController>();
            var visuals = player.AddComponent<PlayerVisuals>();

            var shootingSO = new SerializedObject(shooting);
            shootingSO.FindProperty("bulletPool").objectReferenceValue = playerPool;
            shootingSO.ApplyModifiedProperties();

            // Chấm hitbox: chỉ hiện khi giữ Left Control, vẽ đè lên người player
            var hitbox = new GameObject("HitboxIndicator");
            hitbox.transform.SetParent(player.transform, false);
            var hitboxRenderer = hitbox.AddComponent<SpriteRenderer>();
            hitboxRenderer.sprite = hitboxSprite;
            hitboxRenderer.sortingOrder = 10;
            hitbox.SetActive(false);

            // Vùng graze: collider con lớn hơn hitbox, dùng chung Rigidbody2D của Player
            var grazeZone = new GameObject("GrazeZone");
            grazeZone.transform.SetParent(player.transform, false);
            var grazeCollider = grazeZone.AddComponent<CircleCollider2D>();
            grazeCollider.isTrigger = true;
            grazeCollider.radius = 0.5f;
            grazeZone.AddComponent<GrazeDetector>();

            // Tâm ngắm: object riêng ngoài Player, PlayerAim kéo nó theo chuột mỗi frame
            var crosshair = new GameObject("Crosshair");
            var crosshairRenderer = crosshair.AddComponent<SpriteRenderer>();
            crosshairRenderer.sprite = crosshairSprite;
            crosshairRenderer.sortingOrder = 20;

            var aimSO = new SerializedObject(aim);
            aimSO.FindProperty("crosshair").objectReferenceValue = crosshair.transform;
            aimSO.ApplyModifiedProperties();

            var visualsSO = new SerializedObject(visuals);
            visualsSO.FindProperty("bodyRenderer").objectReferenceValue = playerRenderer;
            visualsSO.FindProperty("hitboxIndicator").objectReferenceValue = hitbox;
            visualsSO.ApplyModifiedProperties();

            player.transform.position = new Vector3(0f, -2.5f, 0f);

            // ---- Boss ----
            var boss = new GameObject("Boss");
            boss.AddComponent<SpriteRenderer>().sprite = bossSprite;
            boss.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            var bossCollider = boss.AddComponent<CircleCollider2D>();
            bossCollider.isTrigger = true;
            bossCollider.radius = 0.9f;

            var bossHealth = boss.AddComponent<BossHealth>();
            var emitter = boss.AddComponent<BulletEmitter>();
            var stateMachine = boss.AddComponent<BossStateMachine>();

            var emitterSO = new SerializedObject(emitter);
            emitterSO.FindProperty("bulletPool").objectReferenceValue = enemyPool;
            emitterSO.ApplyModifiedProperties();

            boss.transform.position = new Vector3(0f, 3f, 0f);

            // ---- Data asset test (3 pha: Radial -> Spiral -> Aimed) ----
            var phases = CreateOrLoadPhases();

            var smSO = new SerializedObject(stateMachine);
            var phasesProp = smSO.FindProperty("phases");
            phasesProp.arraySize = phases.Count;
            for (int i = 0; i < phases.Count; i++)
            {
                phasesProp.GetArrayElementAtIndex(i).objectReferenceValue = phases[i];
            }
            smSO.FindProperty("emitter").objectReferenceValue = emitter;
            smSO.FindProperty("health").objectReferenceValue = bossHealth;
            smSO.ApplyModifiedProperties();

            // ---- Camera ----
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 5.5f;
                cam.transform.position = new Vector3(0f, 0f, -10f);
            }

            // ---- HUD debug (score / lives / boss HP) ----
            CreateHud(playerHealth, bossHealth);

            // ---- Skill Q/E/R (giai đoạn 4a) ----
            SkillBootstrapper.Setup(player, enemyPool);

            // ---- Quái, wave, 3 màn, StageDirector (giai đoạn 2) ----
            StageBootstrapper.SetupStageObjects();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            // Thiếu MainMenu trong Build Settings thì Esc/phá đảo sẽ lỗi load scene — dựng luôn ở đây.
            StageBootstrapper.BuildMainMenuScene();
            Debug.Log("[Houkimajo] Dựng scene xong — Ctrl+S để lưu, rồi bấm Play " +
                "(WASD di chuyển, chuột trái bắn, Left Shift lướt, Left Control bay chậm, Q/E/R = skill).");
        }

        /// <summary>
        /// Tên các object gốc mà SetupScene() tạo ra — Rebuild xoá đúng những object này,
        /// không đụng vào Main Camera hay object người dùng tự thêm vào scene.
        /// </summary>
        private static readonly string[] GeneratedRootNames =
        {
            "Player", "Boss", "Crosshair", "PlayerBulletPool", "EnemyBulletPool",
            "GameManager", "ScoreManager", "HUDCanvas", "HUDController", "EndStateUI",
            "SkillPools", "SkillBarUI", "StageSystems", "StageHudUI", "StageBackground"
        };

        [MenuItem("Houkimajo/Rebuild Playtest Scene (xoá object cũ rồi dựng lại)")]
        public static void RebuildScene()
        {
            foreach (var rootName in GeneratedRootNames)
            {
                // Lặp vì có thể có nhiều object trùng tên (lỡ dựng 2 lần)
                GameObject go;
                while ((go = GameObject.Find(rootName)) != null)
                {
                    Undo.DestroyObjectImmediate(go);
                }
            }

            SetupScene();
        }

        private static SpriteRenderer CreatePlayerVisual(GameObject player, Sprite fallback)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform, false);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = fallback;

            // Sắp theo tên để thứ tự phát không phụ thuộc thứ tự Unity trả sub-asset.
            var frames = new List<Sprite>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(
                "Assets/_Game/Art/Sprites/Player/player_witch_flight_v1.png"))
            {
                if (asset is Sprite sprite) frames.Add(sprite);
            }
            frames.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
            if (frames.Count != 4) return renderer;

            visual.transform.localScale = new Vector3(PlayerVisualScale, PlayerVisualScale, 1f);
            renderer.sprite = frames[0];
            var animation = visual.AddComponent<PlayerFlightAnimation>();
            var animationSO = new SerializedObject(animation);
            var framesProperty = animationSO.FindProperty("frames");
            framesProperty.arraySize = frames.Count;
            for (int i = 0; i < frames.Count; i++)
            {
                framesProperty.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }
            animationSO.ApplyModifiedProperties();
            return renderer;
        }

        [MenuItem("Houkimajo/Apply Player Flight Animation")]
        public static void ApplyPlayerFlightAnimation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var controller = Object.FindAnyObjectByType<PlayerController>();
            if (controller == null)
            {
                Debug.LogWarning("[Houkimajo] Mở scene có Player trước khi gắn animation.");
                return;
            }

            var player = controller.gameObject;
            var existing = player.GetComponentInChildren<PlayerFlightAnimation>(true);
            if (existing != null)
            {
                Undo.RecordObject(existing.transform, "Resize player visual");
                existing.transform.localScale = new Vector3(PlayerVisualScale, PlayerVisualScale, 1f);
                EditorSceneManager.MarkSceneDirty(player.scene);
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            if (AssetDatabase.LoadAllAssetsAtPath(
                "Assets/_Game/Art/Sprites/Player/player_witch_flight_v1.png").Length < 5)
            {
                Debug.LogWarning("[Houkimajo] Chưa import đủ 4 frame bay, kiểm tra sheet trong Art/Sprites/Player.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(player, "Apply player flight animation");
            var oldRenderer = player.GetComponent<SpriteRenderer>();
            var renderer = CreatePlayerVisual(player, oldRenderer != null ? oldRenderer.sprite : null);
            Undo.RegisterCreatedObjectUndo(renderer.gameObject, "Create player visual");
            if (oldRenderer != null)
            {
                renderer.sharedMaterial = oldRenderer.sharedMaterial;
                renderer.sortingLayerID = oldRenderer.sortingLayerID;
                renderer.sortingOrder = oldRenderer.sortingOrder;
                oldRenderer.enabled = false;
            }

            var visuals = player.GetComponent<PlayerVisuals>();
            if (visuals == null) visuals = Undo.AddComponent<PlayerVisuals>(player);
            var visualsSO = new SerializedObject(visuals);
            visualsSO.FindProperty("bodyRenderer").objectReferenceValue = renderer;
            visualsSO.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(player.scene);
            Selection.activeGameObject = renderer.gameObject;
            Debug.Log("[Houkimajo] Đã gắn animation bay. Ctrl+S để lưu, rồi bấm Play.");
        }

        private static GameObject CreateBulletPrefab(string name, Sprite sprite, bool alignToDirection = false)
        {
            const string dir = "Assets/_Game/Prefabs/Bullets";
            string path = $"{dir}/{name}.prefab";

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                // Prefab đạn player đã có từ placeholder vẫn phải nhận art mới khi Rebuild.
                if (alignToDirection)
                {
                    existing.GetComponent<SpriteRenderer>().sprite = sprite;
                    var bulletSO = new SerializedObject(existing.GetComponent<BulletBase>());
                    bulletSO.FindProperty("alignToDirection").boolValue = true;
                    bulletSO.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SavePrefabAsset(existing);
                }
                return existing;
            }

            var go = new GameObject(name);
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.08f;
            var bullet = go.AddComponent<BulletBase>();
            var newBulletSO = new SerializedObject(bullet);
            newBulletSO.FindProperty("alignToDirection").boolValue = alignToDirection;
            newBulletSO.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static BulletPool CreatePool(string name, GameObject bulletPrefab)
        {
            var existingGo = GameObject.Find(name);
            if (existingGo != null) return existingGo.GetComponent<BulletPool>();

            var go = new GameObject(name);
            var pool = go.AddComponent<BulletPool>();
            var so = new SerializedObject(pool);
            so.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab.GetComponent<BulletBase>();
            so.ApplyModifiedProperties();
            return pool;
        }

        /// <summary>
        /// Tạo (hoặc load nếu đã có) 3 pattern đạn + 3 pha boss, độ khó tăng dần khi
        /// HP giảm — để thấy rõ BossStateMachine chuyển pha lúc playtest.
        /// </summary>
        private static List<BossPhaseSO> CreateOrLoadPhases()
        {
            var radial = CreateOrLoadPattern("Test_Radial", PatternShape.Radial,
                bulletCount: 12, bulletSpeed: 2.5f, fireInterval: 0.6f, rotationPerShot: 0f);
            var spiral = CreateOrLoadPattern("Test_Spiral", PatternShape.Spiral,
                bulletCount: 6, bulletSpeed: 3f, fireInterval: 0.15f, rotationPerShot: 15f);
            var aimed = CreateOrLoadPattern("Test_Aimed", PatternShape.Aimed,
                bulletCount: 1, bulletSpeed: 4f, fireInterval: 0.25f, rotationPerShot: 0f);

            var phase1 = CreateOrLoadPhase("Test_Phase1", "Phase 1 - Radial", hpThreshold: 1f, radial);
            var phase2 = CreateOrLoadPhase("Test_Phase2", "Phase 2 - Spiral", hpThreshold: 0.66f, spiral);
            var phase3 = CreateOrLoadPhase("Test_Phase3", "Phase 3 - Aimed (rage)", hpThreshold: 0.33f, aimed);

            return new List<BossPhaseSO> { phase1, phase2, phase3 };
        }

        private static BulletPatternSO CreateOrLoadPattern(string assetName, PatternShape shape,
            int bulletCount, float bulletSpeed, float fireInterval, float rotationPerShot)
        {
            string path = $"Assets/_Game/Data/BulletPatterns/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<BulletPatternSO>(path);
            if (existing != null) return existing;

            var pattern = ScriptableObject.CreateInstance<BulletPatternSO>();
            pattern.shape = shape;
            pattern.bulletCount = bulletCount;
            pattern.bulletSpeed = bulletSpeed;
            pattern.fireInterval = fireInterval;
            pattern.rotationPerShot = rotationPerShot;
            AssetDatabase.CreateAsset(pattern, path);
            return pattern;
        }

        private static BossPhaseSO CreateOrLoadPhase(string assetName, string phaseName,
            float hpThreshold, BulletPatternSO pattern)
        {
            string path = $"Assets/_Game/Data/BossPhases/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<BossPhaseSO>(path);
            if (existing != null) return existing;

            var phase = ScriptableObject.CreateInstance<BossPhaseSO>();
            phase.phaseName = phaseName;
            phase.hpThreshold = hpThreshold;
            phase.bulletPattern = pattern;
            phase.maxDuration = 9999f; // test: không ép chuyển pha theo giờ, chỉ theo HP
            AssetDatabase.CreateAsset(phase, path);
            return phase;
        }

        /// <param name="pixelsPerUnit">0 = bằng size (sprite rộng đúng 1 đơn vị world).</param>
        /// <param name="innerRadiusRatio">&gt; 0 = vẽ vòng tròn rỗng (dùng cho tâm ngắm).</param>
        internal static Sprite CreatePlaceholderSprite(string name, Color color, int size, bool circle,
            int pixelsPerUnit = 0, float innerRadiusRatio = 0f)
        {
            const string dir = "Assets/_Game/Art/Sprites/_Placeholder";
            string path = $"{dir}/{name}.png";

            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            if (!AssetDatabase.IsValidFolder(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    bool inside = !circle || (dist <= radius && dist >= radius * innerRadiusRatio);
                    tex.SetPixel(x, y, inside ? color : new Color(0f, 0f, 0f, 0f));
                }
            }
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit > 0 ? pixelsPerUnit : size;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void CreateHud(PlayerHealth playerHealth, BossHealth bossHealth)
        {
            EnsureManager<GameManager>("GameManager");
            EnsureManager<ScoreManager>("ScoreManager");

            if (GameObject.Find("HUDCanvas") != null) return; // đã có, tránh tạo trùng

            var canvasGo = new GameObject("HUDCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            // Không tạo EventSystem — HUD chỉ hiện text, không có nút/tương tác nên
            // không cần. EventSystem + StandaloneInputModule (input kiểu cũ) xung đột
            // với project đang dùng Input System mới, gây lỗi mỗi frame lúc Play.

            var scoreText = CreateHudText(canvasGo.transform, "ScoreText", "Score: 0",
                TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(20f, -20f));
            var livesText = CreateHudText(canvasGo.transform, "LivesText", "Lives: 3",
                TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-20f, -20f));
            var bossHpText = CreateHudText(canvasGo.transform, "BossHPText", "Boss HP: -/-",
                TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -20f));

            var hudGo = new GameObject("HUDController");
            var hud = hudGo.AddComponent<HUDController>();
            var hudSO = new SerializedObject(hud);
            hudSO.FindProperty("scoreText").objectReferenceValue = scoreText;
            hudSO.FindProperty("livesText").objectReferenceValue = livesText;
            hudSO.FindProperty("bossHpText").objectReferenceValue = bossHpText;
            hudSO.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            hudSO.FindProperty("bossHealth").objectReferenceValue = bossHealth;
            hudSO.ApplyModifiedProperties();

            // Chữ to giữa màn hình cho Game Over / Victory, ẩn sẵn — EndStateUI bật lên khi cần
            var endStateText = CreateHudText(canvasGo.transform, "EndStateText", "",
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero);
            endStateText.fontSize = 48;
            endStateText.fontStyle = FontStyle.Bold;
            endStateText.rectTransform.sizeDelta = new Vector2(600f, 100f);
            endStateText.gameObject.SetActive(false);

            // EndStateUI nghe StageDirector — StageBootstrapper nối director + dòng gợi ý sau.
            var endGo = new GameObject("EndStateUI");
            var endUi = endGo.AddComponent<EndStateUI>();
            var endSO = new SerializedObject(endUi);
            endSO.FindProperty("endStateText").objectReferenceValue = endStateText;
            endSO.ApplyModifiedProperties();
        }

        internal static Text CreateHudText(Transform parent, string name, string defaultText,
            TextAnchor alignment, Vector2 anchor, Vector2 anchoredPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.text = defaultText;
            text.font = GetDefaultFont();
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = alignment;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(320f, 36f);

            return text;
        }

        private static Font cachedDefaultFont;

        /// <summary>
        /// Lấy font mặc định an toàn. "Arial.ttf" từng là tên builtin resource chuẩn
        /// nhưng không ổn định ở Unity bản mới — thử nhiều tên, không để throw làm
        /// hỏng cả hàm gọi nó (từng khiến LivesText/BossHPText không được tạo).
        /// </summary>
        internal static Font GetDefaultFont()
        {
            if (cachedDefaultFont != null) return cachedDefaultFont;

            string[] candidateNames = { "LegacyRuntime.ttf", "Arial.ttf" };
            foreach (var fontName in candidateNames)
            {
                try
                {
                    var font = Resources.GetBuiltinResource<Font>(fontName);
                    if (font != null)
                    {
                        cachedDefaultFont = font;
                        return font;
                    }
                }
                catch { /* thử tên tiếp theo */ }
            }

            Debug.LogWarning("[Houkimajo] Không tìm được builtin font, text HUD có thể không hiện.");
            return null;
        }

        private static T EnsureManager<T>(string name) where T : Component
        {
            var existing = Object.FindAnyObjectByType<T>();
            if (existing != null) return existing;

            var go = new GameObject(name);
            return go.AddComponent<T>();
        }

        private static void EnsureTagExists(string tag)
        {
            var tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            var tagManager = new SerializedObject(tagManagerAsset);
            var tagsProp = tagManager.FindProperty("tags");

            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
            }

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }
}
