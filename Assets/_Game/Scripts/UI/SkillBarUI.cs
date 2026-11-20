using Houkimajo.Player;
using Houkimajo.Skills;
using UnityEngine;

namespace Houkimajo.UI
{
    /// <summary>Dock LMB/Q/E/R đọc cooldown thật, metadata cập nhật qua event.</summary>
    public class SkillBarUI : MonoBehaviour
    {
        [SerializeField] private SkillCaster caster;
        [SerializeField] private PlayerShooting shooting;
        [SerializeField] private SkillHudSlot[] slots = new SkillHudSlot[4];
        [SerializeField] private RectTransform dockRoot;
        [SerializeField] private float dockWidth = 1280f;
        [SerializeField] private float edgeMargin = 0f;
        [SerializeField, Min(1f), Tooltip("chiều cao dải HUD đặc ở scale Thiết kế, dùng để chừa viewport")]
        private float stripHeight = 76f;
        [SerializeField] private Camera gameplayCamera;
        private Rect originalViewport;
        private readonly float[] castUntil = new float[4];
        private readonly float[] failUntil = new float[4];
        private bool identityDirty = true;
        private Canvas canvas;
        private Rect lastSafeArea;
        private int lastWidth;
        private int lastHeight;

        private void Awake()
        {
            if (dockRoot != null) canvas = dockRoot.GetComponentInParent<Canvas>();
            if (gameplayCamera != null) originalViewport = gameplayCamera.rect;
        }

        private void OnEnable()
        {
            identityDirty = true;
            if (caster != null)
            {
                caster.OnSkillCast += HandleCast;
                caster.OnSkillFailed += HandleFailed;
                caster.OnLoadoutChanged += HandleLoadoutChanged;
            }
            if (shooting != null) shooting.OnAttackChanged += HandleAttackChanged;
        }

        private void OnDisable()
        {
            if (Application.isPlaying && gameplayCamera != null) gameplayCamera.rect = originalViewport;
            if (caster != null)
            {
                caster.OnSkillCast -= HandleCast;
                caster.OnSkillFailed -= HandleFailed;
                caster.OnLoadoutChanged -= HandleLoadoutChanged;
            }
            if (shooting != null) shooting.OnAttackChanged -= HandleAttackChanged;
        }

        private void Update()
        {
            FitSafeArea();
            if (identityDirty) RefreshIdentity();
            for (int i = 0; i < slots.Length && i < 4; i++)
            {
                if (slots[i] == null) continue;
                float remaining = 0f, total = 0f;
                int charges = 0, maxCharges = 0;
                bool available = false;
                if (i == 0 && shooting != null)
                {
                    available = true;
                    remaining = shooting.CooldownRemaining;
                    total = shooting.CooldownTotal;
                    maxCharges = 1;
                    charges = remaining <= 0f ? 1 : 0;
                }
                else if (i > 0 && caster != null)
                {
                    var slot = (SkillSlot)(i - 1);
                    available = caster.GetSkill(slot) != null;
                    remaining = caster.GetCooldownRemaining(slot);
                    total = caster.GetCooldownTotal(slot);
                    charges = caster.GetCharges(slot);
                    maxCharges = caster.GetMaxCharges(slot);
                    if (slot == SkillSlot.R)
                    {
                        total = 100f;
                        remaining = 100f - caster.UltimateEnergy;
                    }
                }
                slots[i].SetState(remaining, total, charges, maxCharges, available, i == 0,
                    Mathf.Clamp01((castUntil[i] - Time.unscaledTime) / 0.18f), Time.unscaledTime < failUntil[i], i == 3);
            }
        }

        private void RefreshIdentity()
        {
            identityDirty = false;
            for (int i = 0; i < slots.Length && i < 4; i++)
            {
                if (slots[i] == null) continue;
                if (i == 0)
                {
                    var attack = shooting != null ? shooting.Attack : null;
                    slots[i].SetIdentity(attack != null ? attack.displayName : "Bắn thường",
                        attack != null ? attack.icon : null, shooting != null ? shooting.AttackLevel : 0,
                        attack != null ? attack.attributeName : "Phép",
                        attack != null ? attack.attributeColor : new Color(0.7f, 0.45f, 1f), "*");
                    continue;
                }
                var skill = caster != null ? caster.GetSkill((SkillSlot)(i - 1)) : null;
                slots[i].SetIdentity(skill != null ? skill.displayName : "Chưa trang bị",
                    skill != null ? skill.icon : null, skill != null ? caster.GetLevel((SkillSlot)(i - 1)) : 0,
                    skill != null ? skill.attributeName : "TRỐNG", skill != null ? skill.color
                        : new Color(0.45f, 0.4f, 0.55f), skill != null && i == 2 ? "~" : "—");
            }
        }

        private void FitSafeArea()
        {
            if (dockRoot == null) return;
            Rect safe = Screen.safeArea;
            if (lastWidth == Screen.width && lastHeight == Screen.height && lastSafeArea == safe) return;
            lastWidth = Screen.width; lastHeight = Screen.height; lastSafeArea = safe;
            float factor = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
            FitToSafeArea(safe, factor);
        }

        /// <summary>HUD Phủ ngang vùng an toàn; cửa sổ hẹp co cả bố cục để tránh chồng ô.</summary>
        public void FitToSafeArea(Rect safe, float canvasScale)
        {
            if (dockRoot == null) return;
            float factor = Mathf.Max(0.01f, canvasScale);
            float scale = Mathf.Min(1f, Mathf.Max(1f, safe.width / factor - edgeMargin * 2f) / dockWidth);
            dockRoot.localScale = new Vector3(scale, scale, 1f);
            dockRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                Mathf.Max(1f, safe.width / factor - edgeMargin * 2f) / scale);
            dockRoot.anchoredPosition = new Vector2(safe.xMin / factor + edgeMargin,
                safe.yMin / factor + edgeMargin);
            if (Application.isPlaying && gameplayCamera != null && Screen.height > 0)
            {
                // tách dải HUD khỏi viewport để player/đạn không bay xuống dưới thanh đặc.
                float bottom = Mathf.Clamp01((safe.yMin + (stripHeight * scale + edgeMargin) * factor) / Screen.height);
                gameplayCamera.rect = new Rect(0f, bottom, 1f, Mathf.Max(0.01f, 1f - bottom));
            }
        }

        private void HandleCast(SkillSlot slot) { castUntil[(int)slot + 1] = Time.unscaledTime + 0.18f; }
        private void HandleFailed(SkillSlot slot) { failUntil[(int)slot + 1] = Time.unscaledTime + 0.25f; }
        private void HandleLoadoutChanged(SkillSlot slot) { identityDirty = true; }
        private void HandleAttackChanged() { identityDirty = true; }
    }
}
