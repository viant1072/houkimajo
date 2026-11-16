using UnityEngine;

namespace Houkimajo.Bullets
{
    /// <summary>Hình học dùng chung cho beam damage và xoá đạn.</summary>
    public static class CombatGeometry
    {
        public static float DistanceToSegmentSquared(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 delta = end - start;
            float t = delta.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(point - start, delta) / delta.sqrMagnitude) : 0f;
            return (point - start - delta * t).sqrMagnitude;
        }
    }
}
