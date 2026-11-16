using UnityEngine;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    /// <summary>Họa tiết huy hiệu vector của HUD: không phụ thuộc texture hay độ phân giải.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class ArcaneSealGraphic : MaskableGraphic
    {
        [SerializeField] private bool broomOnly;
        [SerializeField] private Color ink = new Color(0.055f, 0.025f, 0.075f, 0.94f);

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (broomOnly)
            {
                Line(mesh, new Vector2(-7f, -9f), new Vector2(11f, 14f), 2f, color);
                Triangle(mesh, new Vector2(-7f, -6f), new Vector2(-17f, -13f), new Vector2(-9f, -20f), color);
                for (int i = 0; i < 4; i++)
                    Line(mesh, new Vector2(-7f, -8f), new Vector2(-17f + i * 3f, -15f - i * 1.5f), 1f,
                        new Color(0.95f, 0.8f, 0.46f, color.a));
                Line(mesh, new Vector2(-11f, -7f), new Vector2(-5f, -12f), 2f, new Color(0.68f, 0.4f, 0.8f, color.a));
                return;
            }
            float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.42f;
            Disk(mesh, Vector2.zero, radius, ink);
            Ring(mesh, radius, 1.1f, color);
            Ring(mesh, radius - 5f, 0.55f, new Color(color.r, color.g, color.b, color.a * 0.48f));
            for (int side = -1; side <= 1; side += 2)
            {
                for (int leaf = 0; leaf < 3; leaf++)
                {
                    float angle = (side > 0 ? -38f + leaf * 38f : 142f + leaf * 38f) * Mathf.Deg2Rad;
                    Vector2 center = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius + 1.5f);
                    Diamond(mesh, center, 2.4f, color);
                }
            }
            // Chồng đĩa mực lệch tâm lên đĩa vàng để tạo trăng lưỡi liềm.
            Vector2 moon = Vector2.up * (radius + 0.8f);
            Disk(mesh, moon, 5.5f, color);
            Disk(mesh, moon + new Vector2(2.8f, 1.8f), 4.8f, ink);
            Diamond(mesh, Vector2.down * (radius + 1.5f), 2.6f, color);
        }

        private static void Ring(VertexHelper mesh, float radius, float thickness, Color tint)
        {
            const int segments = 80;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                float b = (i + 1) * Mathf.PI * 2f / segments;
                Vector2 first = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Vector2 second = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
                Quad(mesh, first * radius, second * radius, second * (radius - thickness), first * (radius - thickness), tint);
            }
        }

        private static void Disk(VertexHelper mesh, Vector2 center, float radius, Color tint)
        {
            const int segments = 64;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                float b = (i + 1) * Mathf.PI * 2f / segments;
                Triangle(mesh, center, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, tint);
            }
        }

        private static void Diamond(VertexHelper mesh, Vector2 center, float size, Color tint)
        {
            Quad(mesh, center + Vector2.up * size, center + Vector2.right * size * 0.6f,
                center + Vector2.down * size, center + Vector2.left * size * 0.6f, tint);
        }

        private static void Line(VertexHelper mesh, Vector2 from, Vector2 to, float width, Color tint)
        {
            Vector2 direction = (to - from).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (width * 0.5f);
            Quad(mesh, from + normal, to + normal, to - normal, from - normal, tint);
        }

        private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(a, tint, Vector2.zero); mesh.AddVert(b, tint, Vector2.zero);
            mesh.AddVert(c, tint, Vector2.zero); mesh.AddVert(d, tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2); mesh.AddTriangle(index, index + 2, index + 3);
        }

        private static void Triangle(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Color tint)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(a, tint, Vector2.zero); mesh.AddVert(b, tint, Vector2.zero); mesh.AddVert(c, tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
        }
    }
}
