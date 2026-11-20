using System.IO;
using Houkimajo.Player;
using Houkimajo.Skills;
using Houkimajo.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Houkimajo.EditorTools
{
    /// <summary>Dựng HUD đáy theo mẫu avatar/pet/skill/HP/MP, hỗ trợ cập nhật scene cũ.</summary>
    public static class SkillHudBuilder
    {
        private const float DockWidth = 1280f;
        private const float StripHeight = 76f;

        [MenuItem("Houkimajo/Apply Bottom HUD Layout")]
        public static void ApplyToCurrentScene()
        {
            var caster = Object.FindAnyObjectByType<SkillCaster>();
            var canvasGo = GameObject.Find("HUDCanvas");
            if (caster == null || canvasGo == null) return;
            Build(caster, canvasGo.GetComponent<Canvas>());
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(caster.gameObject.scene);
            Debug.Log("[HUD] Đã gắn bố cục avatar/pet/skill/HP/MP. Ctrl+S để lưu.");
        }

        public static void Build(SkillCaster caster, Canvas canvas)
        {
            var shooting = caster.GetComponent<PlayerShooting>();
            EnsureBasicAttack(shooting);
            var mana = caster.GetComponent<PlayerMana>();
            if (mana == null) mana = Undo.AddComponent<PlayerMana>(caster.gameObject);
            string[] legacy = { "SkillDock", "SkillTextQ", "SkillTextE", "SkillTextR", "SkillIcon0", "SkillIcon1", "SkillIcon2" };
            foreach (string name in legacy)
            {
                var child = canvas.transform.Find(name);
                if (child != null) Undo.DestroyObjectImmediate(child.gameObject);
            }
            var oldLives = canvas.transform.Find("LivesText");
            if (oldLives != null) oldLives.gameObject.SetActive(false);
            var dock = Panel("SkillDock", canvas.transform, Vector2.zero,
                new Vector2(DockWidth, 132f), Color.clear);
            dock.rectTransform.anchorMin = dock.rectTransform.anchorMax = Vector2.zero;
            dock.rectTransform.pivot = Vector2.zero;
            dock.rectTransform.anchoredPosition = Vector2.zero;
            var group = dock.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            var white = EnsureWhiteSprite();
            var gold = new Color(0.74f, 0.57f, 0.31f);
            var ink = new Color(0.035f, 0.02f, 0.065f, 0.97f);
            var strip = Panel("BottomStrip", dock.transform, Vector2.zero,
                new Vector2(0f, StripHeight), ink);
            strip.rectTransform.anchorMin = Vector2.zero;
            strip.rectTransform.anchorMax = new Vector2(1f, 0f);
            strip.rectTransform.pivot = new Vector2(0.5f, 0f);
            var line = Panel("GoldEdge", strip.transform, new Vector2(0f, StripHeight), new Vector2(0f, 1.5f), gold);
            line.rectTransform.anchorMin = Vector2.zero;
            line.rectTransform.anchorMax = new Vector2(1f, 0f);

            var portrait = Disc("AvatarRim", dock.transform, new Vector2(72f, StripHeight), 176f, gold);
            portrait.rectTransform.localScale = Vector3.one * 0.6f;
            portrait.rectTransform.anchorMin = portrait.rectTransform.anchorMax = Vector2.zero;
            Disc("AvatarInk", portrait.transform, Vector2.zero, 170f, ink);
            var clip = Disc("AvatarClip", portrait.transform, Vector2.zero, 158f, Color.white);
            var mask = clip.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            var backdrop = Panel("PortraitBackdrop", clip.transform, Vector2.zero, new Vector2(158f, 158f),
                new Color(0.14f, 0.07f, 0.22f));
            var image = Panel("Portrait", clip.transform, new Vector2(0f, -10f), new Vector2(320f, 214f), Color.white);
            image.sprite = FirstSprite("Assets/_Game/Art/Sprites/Player/player_witch_gameplay_v1.png");
            image.preserveAspect = true;
            var pet = Disc("PetRim", dock.transform, new Vector2(117f, 22f), 54f, gold);
            pet.rectTransform.localScale = Vector3.one * 0.75f;
            pet.rectTransform.anchorMin = pet.rectTransform.anchorMax = Vector2.zero;
            Disc("PetEmpty", pet.transform, Vector2.zero, 50f, new Color(0.12f, 0.095f, 0.16f));
            Label("PetLabel", pet.transform, Vector2.zero, new Vector2(44f, 18f), "PET", 10, new Color(0.5f, 0.45f, 0.56f));

            var center = Panel("SkillGroup", dock.transform, new Vector2(0f, StripHeight * 0.5f), new Vector2(328f, 80f), Color.clear);
            center.rectTransform.localScale = Vector3.one * 0.78f;
            center.rectTransform.anchorMin = center.rectTransform.anchorMax = new Vector2(0.48f, 0f);
            var slots = new SkillHudSlot[4];
            string[] keys = { "LMB", "Q", "E", "R" };
            for (int i = 0; i < slots.Length; i++)
                slots[i] = BuildSlot(center.transform, caster, shooting, i, keys[i], white, gold, ink);
            BuildVitals(dock.transform, caster.GetComponent<PlayerHealth>(), mana, white, gold);
            var dashHint = Label("DefaultDash", dock.transform, new Vector2(395f, 47f), new Vector2(160f, 18f), "LShift · Lướt", 10, gold);
            dashHint.rectTransform.anchorMin = dashHint.rectTransform.anchorMax = Vector2.zero;
            var focusHint = Label("FocusHint", dock.transform, new Vector2(395f, 27f), new Vector2(160f, 18f), "LCtrl · Bay chậm", 10, new Color(0.65f, 0.54f, 0.75f));
            focusHint.rectTransform.anchorMin = focusHint.rectTransform.anchorMax = Vector2.zero;
            var dashView = dashHint.gameObject.AddComponent<PlayerDashHUD>();
            var dashViewData = new SerializedObject(dashView);
            Set(dashViewData, "dash", caster.GetComponent<PlayerDash>()); Set(dashViewData, "dashText", dashHint);
            dashViewData.ApplyModifiedProperties();

            var barGo = GameObject.Find("SkillBarUI");
            if (barGo == null) barGo = new GameObject("SkillBarUI");
            var bar = barGo.GetComponent<SkillBarUI>();
            if (bar == null) bar = barGo.AddComponent<SkillBarUI>();
            var barSO = new SerializedObject(bar);
            Set(barSO, "caster", caster); Set(barSO, "shooting", shooting); Set(barSO, "dockRoot", dock.rectTransform);
            Set(barSO, "gameplayCamera", Camera.main);
            barSO.FindProperty("dockWidth").floatValue = DockWidth;
            barSO.FindProperty("stripHeight").floatValue = StripHeight;
            barSO.FindProperty("edgeMargin").floatValue = 0f;
            var prop = barSO.FindProperty("slots"); prop.arraySize = slots.Length;
            for (int i = 0; i < slots.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            barSO.ApplyModifiedPropertiesWithoutUndo();
            bar.FitToSafeArea(new Rect(0f, 0f, Screen.width, Screen.height), Mathf.Max(0.01f, canvas.scaleFactor));
            Undo.RegisterCreatedObjectUndo(dock.gameObject, "Build bottom HUD");
        }

        private static SkillHudSlot BuildSlot(Transform parent, SkillCaster caster, PlayerShooting shooting,
            int index, string keyName, Sprite white, Color gold, Color ink)
        {
            var frame = Panel("Slot_" + keyName, parent, new Vector2(-123f + index * 82f, 0f), new Vector2(76f, 76f), gold);
            var inner = Panel("Surface", frame.transform, Vector2.zero, new Vector2(72f, 72f), ink);
            var icon = Panel("Icon", inner.transform, Vector2.zero, new Vector2(62f, 62f), Color.white);
            icon.preserveAspect = true;
            var fallback = Label("Symbol", inner.transform, Vector2.zero, new Vector2(60f, 60f), "", 24, gold);
            var overlay = Panel("CooldownMask", inner.transform, Vector2.zero, new Vector2(72f, 72f), new Color(0.02f, 0.01f, 0.04f, 0.8f));
            overlay.sprite = white; overlay.type = Image.Type.Filled;
            overlay.fillMethod = Image.FillMethod.Radial360;
            overlay.fillOrigin = (int)Image.Origin360.Top;
            var cooldown = Label("Cooldown", inner.transform, Vector2.zero, new Vector2(64f, 30f), "", 20, Color.white);
            var keyBacking = Panel("KeyBacking", inner.transform, new Vector2(-22f, -26f), new Vector2(30f, 16f), ink);
            var key = Label("Key", keyBacking.transform, Vector2.zero, new Vector2(30f, 16f), keyName, keyName == "SPACE" ? 8 : 12, Color.white);
            key.fontStyle = FontStyle.Bold;
            var level = Label("Level", inner.transform, new Vector2(25f, 25f), new Vector2(18f, 16f), "I", 11, gold);
            var charges = Label("Charges", inner.transform, new Vector2(23f, -26f), new Vector2(30f, 16f), "", 10, gold);
            var recharge = Panel("Recharge", inner.transform, new Vector2(0f, -35f), new Vector2(70f, 2f), gold);
            recharge.sprite = white; recharge.type = Image.Type.Filled; recharge.fillMethod = Image.FillMethod.Horizontal;
            var title = Label("Name", inner.transform, Vector2.zero, Vector2.one, "", 10, gold);
            var attribute = Label("Attribute", inner.transform, Vector2.zero, Vector2.one, "", 10, gold);
            title.gameObject.SetActive(false); attribute.gameObject.SetActive(false);
            var view = frame.gameObject.AddComponent<SkillHudSlot>();
            var data = new SerializedObject(view);
            Set(data,"border",frame); Set(data,"icon",icon); Set(data,"cooldownMask",overlay);
            Set(data,"cooldownShape",white); Set(data,"rechargeBar",recharge); Set(data,"nameText",title);
            Set(data,"levelText",level); Set(data,"attributeText",attribute); Set(data,"cooldownText",cooldown);
            Set(data,"chargeText",charges); Set(data,"fallbackText",fallback);
            data.ApplyModifiedPropertiesWithoutUndo();
            var skill = index > 0 ? caster.GetSkill((SkillSlot)(index - 1)) : null;
            var attack = shooting != null ? shooting.Attack : null;
            bool available = index == 0 ? shooting != null : skill != null;
            view.SetIdentity(index == 0 ? attack != null ? attack.displayName : "Bắn thường" : skill != null ? skill.displayName : "Chưa trang bị",
                index == 0 ? attack != null ? attack.icon : null : skill != null ? skill.icon : null,
                available ? index == 0 ? shooting.AttackLevel : caster.GetLevel((SkillSlot)(index-1)) : 0,
                index == 0 ? attack != null ? attack.attributeName : "Phép" : skill != null ? skill.attributeName : "TRỐNG",
                index == 0 ? attack != null ? attack.attributeColor : gold : skill != null ? skill.color : gold, "—");
            view.SetState(index == 3 ? 100f : 0f, 100f, index == 3 ? 0 : 1, 1, available, index == 0, 0f, false, index == 3);
            return view;
        }

        private static void BuildVitals(Transform parent, PlayerHealth health, PlayerMana mana, Sprite white, Color gold)
        {
            var root = Panel("Vitals", parent, new Vector2(-20f, StripHeight * 0.5f), new Vector2(366f, 92f), Color.clear);
            root.rectTransform.localScale = Vector3.one * 0.8f;
            root.rectTransform.anchorMin = root.rectTransform.anchorMax = new Vector2(1f, 0f);
            root.rectTransform.pivot = new Vector2(1f, 0.5f);
            var hp = VitalRow(root.transform, "HP", 9f, new Color(0.82f, 0.13f, 0.22f), white, gold, out var hpText);
            var mp = VitalRow(root.transform, "MP", -31f, new Color(0.25f, 0.30f, 0.83f), white, gold, out var mpText);
            hpText.text = health != null ? health.MaxHP + " / " + health.MaxHP : "—";
            mpText.text = mana != null ? mana.MaxMP + " / " + mana.MaxMP : "—";
            var view = root.gameObject.AddComponent<PlayerVitalsUI>();
            var data = new SerializedObject(view);
            Set(data,"health",health); Set(data,"mana",mana); Set(data,"healthBar",hp); Set(data,"manaBar",mp);
            Set(data,"healthText",hpText); Set(data,"manaText",mpText);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Image VitalRow(Transform parent, string name, float y, Color tint, Sprite white, Color gold, out Text value)
        {
            var label = Label(name+"Label", parent, new Vector2(-164f,y), new Vector2(38f,22f), name, 14, new Color(0.95f,0.89f,0.77f));
            label.fontStyle = FontStyle.Bold;
            var rim = Panel(name+"Rim", parent, new Vector2(16f,y), new Vector2(330f,18f), gold);
            Panel(name+"Track", rim.transform, Vector2.zero, new Vector2(328f,16f), new Color(0.12f,0.06f,0.12f));
            var bar = Panel(name+"Fill", rim.transform, Vector2.zero, new Vector2(328f,16f), tint);
            bar.sprite=white; bar.type=Image.Type.Filled; bar.fillMethod=Image.FillMethod.Horizontal;
            bar.fillAmount=1f;
            value=Label(name+"Value", parent, new Vector2(16f,y+18f), new Vector2(328f,18f), "", 13, Color.white);
            value.alignment=TextAnchor.MiddleLeft;
            return bar;
        }

        private static HudDiscGraphic Disc(string name, Transform parent, Vector2 position, float diameter, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HudDiscGraphic));
            go.transform.SetParent(parent,false);
            var graphic=go.GetComponent<HudDiscGraphic>();
            graphic.color=color; graphic.raycastTarget=false;
            graphic.rectTransform.anchorMin=graphic.rectTransform.anchorMax=new Vector2(0.5f,0.5f);
            graphic.rectTransform.anchoredPosition=position;
            graphic.rectTransform.sizeDelta=Vector2.one*diameter;
            return graphic;
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
