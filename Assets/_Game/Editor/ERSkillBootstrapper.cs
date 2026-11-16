using System;
using Houkimajo.Skills;
using UnityEditor;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Tạo cấu hình E/R từ spec, cập nhật art và giữ balance đã chỉnh trên asset có sẵn.</summary>
    public static class ERSkillBootstrapper
    {
        private static readonly int[] Costs = { 0, 100, 250, 500, 1000 };

        public static SkillSO[] CreateAssets(DashSkillSO dash)
        {
            Bind(dash, "E", "broom_dash", "E_BroomDash", cast: "CastSpark", loop: "Trail", mastery: "MasteryEcho");
            dash.ringCoverage = 0.6f;
            var shield = Get<ShieldSkillSO>("MagicShield", "Khiên Phép", SkillSlot.E,
                i => new SkillLevelData { count = 1 + i / 2, duration = 5f + i * 0.5f, cooldown = 12f - i });
            Bind(shield, "E", "magic_shield", "E_MagicShield", loop: "ShieldRing", impact: "Hit", mastery: "MasteryBreak");
            shield.attributeName = "Bảo hộ";
            var dispel = Get<DispelSkillSO>("Dispel", "Hoá Giải", SkillSlot.E,
                i => new SkillLevelData { radius = 2f + i * 0.25f, cooldown = 12f - i });
            Bind(dispel, "E", "dispel", "E_Dispel", cast: "ClearRing", impact: "CoinSpark", mastery: "MasteryPush");
            dispel.attributeName = "Thanh tẩy";
            var hourglass = Get<HourglassSkillSO>("Hourglass", "Đồng Hồ Cát", SkillSlot.E,
                i => new SkillLevelData { radius = 2.5f + i * 0.25f, duration = 3f + i * 0.5f, cooldown = 14f - i });
            Bind(hourglass, "E", "hourglass", "E_Hourglass", loop: "SlowField", particle: "TimeMote", mastery: "MasteryField");
            hourglass.attributeName = "Thời gian";
            var teleport = Get<TeleportSkillSO>("Teleport", "Dịch Chuyển", SkillSlot.E,
                i => new SkillLevelData { range = 4f + i * 0.5f, cooldown = 8f - i * 0.75f });
            Bind(teleport, "E", "teleport", "E_Teleport", cast: "Departure", impact: "Arrival", mastery: "MasteryExplosion");
            teleport.attributeName = "Không gian";
            var meteor = Get<MeteorShowerSkillSO>("MeteorShower", "Mưa Thiên Thạch", SkillSlot.R,
                i => new SkillLevelData { count = 4 + i, damage = 80 + 10 * i, radius = 1f,
                    duration = 5f + i * 0.5f, cooldown = 0f });
            Bind(meteor, "R", "meteor_shower", "R_MeteorShower", projectile: "Projectile", warning: "WarningRing",
                impact: "Impact", mastery: "MasteryBurnZone", particle: "Ember");
            meteor.attributeName = "Thiên thể";
            var familiar = Get<FamiliarSummonSkillSO>("FamiliarSummon", "Triệu Hồi Linh Thú", SkillSlot.R,
                i => new SkillLevelData { count = 2 + i / 2, damage = 8, duration = 10f + i * 1.25f, cooldown = 0f });
            Bind(familiar, "R", "familiar_summon", "R_FamiliarSummon", loop: "Familiar", projectile: "Projectile",
                cast: "SummonRing", impact: "Impact", mastery: "MasteryBlock");
            familiar.attributeName = "Linh thú";
            familiar.projectilePrefab = SkillBootstrapper.CreateProjectile("FamiliarBullet", "familiar_summon",
                "R_FamiliarSummon_Projectile", 0.55f, "R");
            var beam = Get<StarBeamSkillSO>("StarBeam", "Tia Sao Băng", SkillSlot.R,
                i => new SkillLevelData { damage = Mathf.RoundToInt(30f + i * 3.75f), radius = 0.8f + i * 0.15f,
                    duration = 2f + i * 0.25f, cooldown = 0f });
            Bind(beam, "R", "star_beam", "R_StarBeam", loop: "BeamBody", cast: "BeamStart", end: "BeamEnd",
                impact: "Impact", mastery: "MasteryExplosion");
            beam.attributeName = "Tinh tú";
            var time = Get<TimeStopSkillSO>("TimeStop", "Ngưng Đọng", SkillSlot.R,
                i => new SkillLevelData { duration = 4f + i * 0.5f, cooldown = 0f });
            Bind(time, "R", "time_stop", "R_TimeStop", loop: "TimeSeal", overlay: "FrozenBulletOverlay",
                cast: "CastRipple", end: "ResumeBurst", mastery: "MasteryClearRing");
            time.attributeName = "Thời gian";
            SkillSO[] skills = { dash, shield, dispel, hourglass, teleport, meteor, familiar, beam, time };
            foreach (var skill in skills) EditorUtility.SetDirty(skill);
            return skills;
        }

        private static T Get<T>(string id, string name, SkillSlot slot, Func<int, SkillLevelData> build) where T : SkillSO
        {
            string path = "Assets/_Game/Data/Skills/Skill_" + id + ".asset";
            var skill = AssetDatabase.LoadAssetAtPath<T>(path);
            if (skill != null) return skill;
            skill = ScriptableObject.CreateInstance<T>();
            skill.id = "skill_" + id.ToLowerInvariant();
            skill.displayName = name;
            skill.slot = slot;
            skill.ringCoverage = 0.6f;
            skill.color = slot == SkillSlot.E ? new Color(0.55f, 0.8f, 1f) : new Color(1f, 0.8f, 0.4f);
            skill.levels = new SkillLevelData[5];
            for (int i = 0; i < skill.levels.Length; i++)
            {
                var level = build(i);
                level.hasMasteryEffect = i == 4;
                level.upgradeCost = Mathf.RoundToInt(Costs[i] * (slot == SkillSlot.R ? 1.5f : 1f));
                skill.levels[i] = level;
            }
            AssetDatabase.CreateAsset(skill, path);
            return skill;
        }

        private static void Bind(SkillSO skill, string group, string slug, string prefix,
            string cast = null, string loop = null, string end = null, string projectile = null,
            string impact = null, string mastery = null, string warning = null, string particle = null, string overlay = null)
        {
            skill.icon = Art(group, slug, prefix, "Icon");
            skill.castSprite = Art(group, slug, prefix, cast);
            skill.loopSprite = Art(group, slug, prefix, loop);
            skill.endSprite = Art(group, slug, prefix, end);
            skill.projectileSprite = Art(group, slug, prefix, projectile);
            skill.impactSprite = Art(group, slug, prefix, impact);
            skill.masterySprite = Art(group, slug, prefix, mastery);
            skill.warningSprite = Art(group, slug, prefix, warning);
            skill.particleSprite = Art(group, slug, prefix, particle);
            skill.overlaySprite = Art(group, slug, prefix, overlay);
        }

        private static Sprite Art(string group, string slug, string prefix, string part)
        {
            return part != null ? SkillBootstrapper.LoadArt(slug, prefix + "_" + part, group) : null;
        }
    }
}
