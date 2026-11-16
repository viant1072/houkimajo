using Houkimajo.Bullets;
using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Chọn chuỗi khác mục tiêu trước khi damage để địch chết không làm lệch chỉ mục.</summary>
    [CreateAssetMenu(fileName = "Skill_ChainLightning", menuName = "Houkimajo/Skills/Chain Lightning")]
    public class ChainLightningSkillSO : SkillSO
    {
        public Sprite arcSprite;
        public float jumpRange = 3f;
        public float arcDuration = 0.16f;
        public float arcWidth = 0.65f;
        public float arcCoverage = 0.88f;
        public float stunDuration = 0.5f;

        public override void Activate(SkillContext ctx, SkillLevelData data)
        {
            // Mảng là cục bộ lần cast, không lưu trạng thái runtime trên ScriptableObject.
            var selected = new IDamageable[Mathf.Max(1, data.count)];
            var positions = new Vector2[selected.Length];
            Vector2 point = ctx.AimPoint;
            int total = 0;
            for (int chain = 0; chain < selected.Length; chain++)
            {
                float bestDistance = (chain == 0 ? data.range : jumpRange);
                bestDistance *= bestDistance;
                int best = -1;
                for (int i = 0; i < TargetRegistry.Count; i++)
                {
                    var target = TargetRegistry.GetTarget(i);
                    var targetTransform = TargetRegistry.GetTransform(i);
                    if (!target.IsAlive || targetTransform == null) continue;
                    bool visited = false;
                    for (int j = 0; j < total; j++)
                        if (selected[j] == target) visited = true;
                    if (visited) continue;
                    float distance = ((Vector2)targetTransform.position - point).sqrMagnitude;
                    if (distance > bestDistance) continue;
                    best = i;
                    bestDistance = distance;
                }
                if (best < 0) break;
                selected[total] = TargetRegistry.GetTarget(best);
                point = TargetRegistry.GetTransform(best).position;
                positions[total++] = point;
            }
            Vector2 previous = ctx.CasterPosition;
            for (int i = 0; i < total; i++)
            {
                Vector2 delta = positions[i] - previous;
                float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;
                ctx.Pools?.Effects?.Play(arcSprite, (positions[i] + previous) * 0.5f,
                    new Vector2(arcWidth, delta.magnitude / Mathf.Max(0.1f, arcCoverage)), arcDuration, angle);
                ctx.Pools?.Effects?.Play(impactSprite, positions[i], Vector2.one * impactSize, impactDuration);
                if (data.hasMasteryEffect && selected[i] is ISkillStatusTarget status)
                {
                    status.ApplyStun(stunDuration);
                    ctx.Pools?.Effects?.Play(masterySprite, positions[i], Vector2.one * 0.7f, stunDuration);
                }
                DamageEvents.DealPlayerDamage(selected[i], ctx.ScaleDamage(data.damage), DamageSource.Skill);
                previous = positions[i];
            }
        }
    }
}
