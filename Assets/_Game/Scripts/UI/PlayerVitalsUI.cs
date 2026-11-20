using Houkimajo.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    /// <summary>Thanh HP/MP đọc dữ liệu thật qua event, chỉ đổi chữ khi giá trị hiển thị đổi.</summary>
    public class PlayerVitalsUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerMana mana;
        [SerializeField] private Image healthBar;
        [SerializeField] private Image manaBar;
        [SerializeField] private Text healthText;
        [SerializeField] private Text manaText;
        private int lastHP = -1, lastMaxHP = -1, lastMP = -1, lastMaxMP = -1;

        private void OnEnable()
        {
            if (health != null) health.OnHPChanged += RefreshHealth;
            if (mana != null) mana.OnMPChanged += RefreshMana;
            Refresh();
        }

        private void Start() { Refresh(); }

        private void OnDisable()
        {
            if (health != null) health.OnHPChanged -= RefreshHealth;
            if (mana != null) mana.OnMPChanged -= RefreshMana;
        }

        private void Refresh()
        {
            if (health != null) RefreshHealth(health.CurrentHP, health.MaxHP);
            if (mana != null) RefreshMana(mana.CurrentMP, mana.MaxMP);
        }

        private void RefreshHealth(int current, int maximum)
        {
            healthBar.fillAmount = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0f;
            if (lastHP == current && lastMaxHP == maximum) return;
            lastHP = current; lastMaxHP = maximum;
            healthText.text = current + " / " + maximum;
        }

        private void RefreshMana(float current, float maximum)
        {
            manaBar.fillAmount = maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f;
            int display = Mathf.FloorToInt(current + 0.001f), max = Mathf.RoundToInt(maximum);
            if (lastMP == display && lastMaxMP == max) return;
            lastMP = display; lastMaxMP = max;
            manaText.text = display + " / " + max;
        }
    }
}
