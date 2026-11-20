using Houkimajo.Player;
using Houkimajo.Progression;
using Houkimajo.Systems;
using Houkimajo.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Houkimajo.EditorTools
{
    /// <summary>Dựng shop prototype bằng chữ/màu, không sinh item asset hay art.</summary>
    public static class HpPotionShopBuilder
    {
        private static readonly Color Ink = new Color(0.055f, 0.025f, 0.09f);
        private static readonly Color Gold = new Color(0.76f, 0.59f, 0.32f);

        [MenuItem("Houkimajo/Apply HP Potion Shop (Prototype)")]
        public static void ApplyToCurrentScene()
        {
            var player = Object.FindAnyObjectByType<PlayerHealth>();
            var canvasGo = GameObject.Find("HUDCanvas");
            var director = Object.FindAnyObjectByType<StageDirector>();
            if (player == null || canvasGo == null || director == null)
            {
                Debug.LogWarning("[Shop] Mở Stage01 có player/HUD/StageDirector trước.");
                return;
            }
            var inventory = player.GetComponent<HpPotionInventory>();
            if (inventory == null) inventory = Undo.AddComponent<HpPotionInventory>(player.gameObject);
            var shop = player.GetComponent<ShopController>();
            if (shop == null) shop = Undo.AddComponent<ShopController>(player.gameObject);
            var directorData = new SerializedObject(director);
            directorData.FindProperty("shop").objectReferenceValue = shop;
            directorData.ApplyModifiedProperties();
            if (canvasGo.GetComponent<GraphicRaycaster>() == null) Undo.AddComponent<GraphicRaycaster>(canvasGo);
            var events = Object.FindAnyObjectByType<EventSystem>();
            if (events == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(go, "Create gameplay UI input");
                events = go.GetComponent<EventSystem>();
            }
            var legacyInput = events.GetComponent<StandaloneInputModule>();
            if (legacyInput != null) Undo.DestroyObjectImmediate(legacyInput);
            var module = events.GetComponent<InputSystemUIInputModule>();
            if (module == null) module = Undo.AddComponent<InputSystemUIInputModule>(events.gameObject);
            module.AssignDefaultActions();
            BuildUI(canvasGo, shop, inventory, player);
            EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
            Debug.Log("[Shop] Shop Bình HP: B mở/đóng, 1 dùng nhanh; seed 10.000 vàng một lần. Ctrl+S để lưu.");
        }

        private static void BuildUI(GameObject canvas, ShopController shop, HpPotionInventory inventory, PlayerHealth health)
        {
            var oldUI = canvas.GetComponent<HpPotionShopUI>();
            if (oldUI != null) Undo.DestroyObjectImmediate(oldUI);
            foreach (string name in new[] { "ShopOverlay", "HpPotionQuickBar" })
            {
                var child = canvas.transform.Find(name);
                if (child != null) Undo.DestroyObjectImmediate(child.gameObject);
            }
            var quick = Panel("HpPotionQuickBar", canvas.transform, Vector2.zero, new Vector2(150f, 64f), Color.clear);
            quick.rectTransform.anchorMin = quick.rectTransform.anchorMax = Vector2.zero;
            var quickButton = Button("UseQuick", quick.transform, new Vector2(-40f, 0f), new Vector2(50f, 50f), "HP", 14);
            quickButton.GetComponentInChildren<Text>().color = new Color(0.96f, 0.65f, 0.73f);
            var count = Text("Count", quick.transform, new Vector2(4f, 10f), new Vector2(36f, 20f), "x0", 13);
            var cooldown = Text("Cooldown", quick.transform, new Vector2(4f, -12f), new Vector2(52f, 18f), "[1] HP", 11);
            var hint = Text("Hint", quick.transform, new Vector2(40f, 42f), new Vector2(250f, 20f), "B: cửa hàng · 1: Bình HP", 12);
            var overlay = Panel("ShopOverlay", canvas.transform, Vector2.zero, Vector2.zero, new Color(0.01f, 0.005f, 0.025f, 0.72f));
            overlay.rectTransform.anchorMin = Vector2.zero; overlay.rectTransform.anchorMax = Vector2.one;
            overlay.raycastTarget = true;
            overlay.gameObject.AddComponent<ArcaneWindowFade>();
            var rim = Panel("Window", overlay.transform, Vector2.zero, new Vector2(1140f, 640f), Color.clear);
            Decor("ArcaneCover", rim.transform, Vector2.zero, new Vector2(1140f, 640f), ArcanePanelGraphic.PanelStyle.Window);
            var crestGo = new GameObject("MoonCrest", typeof(RectTransform), typeof(ArcaneSealGraphic));
            crestGo.transform.SetParent(rim.transform, false); crestGo.layer = 5;
            var crest = crestGo.GetComponent<ArcaneSealGraphic>(); crest.color = Gold; crest.raycastTarget = false;
            crest.rectTransform.anchoredPosition = new Vector2(-515f, 277f);
            crest.rectTransform.sizeDelta = Vector2.one * 64f;
            var broomGo = new GameObject("BroomSeal", typeof(RectTransform), typeof(ArcaneSealGraphic));
            broomGo.transform.SetParent(crestGo.transform, false); broomGo.layer = 5;
            var broom = broomGo.GetComponent<ArcaneSealGraphic>(); broom.color = Gold; broom.raycastTarget = false;
            broom.rectTransform.sizeDelta = Vector2.one * 38f;
            var broomData = new SerializedObject(broom);
            broomData.FindProperty("broomOnly").boolValue = true;
            broomData.ApplyModifiedProperties();
            Decor("GoldPouch", rim.transform, new Vector2(395f, 282f), new Vector2(280f, 46f), ArcanePanelGraphic.PanelStyle.Button);
            var heading = Text("Heading", rim.transform, new Vector2(-225f, 282f), new Vector2(500f, 36f), "CỬA HÀNG PHÉP THUẬT", 24);
            heading.alignment = TextAnchor.MiddleLeft; heading.fontStyle = FontStyle.Bold;
            var subtitle = Text("Subtitle", rim.transform, new Vector2(-225f, 251f), new Vector2(500f, 20f),
                "Bình thuốc và những phép màu cho chuyến đi", 12);
            subtitle.alignment = TextAnchor.MiddleLeft; subtitle.color = new Color(0.65f, 0.53f, 0.72f);
            var gold = Text("Gold", rim.transform, new Vector2(385f, 282f), new Vector2(260f, 32f), "VÀNG  10.000", 20);
            gold.alignment = TextAnchor.MiddleRight; gold.color = new Color(1f, 0.83f, 0.43f);
            var hp = Text("HP", rim.transform, new Vector2(395f, 249f), new Vector2(280f, 24f), "HP 100 / 100", 12);
            hp.alignment = TextAnchor.MiddleRight;
            Panel("Divider", rim.transform, new Vector2(0f, 235f), new Vector2(1070f, 1f), Gold);
            var closeGo = new GameObject("Close", typeof(RectTransform), typeof(HudDiscGraphic), typeof(Button));
            closeGo.transform.SetParent(rim.transform, false); closeGo.layer = 5;
            var closeDisc = closeGo.GetComponent<HudDiscGraphic>(); closeDisc.color = Gold; closeDisc.raycastTarget = true;
            closeDisc.rectTransform.anchorMin = closeDisc.rectTransform.anchorMax = Vector2.one;
            closeDisc.rectTransform.anchoredPosition = Vector2.zero; closeDisc.rectTransform.sizeDelta = Vector2.one * 38f;
            var close = closeGo.GetComponent<Button>(); close.targetGraphic = closeDisc;
            Text("Label", closeGo.transform, Vector2.zero, new Vector2(32f, 32f), "×", 26).color = Ink;
            var items = Button("ItemsTab", rim.transform, new Vector2(-485f, 151f), new Vector2(120f, 48f), "Vật phẩm", 14);
            var equipment = Button("EquipmentTab", rim.transform, new Vector2(-485f, 88f), new Vector2(120f, 48f), "Trang bị", 14);
            var upgrades = Button("UpgradesTab", rim.transform, new Vector2(-485f, 25f), new Vector2(120f, 48f), "Nâng cấp", 14);
            StyleCategory(items, 0); StyleCategory(equipment, 1); StyleCategory(upgrades, 2);
            var storeTitle = Text("StoreTitle", rim.transform, new Vector2(-180f, 207f), new Vector2(440f, 32f), "Cửa hàng", 20);
            storeTitle.alignment = TextAnchor.MiddleLeft; storeTitle.fontStyle = FontStyle.BoldAndItalic; storeTitle.color = new Color(0.9f, 0.77f, 0.57f);
            var bagTitle = Text("BagTitle", rim.transform, new Vector2(285f, 207f), new Vector2(440f, 32f), "Túi đồ", 20);
            bagTitle.alignment = TextAnchor.MiddleLeft; bagTitle.fontStyle = FontStyle.BoldAndItalic; bagTitle.color = new Color(0.9f, 0.77f, 0.57f);
            var storeGrid = Panel("StoreGrid", rim.transform, Vector2.left * 180f, new Vector2(440f, 350f), Color.clear);
            var bagGrid = Panel("BagGrid", rim.transform, Vector2.right * 285f, new Vector2(440f, 350f), Color.clear);
            Decor("Page", storeGrid.transform, new Vector2(0f, 20f), new Vector2(454f, 402f), ArcanePanelGraphic.PanelStyle.Page);
            Decor("Page", bagGrid.transform, new Vector2(0f, 20f), new Vector2(454f, 402f), ArcanePanelGraphic.PanelStyle.Page);
            var storeCells = BuildGrid(storeGrid.transform);
            var bagCells = BuildGrid(bagGrid.transform);
            var storeEmpty = Text("Empty", storeGrid.transform, Vector2.zero, new Vector2(410f, 70f), "", 17);
            var bagEmpty = Text("Empty", bagGrid.transform, Vector2.zero, new Vector2(410f, 70f), "Túi đồ trống", 17);
            storeTitle.transform.SetAsLastSibling(); bagTitle.transform.SetAsLastSibling();
            Panel("InformationInk", rim.transform, new Vector2(40f, -202f), new Vector2(980f, 48f), new Color(0.07f, 0.04f, 0.11f, 0.8f));
            var details = Text("Details", rim.transform, new Vector2(40f, -202f), new Vector2(970f, 46f), "Chọn một ô để xem thông tin.", 14);
            details.alignment = TextAnchor.MiddleLeft;
            var buy = Button("Buy", rim.transform, new Vector2(-80f, -257f), new Vector2(220f, 44f), "MUA", 16);
            var use = Button("Use", rim.transform, new Vector2(197f, -257f), new Vector2(175f, 44f), "DÙNG 1", 15);
            var sell = Button("Sell", rim.transform, new Vector2(410f, -257f), new Vector2(175f, 44f), "BÁN", 16);
            var status = Text("Status", rim.transform, new Vector2(0f, -300f), new Vector2(1050f, 22f), "", 12);
            status.color = new Color(0.9f, 0.8f, 0.6f);
            var view = Undo.AddComponent<HpPotionShopUI>(canvas);
            var data = new SerializedObject(view);
            Set(data,"shop",shop); Set(data,"inventory",inventory); Set(data,"wallet",health.GetComponent<Wallet>());
            Set(data,"health",health); Set(data,"caster",health.GetComponent<Houkimajo.Skills.SkillCaster>());
            Set(data,"overlay",overlay.gameObject); Set(data,"window",rim.rectTransform);
            Set(data,"quickRoot",quick.rectTransform); Set(data,"goldText",gold); Set(data,"healthText",hp);
            Set(data,"detailText",details); Set(data,"statusText",status); Set(data,"bagTitle",bagTitle);
            Set(data,"storeEmpty",storeEmpty); Set(data,"bagEmpty",bagEmpty);
            Set(data,"quickCount",count); Set(data,"quickStatus",cooldown); Set(data,"hintText",hint);
            Set(data,"buyLabel",buy.GetComponentInChildren<Text>()); Set(data,"buyButton",buy); Set(data,"useButton",use);
            Set(data,"sellButton",sell); Set(data,"continueButton",close); Set(data,"quickButton",quickButton);
            Set(data,"itemsTab",items); Set(data,"equipmentTab",equipment); Set(data,"upgradesTab",upgrades);
            SetCells(data,"storeCells",storeCells); SetCells(data,"bagCells",bagCells);
            data.ApplyModifiedProperties();
            overlay.gameObject.SetActive(false);
        }

        private static ShopGridCell[] BuildGrid(Transform root)
        {
            var cells = new ShopGridCell[25];
            for (int i = 0; i < cells.Length; i++)
            {
                var button = Button("Cell_"+i, root, new Vector2(-176f + i % 5 * 88f, 140f - i / 5 * 70f),
                    new Vector2(80f,62f), "", 11);
                var surface = Panel("Surface", button.transform, Vector2.zero, new Vector2(76f,58f), Color.clear);
                var label = button.GetComponentInChildren<Text>(); label.transform.SetAsLastSibling();
                var frame = (ArcanePanelGraphic)button.targetGraphic;
                frame.Configure(ArcanePanelGraphic.PanelStyle.Slot);
                var colors = button.colors; colors.disabledColor = Color.white; button.colors = colors;
                var cell = button.gameObject.AddComponent<ShopGridCell>();
                var data = new SerializedObject(cell);
                Set(data,"button",button); Set(data,"surface",surface); Set(data,"label",label); Set(data,"decoration",frame);
                data.ApplyModifiedProperties(); cells[i]=cell;
            }
            return cells;
        }

        private static void SetCells(SerializedObject data, string name, ShopGridCell[] cells)
        {
            var property = data.FindProperty(name); property.arraySize=cells.Length;
            for(int i=0;i<cells.Length;i++) property.GetArrayElementAtIndex(i).objectReferenceValue=cells[i];
        }
        private static Image Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false); go.layer = 5;
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            image.rectTransform.anchoredPosition = position; image.rectTransform.sizeDelta = size;
            return image;
        }

        private static Text Text(string name, Transform parent, Vector2 position, Vector2 size, string value, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false); go.layer = 5;
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = fontSize; text.color = new Color(0.96f, 0.9f, 0.8f);
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchoredPosition = position; text.rectTransform.sizeDelta = size;
            return text;
        }

        private static Button Button(string name, Transform parent, Vector2 position, Vector2 size, string label, int fontSize)
        {
            var panel = Panel(name, parent, position, size, Color.clear);
            var button = panel.gameObject.AddComponent<Button>();
            var frame = Decor("Frame", panel.transform, Vector2.zero, size, ArcanePanelGraphic.PanelStyle.Button);
            frame.raycastTarget = true; button.targetGraphic = frame;
            var colors = button.colors; colors.disabledColor = new Color(0.46f, 0.39f, 0.53f, 0.65f);
            colors.highlightedColor = new Color(1.15f, 1.04f, 1.16f); colors.pressedColor = new Color(0.7f, 0.57f, 0.83f);
            colors.selectedColor = new Color(1.08f, 0.98f, 1.12f); colors.fadeDuration = 0.09f; button.colors = colors;
            var labelText = Text("Label", panel.transform, Vector2.zero, size - new Vector2(12f, 6f), label, fontSize);
            labelText.fontStyle = FontStyle.Bold;
            return button;
        }

        private static ArcanePanelGraphic Decor(string name, Transform parent, Vector2 position, Vector2 size, ArcanePanelGraphic.PanelStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(ArcanePanelGraphic));
            go.transform.SetParent(parent, false); go.layer = 5;
            var graphic = go.GetComponent<ArcanePanelGraphic>();
            graphic.Configure(style); graphic.raycastTarget = false;
            graphic.rectTransform.anchorMin = graphic.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            graphic.rectTransform.anchoredPosition = position; graphic.rectTransform.sizeDelta = size;
            return graphic;
        }

        private static void StyleCategory(Button button, int variant)
        {
            ((ArcanePanelGraphic)button.targetGraphic).Configure(ArcanePanelGraphic.PanelStyle.Category, variant);
            var label = button.GetComponentInChildren<Text>(); label.rectTransform.anchoredPosition = Vector2.right * 9f;
            label.rectTransform.sizeDelta = new Vector2(92f, 42f); label.fontStyle = FontStyle.Normal;
        }
        private static void Set(SerializedObject data, string name, Object value)
        {
            data.FindProperty(name).objectReferenceValue = value;
        }
    }
}
