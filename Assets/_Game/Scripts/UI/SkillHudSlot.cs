using UnityEngine;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    /// <summary>Một thẻ HUD chỉ trình bày dữ liệu, không sở hữu cooldown hay xử lý input.</summary>
    public class SkillHudSlot : MonoBehaviour
    {
        [SerializeField] private Graphic border;
        [SerializeField] private Image icon;
        [SerializeField] private Image cooldownMask;
        [SerializeField] private Sprite cooldownShape;
        [SerializeField] private Image rechargeBar;
        [SerializeField] private Text nameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text attributeText;
        [SerializeField] private Text cooldownText;
        [SerializeField] private Text chargeText;
        [SerializeField] private Text fallbackText;
        [SerializeField] private Graphic fallbackSigil;
        private static readonly string[] RomanLevels = { "I", "II", "III", "IV", "V" };
        private Color accent = Color.white;
        private int lastLevel = int.MinValue;
        private int lastTenths = int.MinValue;
        private int lastCharges = int.MinValue;
        private int lastMaxCharges = int.MinValue;

        public void SetIdentity(string displayName, Sprite sprite, int level, string attribute,
            Color color, string fallback)
        {
            accent = color;
            if (nameText.text != displayName) nameText.text = displayName;
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            cooldownMask.sprite = sprite != null ? sprite : cooldownShape;
            fallbackText.text = sprite == null && fallback != "~" ? fallback : "";
            fallbackText.color = color;
            if (fallbackSigil != null) fallbackSigil.enabled = sprite == null && fallback == "~";
            if (attributeText.text != attribute) attributeText.text = attribute;
            attributeText.color = color;
            if (lastLevel != level)
            {
                lastLevel = level;
                levelText.text = level <= 0 ? "" : level <= RomanLevels.Length ? RomanLevels[level - 1] : level.ToString();
            }
        }

        public void SetState(float remaining, float total, int charges, int maxCharges,
            bool available, bool basicAttack, float castFlash, bool failed, bool energyMode = false)
        {
            float fraction = total > 0f ? Mathf.Clamp01(remaining / total) : 0f;
            bool usable = available && charges > 0;
            cooldownMask.enabled = !basicAttack && available && fraction > 0f;
            cooldownMask.fillAmount = fraction;
            rechargeBar.enabled = basicAttack && available;
            rechargeBar.fillAmount = 1f - fraction;
            rechargeBar.color = accent;
            icon.color = usable || basicAttack ? Color.white : new Color(0.6f, 0.6f, 0.67f);
            Color frame = available ? Color.Lerp(new Color(0.34f, 0.25f, 0.16f), new Color(0.85f, 0.68f, 0.36f),
                usable || basicAttack ? 0.9f : 0.4f) : new Color(0.35f, 0.29f, 0.25f);
            border.color = failed ? new Color(1f, 0.25f, 0.3f)
                : Color.Lerp(frame, new Color(1f, 0.9f, 0.75f), castFlash);
            int tenths = !available ? -2 : energyMode ? Mathf.FloorToInt(total - remaining)
                : basicAttack || usable ? -1 : Mathf.CeilToInt(remaining * 10f);
            if (lastTenths != tenths)
            {
                lastTenths = tenths;
                cooldownText.fontSize = energyMode ? 14 : 20;
                cooldownText.text = tenths < 0 ? "" : energyMode ? tenths + "%" : (tenths / 10f).ToString("0.0");
            }
            if (lastCharges != charges || lastMaxCharges != maxCharges)
            {
                lastCharges = charges;
                lastMaxCharges = maxCharges;
                chargeText.text = maxCharges > 1 ? charges + "/" + maxCharges : "";
            }
        }
    }
}
