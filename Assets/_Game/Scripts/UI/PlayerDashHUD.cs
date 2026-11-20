using Houkimajo.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    /// <summary>Thông tin lướt mặc định, tách khỏi bốn ô bắn thường/Q/E/R.</summary>
    public class PlayerDashHUD : MonoBehaviour
    {
        [SerializeField] private PlayerDash dash;
        [SerializeField] private Text dashText;
        private void OnEnable() { if (dash != null) dash.OnChanged += Refresh; }
        private void OnDisable() { if (dash != null) dash.OnChanged -= Refresh; }
        private void Start() { Refresh(); }
        private void Refresh()
        {
            if (dash == null || dashText == null) return;
            string value = dash.Charges > 0 ? "LShift · Lướt" + (dash.MaxCharges > 1 ? " " + dash.Charges + "/" + dash.MaxCharges : "")
                : "LShift · " + dash.CooldownRemaining.ToString("0.0") + "s";
            if (dashText.text != value) dashText.text = value;
        }
    }
}
