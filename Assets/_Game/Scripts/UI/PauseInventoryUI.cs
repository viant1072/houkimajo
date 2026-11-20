using Houkimajo.Player;
using Houkimajo.Progression;
using Houkimajo.Skills;
using Houkimajo.Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    // Đọc Esc trước shop để một lần nhấn chỉ đóng hoặc mở một lớp giao diện.
    [DefaultExecutionOrder(-100)]
    public class PauseInventoryUI : MonoBehaviour
    {
        private enum Filter { All, Equipment, Bag }
        private static readonly string[] SlotNames = { "Vũ khí", "Bùa chú", "Tinh thể", "Phụ kiện", "Mũ", "Áo", "Găng tay", "Giày" };
        private static readonly string[] FilterNames = { "Tất cả", "Trang bị", "Túi đồ" };
        private readonly ArcanePanelGraphic[] tabs = new ArcanePanelGraphic[3];
        private readonly Text[] skills = new Text[3];
        private readonly Image[] skillIcons = new Image[3];
        private Canvas overlay;
        private RectTransform menu, sheet;
        private Text stats, detail, potionLabel;
        private Image avatar;
        private Button potionButton;
        private Font font;
        private PlayerHealth player;
        private PlayerFlightAnimation flight;
        private HpPotionInventory potions;
        private Filter filter;

        public void Bind(PlayerHealth health)
        {
            player = health;
            if (health == null) return;
            flight = health.GetComponentInChildren<PlayerFlightAnimation>();
            potions = health.GetComponent<HpPotionInventory>();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || player == null) return;
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                if (gm.CurrentState == GameState.Playing)
                {
                    if (overlay == null) Build();
                    gm.SetState(GameState.Paused);
                    overlay.gameObject.SetActive(true);
                    ShowMenu();
                }
                else if (gm.CurrentState == GameState.Paused && overlay != null && overlay.gameObject.activeSelf)
                {
                    if (sheet.gameObject.activeSelf) ShowMenu();
                    else Resume();
                }
            }
            if (overlay == null || !overlay.gameObject.activeSelf) return;
            if (gm.CurrentState != GameState.Paused) overlay.gameObject.SetActive(false);
            else if (sheet.gameObject.activeSelf && flight != null)
                avatar.sprite = flight.PreviewFrame(Time.unscaledTime);
        }

        private void Resume()
        {
            overlay.gameObject.SetActive(false);
            GameManager.Instance.SetState(GameState.Playing);
        }

        private void OnDisable()
        {
            if (overlay != null && overlay.gameObject.activeSelf && GameManager.Instance?.CurrentState == GameState.Paused)
                Resume();
        }

        private void ShowMenu()
        {
            sheet.gameObject.SetActive(false);
            menu.gameObject.SetActive(true);
        }

        private void OpenSheet(Filter initial)
        {
            menu.gameObject.SetActive(false);
            sheet.gameObject.SetActive(true);
            var mana = player.GetComponent<PlayerMana>();
            var shot = player.GetComponent<PlayerShooting>();
            var wallet = player.GetComponent<Wallet>();
            stats.text = $"HP     {player.CurrentHP} / {player.MaxHP}\n\nMP     {mana.CurrentMP:0} / {mana.MaxMP:0}\n\nSát thương đạn     {shot.CurrentDamage}\n\nNhịp bắn     {shot.CooldownTotal:0.##} s\n\nVàng     {wallet.Gold}";
            var caster = player.GetComponent<SkillCaster>();
            for (int i = 0; i < skills.Length; i++)
            {
                var slot = (SkillSlot)i;
                var skill = caster.GetSkill(slot);
                skills[i].text = skill == null ? slot.ToString() : $"{slot} · Cấp {caster.GetLevel(slot)}\n{skill.displayName}";
                skillIcons[i].sprite = skill == null ? null : skill.icon;
                skillIcons[i].enabled = skillIcons[i].sprite != null;
            }
            SelectFilter(initial);
        }

        private void SelectFilter(Filter value)
        {
            filter = value;
            for (int i = 0; i < tabs.Length; i++) tabs[i].SetState(true, i == (int)value);
            bool hasPotion = value != Filter.Equipment && potions != null && potions.Count > 0;
            potionButton.interactable = hasPotion;
            potionLabel.text = hasPotion ? $"HP ×{potions.Count}" : "";
            detail.text = hasPotion ? "Chọn vật phẩm để xem chi tiết." :
                filter == Filter.Equipment ? "Chưa có trang bị." : "Túi đồ trống.";
        }

        private void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("PauseOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            overlay = root.GetComponent<Canvas>();
            overlay.renderMode = RenderMode.ScreenSpaceOverlay;
            overlay.sortingOrder = 110;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var shade = Rect(root.transform, "Shade", 0, 0, 0, 0);
            shade.anchorMin = Vector2.zero;
            shade.anchorMax = Vector2.one;
            shade.offsetMin = shade.offsetMax = Vector2.zero;
            shade.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.025f, 0.08f, 0.75f);
            menu = Window(shade, "Pause", 370, 338);
            Label(menu, "Tạm dừng", 25, 20, 320, 55, 32);
            Action(menu, "Tiếp tục", 52, 90, 266, 52, Resume);
            Action(menu, "Kho đồ", 52, 155, 266, 52, () => OpenSheet(Filter.All));
            Action(menu, "Trang bị", 52, 220, 266, 52, () => OpenSheet(Filter.Equipment));
            sheet = Window(shade, "Inventory", 1200, 610);
            Label(sheet, "Kho đồ và trang bị", 25, 14, 1150, 48, 29);
            Action(sheet, "Quay lại", 1048, 19, 122, 38, ShowMenu);

            var left = Panel(sheet, "Stats", 30, 90, 232, 455);
            Label(left, "Chỉ số nhân vật", 10, 14, 212, 44, 22);
            stats = Label(left, "", 18, 78, 196, 300, 19);
            stats.alignment = TextAnchor.UpperLeft;

            var portrait = Panel(sheet, "Avatar", 402, 100, 190, 244);
            avatar = Rect(portrait, "Witch", 7, 7, 176, 230).gameObject.AddComponent<Image>();
            avatar.preserveAspect = true;
            avatar.raycastTarget = false;
            var renderer = player.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null) avatar.sprite = renderer.sprite;
            for (int i = 0; i < SlotNames.Length; i++)
            {
                int x = i < 4 ? 284 : 606;
                var cell = Panel(sheet, SlotNames[i], x, 100 + i % 4 * 64, 104, 52);
                Label(cell, SlotNames[i] + "\n—", 4, 2, 96, 48, 16);
            }
            var pet = Panel(sheet, "Pet", 308, 368, 163, 58);
            Label(pet, "Tinh linh (Pet)\n—", 4, 2, 155, 54, 17);
            Panel(sheet, "CompanionSlot", 523, 368, 163, 58);
            for (int i = 0; i < skills.Length; i++)
            {
                var cell = Panel(sheet, "Skill" + i, 284 + i * 146, 454, 134, 102);
                skillIcons[i] = Rect(cell, "Icon", 48, 5, 38, 38).gameObject.AddComponent<Image>();
                skillIcons[i].preserveAspect = true;
                skills[i] = Label(cell, "", 3, 46, 128, 52, 15);
            }

            for (int i = 0; i < tabs.Length; i++)
            {
                Filter choice = (Filter)i;
                tabs[i] = Action(sheet, FilterNames[i], 746 + i * 140, 83, 138, 41, () => SelectFilter(choice))
                    .GetComponent<ArcanePanelGraphic>();
            }
            for (int i = 0; i < 70; i++)
            {
                var cell = Panel(sheet, "BagSlot" + i, 746 + i % 5 * 84, 132 + i / 5 * 27, 82, 26);
                if (i != 0) continue;
                potionLabel = Label(cell, "", 1, 0, 80, 26, 15);
                potionButton = cell.gameObject.AddComponent<Button>();
                potionButton.targetGraphic = cell.GetComponent<ArcanePanelGraphic>();
                potionButton.onClick.AddListener(() => detail.text = $"Bình HP   ·   Hồi {potions.HealAmount} HP   ·   Đang có {potions.Count}");
            }
            detail = Label(sheet, "", 746, 520, 418, 50, 17);
        }

        private RectTransform Window(Transform parent, string id, int width, int height)
        {
            var rect = Panel(parent, id, 0, 0, width, height);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.GetComponent<ArcanePanelGraphic>().Configure(ArcanePanelGraphic.PanelStyle.Window);
            return rect;
        }

        private Button Action(Transform parent, string title, int x, int y, int w, int h, UnityEngine.Events.UnityAction action)
        {
            var rect = Panel(parent, title, x, y, w, h);
            var button = rect.gameObject.AddComponent<Button>();
            var graphic = rect.GetComponent<ArcanePanelGraphic>();
            graphic.Configure(ArcanePanelGraphic.PanelStyle.Button);
            button.targetGraphic = graphic;
            button.onClick.AddListener(action);
            Label(rect, title, 4, 2, w - 8, h - 4, 19);
            return button;
        }

        private static RectTransform Panel(Transform parent, string id, int x, int y, int w, int h)
        {
            var rect = Rect(parent, id, x, y, w, h);
            rect.gameObject.AddComponent<ArcanePanelGraphic>().Configure(ArcanePanelGraphic.PanelStyle.Slot);
            return rect;
        }

        private Text Label(Transform parent, string value, int x, int y, int w, int h, int size)
        {
            var text = Rect(parent, "Label", x, y, w, h).gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = new Color(1f, 0.91f, 0.78f);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static RectTransform Rect(Transform parent, string id, int x, int y, int w, int h)
        {
            var rect = (RectTransform)new GameObject(id, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(w, h);
            rect.anchoredPosition = new Vector2(x, -y);
            return rect;
        }
    }
}
