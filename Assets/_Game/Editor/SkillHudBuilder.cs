using System.IO;
using Houkimajo.Player;
using Houkimajo.Skills;
using Houkimajo.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Houkimajo.EditorTools
{
    /// <summary>Dựng bốn huy hiệu phép ở góc trái dưới, chạy lại được để cập nhật scene cũ.</summary>
    public static class SkillHudBuilder
    {
        private const float DockWidth = 420f;

        public static void Build(SkillCaster caster, Canvas canvas)
        {
            var shooting = caster.GetComponent<PlayerShooting>();
            EnsureBasicAttack(shooting);
            string[] legacy = { "SkillDock", "SkillTextQ", "SkillTextE", "SkillTextR", "SkillIcon0", "SkillIcon1", "SkillIcon2" };
            foreach (string name in legacy)
            {
                var child = canvas.transform.Find(name);
                if (child != null) Undo.DestroyObjectImmediate(child.gameObject);
            }
            var dock = Panel("SkillDock", canvas.transform, Vector2.zero,
                new Vector2(DockWidth, 114f), Color.clear);
            dock.rectTransform.anchorMin = dock.rectTransform.anchorMax = Vector2.zero;
            dock.rectTransform.pivot = Vector2.zero;
            dock.rectTransform.anchoredPosition = new Vector2(12f, 12f);
            var group = dock.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            var slots = new SkillHudSlot[4];
            string[] keys = { "SPACE", "Q", "E", "R" };
            var shape = FirstSprite("Assets/_Game/Art/Sprites/_Placeholder/hitbox.png");
            var white = EnsureWhiteSprite();
            Color gold = new Color(0.85f, 0.68f, 0.36f);
            for (int i = 0; i < 3; i++)
            {
                Panel("Chain" + i, dock.transform, new Vector2(-104f + i * 104f, 15f),
                    new Vector2(44f, 1f), new Color(0.58f, 0.42f, 0.23f, 0.55f));
                var knot = Panel("ChainGem" + i, dock.transform, new Vector2(-104f + i * 104f, 15f),
                    new Vector2(3f, 3f), gold);
                knot.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
            for (int i = 0; i < slots.Length; i++)
            {
                var frame = Panel("Slot_" + keys[i], dock.transform, new Vector2(-156f + 104f * i, 1f),
                    new Vector2(94f, 104f), Color.clear);
                var inner = Panel("Surface", frame.transform, Vector2.zero, new Vector2(94f, 104f), Color.clear);
                var sealGo = new GameObject("Seal", typeof(RectTransform), typeof(ArcaneSealGraphic));
                sealGo.transform.SetParent(inner.transform, false);
                var seal = sealGo.GetComponent<ArcaneSealGraphic>();
                seal.rectTransform.anchorMin = seal.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                seal.rectTransform.anchoredPosition = new Vector2(0f, 14f);
                seal.rectTransform.sizeDelta = new Vector2(70f, 70f);
                seal.color = gold;
                seal.raycastTarget = false;
                var levelRim = Panel("LevelRim", inner.transform, new Vector2(27f, 39f), new Vector2(18f, 18f), gold);
                levelRim.sprite = shape;
                var levelDisk = Panel("LevelDisk", inner.transform, new Vector2(27f, 39f), new Vector2(16f, 16f),
                    new Color(0.09f, 0.045f, 0.105f));
                levelDisk.sprite = shape;
                var level = Label("Level", inner.transform, new Vector2(27f, 39f), new Vector2(18f, 16f), "I", 9, gold);
                var icon = Panel("Icon", inner.transform, new Vector2(0f, 15f), new Vector2(42f, 42f), Color.white);
                icon.preserveAspect = true;
                var fallback = Label("Symbol", inner.transform, new Vector2(0f, 15f), new Vector2(42f, 42f), "", 24, gold);
                var broomGo = new GameObject("BroomSigil", typeof(RectTransform), typeof(ArcaneSealGraphic));
                broomGo.transform.SetParent(inner.transform, false);
                var broom = broomGo.GetComponent<ArcaneSealGraphic>();
                broom.rectTransform.anchorMin = broom.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                broom.rectTransform.anchoredPosition = new Vector2(0f, 15f);
                broom.rectTransform.sizeDelta = new Vector2(42f, 42f);
                broom.color = new Color(0.92f, 0.76f, 0.43f);
                broom.raycastTarget = false;
                var broomSO = new SerializedObject(broom);
                broomSO.FindProperty("broomOnly").boolValue = true;
                broomSO.ApplyModifiedPropertiesWithoutUndo();
                var mask = Panel("CooldownMask", inner.transform, new Vector2(0f, 15f), new Vector2(42f, 42f),
                    new Color(0.03f, 0.01f, 0.04f, 0.82f));
                mask.type = Image.Type.Filled;
                mask.fillMethod = Image.FillMethod.Radial360;
                mask.fillOrigin = (int)Image.Origin360.Top;
                mask.fillClockwise = true;
                mask.preserveAspect = true;
                var counter = Label("Cooldown", inner.transform, new Vector2(0f, 15f), new Vector2(54f, 30f), "", 20,
                    new Color(1f, 0.92f, 0.75f));
                counter.fontStyle = FontStyle.Bold;
                var track = Panel("RechargeTrack", inner.transform, new Vector2(0f, -9f), new Vector2(34f, 2f),
                    new Color(0.24f, 0.15f, 0.12f));
                track.enabled = i == 0;
                var recharge = Panel("Recharge", inner.transform, new Vector2(0f, -9f), new Vector2(34f, 2f), gold);
                recharge.type = Image.Type.Filled;
                recharge.fillMethod = Image.FillMethod.Horizontal;
                recharge.sprite = white;
                var key = Label("Key", inner.transform, new Vector2(0f, -21f), new Vector2(56f, 12f), keys[i], 9,
                    new Color(0.94f, 0.81f, 0.55f));
                key.fontStyle = FontStyle.Italic;
                var title = Label("Name", inner.transform, new Vector2(0f, -36f), new Vector2(98f, 20f), "", 10,
                    new Color(0.93f, 0.85f, 0.74f));
                title.horizontalOverflow = HorizontalWrapMode.Wrap;
                var attribute = Label("Attribute", inner.transform, new Vector2(0f, -51f), new Vector2(94f, 12f), "", 9, gold);
                var charges = Label("Charges", inner.transform, new Vector2(24f, -2f), new Vector2(30f, 16f), "", 10, gold);
                var view = frame.gameObject.AddComponent<SkillHudSlot>();
                var so = new SerializedObject(view);
                Set(so, "border", seal); Set(so, "icon", icon); Set(so, "cooldownMask", mask);
                Set(so, "cooldownShape", shape != null ? shape : white); Set(so, "rechargeBar", recharge); Set(so, "nameText", title);
                Set(so, "levelText", level); Set(so, "attributeText", attribute); Set(so, "cooldownText", counter);
                Set(so, "chargeText", charges); Set(so, "fallbackText", fallback); Set(so, "fallbackSigil", broom);
                so.ApplyModifiedPropertiesWithoutUndo();
                var skill = i > 0 ? caster.GetSkill((SkillSlot)(i - 1)) : null;
                var attack = shooting != null ? shooting.Attack : null;
                bool available = i == 0 ? shooting != null : skill != null;
                view.SetIdentity(i == 0 ? attack != null ? attack.displayName : "Bắn thường"
                        : skill != null ? skill.displayName : "Chưa trang bị",
                    i == 0 ? attack != null ? attack.icon : null : skill != null ? skill.icon : null,
                    available ? i == 0 ? shooting.AttackLevel : caster.GetLevel((SkillSlot)(i - 1)) : 0,
                    i == 0 ? attack != null ? attack.attributeName : "Phép" : skill != null ? skill.attributeName : "TRỐNG",
                    i == 0 ? attack != null ? attack.attributeColor : Color.white : skill != null ? skill.color : new Color(0.45f, 0.4f, 0.55f),
                    available ? i == 2 ? "~" : "*" : "—");
                view.SetState(0f, 1f, available ? 1 : 0, 1, available, i == 0, 0f, false);
                slots[i] = view;
            }
            var barGo = GameObject.Find("SkillBarUI");
            if (barGo == null) barGo = new GameObject("SkillBarUI");
            var bar = barGo.GetComponent<SkillBarUI>();
            if (bar == null) bar = barGo.AddComponent<SkillBarUI>();
            var barSO = new SerializedObject(bar);
            Set(barSO, "caster", caster); Set(barSO, "shooting", shooting); Set(barSO, "dockRoot", dock.rectTransform);
            barSO.FindProperty("dockWidth").floatValue = DockWidth;
            barSO.FindProperty("edgeMargin").floatValue = 12f;
            var prop = barSO.FindProperty("slots"); prop.arraySize = slots.Length;
            for (int i = 0; i < slots.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            barSO.ApplyModifiedPropertiesWithoutUndo();
            Undo.RegisterCreatedObjectUndo(dock.gameObject, "Build skill dock");
        }

        private static void EnsureBasicAttack(PlayerShooting shooting)
        {
            if (shooting == null || shooting.Attack != null) return;
            const string path = "Assets/_Game/Data/Skills/BasicAttack_Arcane.asset";
            var attack = AssetDatabase.LoadAssetAtPath<BasicAttackSO>(path);
            if (attack == null)
            {
                attack = ScriptableObject.CreateInstance<BasicAttackSO>();
                attack.icon = attack.projectileSprite = FirstSprite("Assets/_Game/Art/Sprites/Bullets/bullet_player_arcane_v1.png");
                attack.levels = new[] { new BasicAttackLevelData { damage = shooting.CurrentDamage,
                    fireInterval = shooting.CooldownTotal, bulletSpeed = shooting.CurrentBulletSpeed } };
                AssetDatabase.CreateAsset(attack, path);
            }
            var so = new SerializedObject(shooting);
            Set(so, "basicAttack", attack);
            so.FindProperty("attackLevel").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite FirstSprite(string path)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path)) if (asset is Sprite sprite) return sprite;
            return null;
        }

        private static Sprite EnsureWhiteSprite()
        {
            const string path = "Assets/_Game/Art/Sprites/UI/hud_fill_white.png";
            var existing = FirstSprite(path);
            if (existing != null) return existing;
            // Texture kỹ thuật 2×2 để UGUI Image.Filled cắt được thanh hồi, không phải art.
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 2f;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return FirstSprite(path);
        }

        private static Image Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            image.rectTransform.anchoredPosition = position; image.rectTransform.sizeDelta = size;
            image.color = color; image.raycastTarget = false;
            return image;
        }

        private static Text Label(string name, Transform parent, Vector2 position, Vector2 size,
            string value, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize; text.color = color; text.text = value;
            text.alignment = TextAnchor.MiddleCenter;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.015f, 0.005f, 0.025f, 0.95f);
            shadow.effectDistance = new Vector2(1f, -1f);
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchoredPosition = position; text.rectTransform.sizeDelta = size;
            return text;
        }

        private static void Set(SerializedObject so, string name, Object value)
        {
            so.FindProperty(name).objectReferenceValue = value;
        }
    }
}
