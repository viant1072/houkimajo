using UnityEngine;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    /// <summary>Khung phép thuật vector: góc vát, nét vàng mảnh và nền tím chuyển sắc.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class ArcanePanelGraphic : MaskableGraphic
    {
        public enum PanelStyle { Window, Page, Slot, Button, Category }
        [SerializeField] private PanelStyle style;
        [SerializeField] private bool occupied = true;
        [SerializeField] private bool selected;
        [SerializeField] private int variant;

        public void Configure(PanelStyle value, int iconVariant = 0)
        {
            style = value; variant = iconVariant; SetVerticesDirty();
        }

        public void SetState(bool hasContent, bool isSelected)
        {
            if (occupied == hasContent && selected == isSelected) return;
            occupied = hasContent; selected = isSelected; SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect r = GetPixelAdjustedRect();
            Color gold = new Color(0.72f, 0.55f, 0.31f, color.a);
            Color soft = new Color(0.43f, 0.32f, 0.47f, color.a * 0.7f);
            float cut = style == PanelStyle.Window ? 22f : style == PanelStyle.Page ? 10f : 7f;
            Color top = new Color(0.14f, 0.075f, 0.205f, color.a * 0.99f);
            Color bottom = new Color(0.045f, 0.025f, 0.085f, color.a * 0.99f);
            if (style == PanelStyle.Page)
            { top = new Color(0.11f, 0.075f, 0.155f, color.a); bottom = new Color(0.065f, 0.045f, 0.105f, color.a); }
            if (style == PanelStyle.Slot)
            {
                top = occupied ? new Color(0.2f, 0.095f, 0.26f, color.a) : new Color(0.075f, 0.05f, 0.115f, color.a);
                bottom = new Color(0.045f, 0.03f, 0.075f, color.a);
                gold = occupied ? new Color(0.58f, 0.43f, 0.37f, color.a) : new Color(0.27f, 0.2f, 0.34f, color.a * 0.6f);
            }
            if (selected)
            { top = new Color(0.29f, 0.14f, 0.35f, color.a); gold = new Color(0.96f, 0.77f, 0.45f, color.a); }
            Fill(mesh, r, cut, top, bottom);
            Border(mesh, r, cut, style == PanelStyle.Window ? 1.5f : 1f, gold);
            if (style != PanelStyle.Slot || occupied)
                Border(mesh, Inset(r, 4f), Mathf.Max(2f, cut - 2f), 0.6f, soft);
            if (style == PanelStyle.Window)
            {
                Rect inner = Inset(r, 10f);
                Border(mesh, inner, 16f, 0.55f, new Color(0.64f, 0.47f, 0.31f, color.a * 0.4f));
                for (int side = -1; side <= 1; side += 2)
                    for (int vertical = -1; vertical <= 1; vertical += 2)
                    {
                        Vector2 corner = new Vector2(side * (r.width * 0.5f - 15f), vertical * (r.height * 0.5f - 15f));
                        Star(mesh, corner, 5f, gold);
                        Line(mesh, corner + new Vector2(-side * 12f, 0f), corner + new Vector2(-side * 55f, 0f), 0.7f, gold);
                        Diamond(mesh, corner + new Vector2(-side * 65f, 0f), 2f, gold);
                    }
                Moon(mesh, new Vector2(r.center.x, r.yMax - 5f), 10f, gold, top);
                for (int i = -2; i <= 2; i++)
                    if (i != 0) Star(mesh, new Vector2(r.center.x + i * 24f, r.yMax - 6f), i % 2 == 0 ? 2f : 3f, gold);
                Star(mesh, new Vector2(r.center.x, r.yMin + 8f), 3f, gold);
            }
            else if (style == PanelStyle.Page)
            {
                Line(mesh, new Vector2(r.xMin + 12f, r.yMin + 12f), new Vector2(r.xMin + 12f, r.yMax - 12f), 0.6f, soft);
                Line(mesh, new Vector2(r.xMin + 20f, r.yMax - 43f), new Vector2(r.xMax - 20f, r.yMax - 43f), 0.6f, soft);
                Star(mesh, new Vector2(r.xMax - 23f, r.yMax - 24f), 4f, gold);
            }
            else if (style == PanelStyle.Slot)
            {
                if (!occupied) Diamond(mesh, r.center, 2f, new Color(0.45f, 0.34f, 0.52f, color.a * 0.22f));
                if (selected)
                {
                    Border(mesh, Inset(r, 2f), 5f, 0.6f, new Color(0.72f, 0.46f, 0.83f, color.a));
                    Star(mesh, new Vector2(r.xMax - 5f, r.yMax - 5f), 3.2f, gold);
                }
            }
            else if (style == PanelStyle.Category)
            {
                Vector2 point = new Vector2(r.xMin + 16f, r.center.y);
                if (variant == 0) Moon(mesh, point, 5f, gold, top);
                else if (variant == 1) Diamond(mesh, point, 5f, gold);
                else Star(mesh, point, 6f, gold);
                if (selected) Line(mesh, new Vector2(r.xMin + 5f, r.yMin + 10f), new Vector2(r.xMin + 5f, r.yMax - 10f), 2f, gold);
            }
            else
            {
                Diamond(mesh, new Vector2(r.xMin + 13f, r.center.y), 2f, gold);
                Diamond(mesh, new Vector2(r.xMax - 13f, r.center.y), 2f, gold);
            }
        }

        private static Rect Inset(Rect r, float amount) => new Rect(r.x + amount, r.y + amount, Mathf.Max(0f, r.width - amount * 2f), Mathf.Max(0f, r.height - amount * 2f));
        private static Vector2 Point(Rect r, float cut, int index)
        {
            switch (index % 8)
            {
                case 0: return new Vector2(r.xMin + cut, r.yMin);
                case 1: return new Vector2(r.xMax - cut, r.yMin);
                case 2: return new Vector2(r.xMax, r.yMin + cut);
                case 3: return new Vector2(r.xMax, r.yMax - cut);
                case 4: return new Vector2(r.xMax - cut, r.yMax);
                case 5: return new Vector2(r.xMin + cut, r.yMax);
                case 6: return new Vector2(r.xMin, r.yMax - cut);
                default: return new Vector2(r.xMin, r.yMin + cut);
            }
        }
        private static void Fill(VertexHelper mesh, Rect r, float cut, Color top, Color bottom)
        {
            int start = mesh.currentVertCount;
            mesh.AddVert(r.center, Color.Lerp(bottom, top, 0.5f), Vector2.zero);
            for (int i = 0; i < 8; i++)
            {
                Vector2 p = Point(r, cut, i);
                mesh.AddVert(p, Color.Lerp(bottom, top, Mathf.InverseLerp(r.yMin, r.yMax, p.y)), Vector2.zero);
            }
            for (int i = 0; i < 8; i++) mesh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % 8);
        }
        private static void Border(VertexHelper mesh, Rect r, float cut, float width, Color tint)
        { for (int i = 0; i < 8; i++) Line(mesh, Point(r, cut, i), Point(r, cut, i + 1), width, tint); }
        private static void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tint)
        {
            Vector2 delta = b - a; if (delta.sqrMagnitude < 0.001f) return;
            Vector2 n = new Vector2(-delta.y, delta.x).normalized * width * 0.5f;
            int start = mesh.currentVertCount;
            mesh.AddVert(a + n, tint, Vector2.zero); mesh.AddVert(b + n, tint, Vector2.zero);
            mesh.AddVert(b - n, tint, Vector2.zero); mesh.AddVert(a - n, tint, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
        }
        private static void Diamond(VertexHelper mesh, Vector2 center, float radius, Color tint)
        {
            int start = mesh.currentVertCount;
            mesh.AddVert(center + Vector2.up * radius, tint, Vector2.zero); mesh.AddVert(center + Vector2.right * radius * 0.55f, tint, Vector2.zero);
            mesh.AddVert(center + Vector2.down * radius, tint, Vector2.zero); mesh.AddVert(center + Vector2.left * radius * 0.55f, tint, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
        }
        private static void Star(VertexHelper mesh, Vector2 center, float radius, Color tint)
        { Diamond(mesh, center, radius, tint); Line(mesh, center - Vector2.right * radius * 0.7f, center + Vector2.right * radius * 0.7f, 0.8f, tint); }
        private static void Moon(VertexHelper mesh, Vector2 center, float radius, Color tint, Color ink)
        { Disk(mesh, center, radius, tint); Disk(mesh, center + new Vector2(radius * 0.4f, radius * 0.2f), radius * 0.8f, ink); }
        private static void Disk(VertexHelper mesh, Vector2 center, float radius, Color tint)
        {
            int start = mesh.currentVertCount; mesh.AddVert(center, tint, Vector2.zero);
            for (int i = 0; i <= 32; i++)
            {
                float angle = i * Mathf.PI * 2f / 32f;
                mesh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, tint, Vector2.zero);
                if (i > 0) mesh.AddTriangle(start, start + i, start + i + 1);
            }
        }
    }
}
