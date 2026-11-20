using System;
using UnityEngine;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    /// <summary>Ô lưới shop tạo sẵn, chỉ đổi nội dung và highlight khi chọn.</summary>
    public class ShopGridCell : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image surface;
        [SerializeField] private Text label;
        [SerializeField] private ArcanePanelGraphic decoration;
        private int index;
        private Action<int> select;

        public void Bind(int cellIndex, Action<int> onSelect)
        {
            index = cellIndex; select = onSelect;
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }

        private void HandleClick() { select?.Invoke(index); }

        public void SetData(string value, bool occupied, bool selected, bool skill)
        {
            if (label.text != value) label.text = value;
            button.interactable = occupied;
            if (decoration != null)
            {
                decoration.SetState(occupied, selected);
                surface.color = Color.clear;
                label.color = occupied ? new Color(0.96f, 0.88f, 0.79f) : new Color(0.5f, 0.42f, 0.57f);
                return;
            }
            button.image.color = selected ? new Color(0.96f, 0.77f, 0.38f)
                : new Color(0.37f, 0.29f, 0.22f, occupied ? 1f : 0.5f);
            surface.color = !occupied ? new Color(0.055f, 0.035f, 0.085f)
                : skill ? new Color(0.17f, 0.09f, 0.25f) : new Color(0.25f, 0.065f, 0.12f);
        }
    }
}
