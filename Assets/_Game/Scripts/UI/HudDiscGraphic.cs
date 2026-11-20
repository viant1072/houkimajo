using UnityEngine;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    /// <summary>Đĩa tròn vector cho khung và mask avatar, không vỡ nét khi co HUD.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class HudDiscGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            mesh.AddVert(rect.center, color, new Vector2(0.5f, 0.5f));
            const int segments = 96;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                mesh.AddVert(rect.center + direction * radius, color, direction * 0.5f + Vector2.one * 0.5f);
                if (i > 0) mesh.AddTriangle(0, i, i + 1);
            }
        }
    }
}
