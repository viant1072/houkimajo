using Houkimajo.Player;
using Houkimajo.Progression;
using Houkimajo.Skills;
using Houkimajo.Systems;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    /// <summary>Hai bảng shop/túi cùng hiện; cột trái chọn vật phẩm, trang bị hoặc nâng skill.</summary>
    public class HpPotionShopUI : MonoBehaviour
    {
        [SerializeField] private ShopController shop;
        [SerializeField] private HpPotionInventory inventory;
        [SerializeField] private Wallet wallet;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private SkillCaster caster;
        [SerializeField] private GameObject overlay;
        [SerializeField] private RectTransform window;
        [SerializeField] private RectTransform quickRoot;
        [SerializeField] private Text goldText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text quickCount;
        [SerializeField] private Text quickStatus;
        [SerializeField] private Text hintText;
        [SerializeField] private Text buyLabel;
        [SerializeField] private Text bagTitle;
        [SerializeField] private Text storeEmpty;
        [SerializeField] private Text bagEmpty;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button useButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quickButton;
        [SerializeField] private Button itemsTab;
        [SerializeField] private Button equipmentTab;
        [SerializeField] private Button upgradesTab;
        [SerializeField] private ShopGridCell[] storeCells;
        [SerializeField] private ShopGridCell[] bagCells;
        private int category;
        private int selectedStore;
        private int selectedBag = -1;
        private bool wasOpen;
        private Canvas canvas;
        private Text useLabel;
        private Rect lastSafe;
        private int lastWidth = -1, lastHeight = -1;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            useLabel = useButton.GetComponentInChildren<Text>();
            buyButton.onClick.AddListener(BuySelected);
            sellButton.onClick.AddListener(shop.SellPotion);
            useButton.onClick.AddListener(shop.UsePotion);
            continueButton.onClick.AddListener(shop.Close);
            quickButton.onClick.AddListener(shop.UsePotion);
            itemsTab.onClick.AddListener(ShowItems);
            equipmentTab.onClick.AddListener(ShowEquipment);
            upgradesTab.onClick.AddListener(ShowUpgrades);
            for (int i = 0; i < storeCells.Length; i++) storeCells[i].Bind(i, SelectStore);
            for (int i = 0; i < bagCells.Length; i++) bagCells[i].Bind(i, SelectBag);
        }

        private void OnEnable()
        {
            shop.OnChanged += Refresh;
            inventory.OnChanged += Refresh;
            wallet.OnGoldChanged += HandleGold;
            health.OnHPChanged += HandleHealth;
        }
        private void Start() { Refresh(); Fit(); }
        private void OnDisable()
        {
            shop.OnChanged -= Refresh;
            inventory.OnChanged -= Refresh;
            wallet.OnGoldChanged -= HandleGold;
            health.OnHPChanged -= HandleHealth;
        }

        private void ShowItems() { category = 0; selectedStore = 0; selectedBag = inventory.Count > 0 ? 0 : -1; Refresh(); }
        private void ShowEquipment() { category = 1; selectedStore = selectedBag = -1; Refresh(); }
        private void ShowUpgrades() { category = 2; selectedStore = selectedBag = 0; Refresh(); }
        private void SelectStore(int index) { selectedStore = index; if (category == 2) selectedBag = index; Refresh(); }
        private void SelectBag(int index) { selectedBag = index; if (category == 2) selectedStore = index; Refresh(); }
        private void HandleGold(int gold) { Refresh(); }
        private void HandleHealth(int current, int maximum) { Refresh(); }
        private void Update() { Fit(); }

        private void BuySelected()
        {
            if (category == 0 && selectedStore == 0) shop.BuyPotion();
            else if (category == 2 && selectedStore >= 0 && selectedStore < 3)
                shop.UpgradeSkill((SkillSlot)selectedStore);
        }

        private void Refresh()
        {
            bool open = shop.IsOpen;
            overlay.SetActive(open);
            goldText.text = "VÀNG  " + wallet.Gold.ToString("N0");
            healthText.text = "HP  " + health.CurrentHP + " / " + health.MaxHP;
            statusText.text = shop.Message;
            bagTitle.text = category == 2 ? "Kỹ năng đang lắp" : "Túi đồ";
            storeEmpty.gameObject.SetActive(category == 1);
            storeEmpty.text = "Chưa có trang bị trong cửa hàng.";
            bagEmpty.gameObject.SetActive(category == 1 || (category == 0 && inventory.Count == 0));
            bagEmpty.text = category == 1 ? "Chưa có trang bị trong túi." : "Túi đồ trống";
            if (category == 0)
            {
                if (inventory.Count <= 0) selectedBag = -1;
                else if (selectedBag < 0) selectedBag = 0;
            }
            RefreshGrids();
            buyButton.interactable = false;
            sellButton.interactable = open && category == 0 && selectedBag == 0 && inventory.Count > 0;
            useButton.gameObject.SetActive(category == 0);
            useButton.interactable = open && selectedBag == 0 && inventory.CanUse;
            int cooldown = Mathf.CeilToInt(inventory.CooldownRemaining);
            useLabel.text = inventory.Count == 0 ? "CHƯA CÓ BÌNH" : cooldown > 0 ? "CHỜ " + cooldown + "s"
                : health.CurrentHP >= health.MaxHP ? "HP ĐÃ ĐẦY" : "DÙNG 1";
            if (category == 0)
            {
                buyLabel.text = inventory.Count >= inventory.MaxStack ? "TÚI ĐÃ ĐẦY"
                    : wallet.Gold < inventory.Price ? "THIẾU VÀNG" : "MUA · " + inventory.Price;
                buyButton.interactable = open && shop.IsInitialized && selectedStore == 0
                    && inventory.Count < inventory.MaxStack && wallet.Gold >= inventory.Price;
                detailText.text = "Bình HP  ·  Hồi " + inventory.HealAmount + " HP  ·  Đang có " + inventory.Count + "/" + inventory.MaxStack
                    + "  ·  Mua " + inventory.Price + " vàng  /  Bán " + inventory.Price / 2 + " vàng  ·  Phím nhanh [1]";
            }
            else if (category == 1)
            {
                buyLabel.text = "MUA";
                detailText.text = "TRANG BỊ  ·  Danh mục đang trống. Chưa có vật phẩm trang bị để mua hoặc bán.";
            }
            else RefreshUpgrade(open);
            quickButton.interactable = !open && inventory.CanUse && GameManager.Instance != null
                && GameManager.Instance.CurrentState == GameState.Playing;
            quickCount.text = "x" + inventory.Count;
            quickStatus.text = cooldown > 0 ? cooldown + "s" : "[1] HP";
            hintText.text = open ? "B / ESC: tiếp tục trận" : "B: cửa hàng   ·   1: dùng Bình HP";
            SetTab(itemsTab, category == 0); SetTab(equipmentTab, category == 1); SetTab(upgradesTab, category == 2);
            if (open != wasOpen && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(open ? continueButton.gameObject : null);
            wasOpen = open;
        }

        private void RefreshGrids()
        {
            for (int i = 0; i < storeCells.Length; i++)
            {
                string value = ""; bool occupied = false;
                if (category == 0 && i == 0) { occupied = true; value = "HP\nBình HP\n" + inventory.Price + " V"; }
                else if (category == 2 && i < 3 && caster != null)
                {
                    var slot = (SkillSlot)i; var skill = caster.GetSkill(slot);
                    if (skill != null)
                    {
                        occupied = true;
                        value = slot + " · " + skill.displayName + "\n"
                            + (shop.GetUpgradeQuote(slot, out int cost) ? "Lv" + (caster.GetLevel(slot) + 1) + " · " + cost + " V" : "TỐI ĐA");
                    }
                }
                storeCells[i].SetData(value, occupied, selectedStore == i && occupied, category == 2);
            }
            for (int i = 0; i < bagCells.Length; i++)
            {
                string value = ""; bool occupied = false;
                if (category == 0 && i == 0 && inventory.Count > 0) { occupied = true; value = "HP\nBình HP\nx" + inventory.Count; }
                else if (category == 2 && i < 3 && caster != null)
                {
                    var slot = (SkillSlot)i; var skill = caster.GetSkill(slot);
                    if (skill != null) { occupied = true; value = slot + " · " + skill.displayName + "\nLv" + caster.GetLevel(slot) + " / " + skill.MaxLevel; }
                }
                bagCells[i].SetData(value, occupied, selectedBag == i && occupied, category == 2);
            }
        }

        private void RefreshUpgrade(bool open)
        {
            buyLabel.text = "NÂNG CẤP";
            if (caster == null || selectedStore < 0 || selectedStore >= 3)
            { detailText.text = "Chọn kỹ năng Q / E / R để xem cấp và giá nâng cấp."; return; }
            var slot = (SkillSlot)selectedStore;
            var skill = caster.GetSkill(slot);
            if (skill == null) { detailText.text = "Ô kỹ năng chưa được lắp."; return; }
            int level = caster.GetLevel(slot);
            if (!shop.GetUpgradeQuote(slot, out int cost))
            { buyLabel.text = "ĐÃ TỐI ĐA"; detailText.text = skill.displayName + " · Cấp " + level + " / " + skill.MaxLevel + " (tối đa)"; return; }
            buyLabel.text = wallet.Gold < cost ? "THIẾU VÀNG" : "NÂNG CẤP · " + cost;
            buyButton.interactable = open && shop.IsInitialized && wallet.Gold >= cost;
            var current = skill.GetLevel(level); var next = skill.GetLevel(level + 1);
            detailText.text = slot + " · " + skill.displayName + " · Lv" + level + " → " + (level + 1)
                + " · " + cost + " vàng\n" + DescribeChanges(current, next);
        }

        private static string DescribeChanges(SkillLevelData current, SkillLevelData next)
        {
            var text = new StringBuilder();
            if (current.damage > 0 || next.damage > 0) text.Append("Sát thương ").Append(current.damage).Append(" → ").Append(next.damage).Append("  ·  ");
            if (current.cooldown > 0 || next.cooldown > 0) text.Append("Hồi ").Append(current.cooldown).Append(" → ").Append(next.cooldown).Append("s  ·  ");
            if (current.count != next.count) text.Append("Số lượng ").Append(current.count).Append(" → ").Append(next.count).Append("  ·  ");
            if (current.duration > 0 || next.duration > 0) text.Append("Duy trì ").Append(current.duration).Append(" → ").Append(next.duration).Append("s  ·  ");
            if (current.range > 0 || next.range > 0) text.Append("Tầm ").Append(current.range).Append(" → ").Append(next.range).Append("  ·  ");
            if (current.radius != next.radius) text.Append("Vùng ").Append(current.radius).Append(" → ").Append(next.radius).Append("  ·  ");
            if (next.hasMasteryEffect) text.Append("Hiệu ứng đặc biệt Lv5");
            return text.ToString().TrimEnd(' ', '·');
        }

        private static void SetTab(Button tab, bool selected)
        {
            if (tab.targetGraphic is ArcanePanelGraphic frame) frame.SetState(true, selected);
            else tab.image.color = selected ? new Color(0.43f, 0.29f, 0.13f) : new Color(0.13f, 0.08f, 0.19f);
        }

        private void Fit()
        {
            Rect safe = Screen.safeArea;
            if (lastWidth == Screen.width && lastHeight == Screen.height && lastSafe == safe) return;
            lastWidth = Screen.width; lastHeight = Screen.height; lastSafe = safe;
            float factor = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
            float scale = Mathf.Min(1f, Mathf.Max(1f, safe.width / factor - 32f) / 1180f,
                Mathf.Max(1f, safe.height / factor - 32f) / 680f);
            window.localScale = Vector3.one * scale;
            window.anchoredPosition = new Vector2((safe.center.x - Screen.width * 0.5f) / factor,
                (safe.center.y - Screen.height * 0.5f) / factor);
            float hudScale = Mathf.Min(1f, safe.width / factor / 1280f);
            quickRoot.localScale = Vector3.one * hudScale;
            quickRoot.anchoredPosition = new Vector2((safe.xMin + safe.width * 0.2f) / factor,
                safe.yMin / factor + 38f * hudScale);
        }
    }
}
