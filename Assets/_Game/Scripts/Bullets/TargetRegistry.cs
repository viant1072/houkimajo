using System.Collections.Generic;
using UnityEngine;

namespace Houkimajo.Bullets
{
    /// <summary>
    /// Danh sách mọi kẻ địch đang có mặt trong arena (boss, quái). Kẻ địch tự đăng ký
    /// lúc OnEnable và huỷ lúc OnDisable/chết. Skill tự tìm mục tiêu (tên lửa, sét...)
    /// duyệt danh sách này thay vì FindObjectsOfType hay Physics2D.Overlap mỗi lần dùng.
    /// </summary>
    public static class TargetRegistry
    {
        private static readonly List<IDamageable> targets = new List<IDamageable>();
        private static readonly List<Transform> transforms = new List<Transform>();

        public static int Count => targets.Count;

        public static IDamageable GetTarget(int index) => targets[index];
        public static Transform GetTransform(int index) => transforms[index];

        public static void Register(IDamageable target, Transform targetTransform)
        {
            if (target == null || targets.Contains(target)) return;
            targets.Add(target);
            transforms.Add(targetTransform);
        }

        public static void Unregister(IDamageable target)
        {
            int index = targets.IndexOf(target);
            if (index < 0) return;
            targets.RemoveAt(index);
            transforms.RemoveAt(index);
        }

        /// <summary>
        /// Tìm kẻ địch còn sống gần <paramref name="point"/> nhất trong bán kính
        /// <paramref name="maxDistance"/>. Trả về -1 nếu không có.
        /// </summary>
        public static int FindNearest(Vector2 point, float maxDistance = float.MaxValue)
        {
            int bestIndex = -1;
            float bestSqr = maxDistance >= float.MaxValue ? float.MaxValue : maxDistance * maxDistance;

            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets[i].IsAlive) continue;

                float sqr = ((Vector2)transforms[i].position - point).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        /// <summary>Gây sát thương lên mọi kẻ địch trong bán kính (vụ nổ), bỏ qua <paramref name="exclude"/>.</summary>
        public static void DamageInRadius(Vector2 center, float radius, int damage,
            DamageSource source, IDamageable exclude = null)
        {
            float radiusSqr = radius * radius;

            // Duyệt ngược: sát thương có thể giết địch -> nó tự Unregister khỏi list.
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (i >= targets.Count) continue;
                var target = targets[i];
                if (target == exclude) continue;

                if (((Vector2)transforms[i].position - center).sqrMagnitude <= radiusSqr)
                    DamageEvents.DealPlayerDamage(target, damage, source);
            }
        }

        // Tắt Domain Reload thì list static còn giữ object của lần Play trước -> dọn sạch.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            targets.Clear();
            transforms.Clear();
        }
    }
}
