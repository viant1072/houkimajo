using Houkimajo.Bullets;
using Houkimajo.Systems;
using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>Trạng thái E/R của từng player. Pool/VFX tạo sẵn, không coroutine hay allocation trong Tick.</summary>
    [DefaultExecutionOrder(-50)]
    public class SkillEffectController : MonoBehaviour
    {
        private struct Lease
        {
            public SkillVfx Effect;
            public int Generation;
            public Lease(SkillVfx effect) { Effect = effect; Generation = effect != null ? effect.Generation : -1; }
            public bool Valid => Effect != null && Effect.IsActive && Effect.Generation == Generation;
            public void Stop() { if (Valid) Effect.Stop(); }
            public void Pose(Vector2 position, Vector2 size, float angle) { if (Valid) Effect.SetPose(position, size, angle); }
        }

        private struct Meteor
        {
            public MeteorShowerSkillSO Skill;
            public Vector2 Position;
            public float Age;
            public bool Falling;
            public int Damage;
            public float Radius;
            public bool Mastery;
        }

        private SkillContext context;
        private bool healthSubscribed;
        private ShieldSkillSO shield;
        private SkillLevelData shieldLevel;
        private Lease shieldVisual;
        private HourglassSkillSO slow;
        private SkillLevelData slowLevel;
        private float slowRemaining;
        private float moteTimer;
        private Lease slowVisual;
        private TimeStopSkillSO timeStop;
        private SkillLevelData timeLevel;
        private float timeRemaining;
        private bool freezeHeld;
        private Lease timeVisual;
        private MeteorShowerSkillSO shower;
        private SkillLevelData showerLevel;
        private float showerRemaining;
        private float meteorTimer;
        private readonly Meteor[] meteors = new Meteor[64];
        private FamiliarSummonSkillSO familiar;
        private SkillLevelData familiarLevel;
        private float familiarRemaining;
        private float familiarFireTimer;
        private float orbitAngle;
        private readonly SpriteRenderer[] cats = new SpriteRenderer[4];
        private readonly int[] blocked = new int[4];
        private StarBeamSkillSO beam;
        private SkillLevelData beamLevel;
        private float beamRemaining;
        private float beamTick;
        private Vector2 beamEnd;
        private Lease beamBody, beamStart, beamTip;

        public bool IsBeamActive => beamRemaining > 0f;
        public bool IsTimeStopActive => timeRemaining > 0f;
        public int ActiveFamiliarCount => familiarRemaining > 0f ? Mathf.Clamp(familiarLevel.count, 1, cats.Length) : 0;
        public int PendingMeteorCount
        {
            get { int count = 0; for (int i = 0; i < meteors.Length; i++) if (meteors[i].Skill != null) count++; return count; }
        }

        private void Awake()
        {
            for (int i = 0; i < cats.Length; i++)
            {
                var go = new GameObject("Familiar_" + i);
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * 0.65f;
                cats[i] = go.AddComponent<SpriteRenderer>();
                cats[i].sortingOrder = 4;
                go.SetActive(false);
            }
        }

        public void Bind(SkillContext ctx)
        {
            Unsubscribe();
            context = ctx;
            Subscribe();
        }

        private void OnEnable() { Subscribe(); }
        private void Subscribe()
        {
            if (healthSubscribed || context == null || context.Health == null) return;
            context.Health.OnShieldBlocked += HandleShieldHit;
            healthSubscribed = true;
        }
        private void Unsubscribe()
        {
            if (healthSubscribed && context != null && context.Health != null)
                context.Health.OnShieldBlocked -= HandleShieldHit;
            healthSubscribed = false;
        }

        private void Update() { Advance(Time.deltaTime); }

        public void Advance(float deltaTime)
        {
            if (context == null || deltaTime <= 0f) return;
            if (shield != null && (context.Health == null || context.Health.ShieldHits <= 0))
            { shieldVisual.Stop(); shield = null; }
            TickSlow(deltaTime);
            TickTimeStop(deltaTime);
            TickMeteors(deltaTime);
            TickFamiliars(deltaTime);
            TickBeam(deltaTime);
        }

        public void ShowDash(DashSkillSO skill, SkillLevelData level, Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Play(skill.castSprite, context.CasterPosition, 0.5f, 0.2f);
            var trail = Play(skill.loopSprite, context.CasterPosition, 0.9f, skill.dashDuration, angle);
            trail?.Follow(context.Caster, -direction.normalized * 0.35f);
            if (level.hasMasteryEffect) Play(skill.masterySprite, context.CasterPosition, 0.7f, 0.3f, angle);
        }

        public void BeginShield(ShieldSkillSO skill, SkillLevelData level)
        {
            if (context.Health == null) return;
            shieldVisual.Stop();
            shield = skill; shieldLevel = level;
            context.Health.AddShield(level.count, level.duration, skill.blockInvincibility);
            var visual = Ring(skill, skill.loopSprite, context.CasterPosition, 0.65f, level.duration + 0.1f);
            visual?.Follow(context.Caster, Vector2.zero);
            shieldVisual = new Lease(visual);
        }

        private void HandleShieldHit(bool broken)
        {
            if (shield == null) return;
            Play(shield.impactSprite, context.CasterPosition, 0.7f, 0.25f);
            if (!broken) return;
            if (shieldLevel.hasMasteryEffect)
            {
                context.EnemyBullets?.ClearInRadius(context.CasterPosition, shield.breakRadius);
                Ring(shield, shield.masterySprite, context.CasterPosition, shield.breakRadius, 0.35f);
            }
            shieldVisual.Stop();
            shield = null;
        }

        public void Dispel(DispelSkillSO skill, SkillLevelData level)
        {
            int count = context.EnemyBullets != null ? context.EnemyBullets.ClearInRadius(context.CasterPosition, level.radius) : 0;
            int gold = Mathf.Min(skill.maximumGold, count);
            context.Wallet?.AddGold(gold);
            Ring(skill, skill.castSprite, context.CasterPosition, level.radius, 0.35f);
            if (gold > 0) Play(skill.impactSprite, context.CasterPosition + Vector2.up * 0.5f, 0.7f, 0.6f);
            if (!level.hasMasteryEffect) return;
            Ring(skill, skill.masterySprite, context.CasterPosition, level.radius, 0.4f);
            for (int i = 0; i < TargetRegistry.Count; i++)
            {
                var target = TargetRegistry.GetTarget(i);
                Vector2 delta = (Vector2)TargetRegistry.GetTransform(i).position - context.CasterPosition;
                if (delta.sqrMagnitude <= level.radius * level.radius && target is IKnockbackTarget movable)
                    movable.ApplyKnockback(delta.normalized, skill.knockbackDistance);
            }
        }

        public void BeginSlowField(HourglassSkillSO skill, SkillLevelData level)
        {
            slowVisual.Stop(); slow = skill; slowLevel = level; slowRemaining = level.duration; moteTimer = 0f;
            var visual = Ring(skill, level.hasMasteryEffect ? skill.masterySprite : skill.loopSprite,
                context.CasterPosition, level.radius, level.duration + 0.1f, 0.5f);
            visual?.Follow(context.Caster, Vector2.zero);
            slowVisual = new Lease(visual);
        }

        private void TickSlow(float deltaTime)
        {
            if (slowRemaining <= 0f) return;
            float fraction = slowLevel.hasMasteryEffect ? slow.masteryBulletSlow : slow.bulletSlow;
            context.EnemyBullets?.ApplySlowInRadius(context.CasterPosition, slowLevel.radius, 1f - fraction);
            if (slowLevel.hasMasteryEffect)
                for (int i = 0; i < TargetRegistry.Count; i++)
                {
                    var target = TargetRegistry.GetTarget(i);
                    if (((Vector2)TargetRegistry.GetTransform(i).position - context.CasterPosition).sqrMagnitude
                        <= slowLevel.radius * slowLevel.radius && target is ISkillStatusTarget status)
                        status.ApplySlow(slow.masteryEnemySlow, 0.1f);
                }
            moteTimer -= deltaTime;
            if (moteTimer <= 0f)
            {
                moteTimer = 0.3f;
                float angle = Random.value * Mathf.PI * 2f;
                Play(slow.particleSprite, context.CasterPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))
                    * slowLevel.radius, 0.35f, 0.6f);
            }
            slowRemaining -= deltaTime;
            if (slowRemaining <= 0f) slowVisual.Stop();
        }

        public void Teleport(TeleportSkillSO skill, SkillLevelData level)
        {
            if (context.Controller == null) return;
            Vector2 departure = context.CasterPosition;
            Ring(skill, skill.castSprite, departure, 0.6f, 0.25f);
            context.Controller.TeleportTo(context.AimPoint, level.range);
            context.Health?.GrantInvincibility(skill.invincibility);
            Ring(skill, skill.impactSprite, context.CasterPosition, 0.65f, 0.3f);
            if (!level.hasMasteryEffect) return;
            TargetRegistry.DamageInRadius(departure, skill.masteryRadius, context.ScaleDamage(skill.masteryDamage), DamageSource.Skill);
            Ring(skill, skill.masterySprite, departure, skill.masteryRadius, 0.35f);
        }

        public void BeginTimeStop(TimeStopSkillSO skill, SkillLevelData level)
        {
            if (!freezeHeld && context.EnemyBullets != null)
            { context.EnemyBullets.SetTimeStopped(true, skill.overlaySprite); freezeHeld = true; }
            timeVisual.Stop(); timeStop = skill; timeLevel = level; timeRemaining = level.duration;
            Ring(skill, skill.castSprite, context.CasterPosition, 1.5f, 0.4f);
            var visual = Ring(skill, skill.loopSprite, context.CasterPosition, 0.8f, level.duration + 0.1f, 0.5f);
            visual?.Follow(context.Caster, Vector2.zero);
            timeVisual = new Lease(visual);
        }

        private void TickTimeStop(float deltaTime)
        {
            if (timeRemaining <= 0f) return;
            timeRemaining -= deltaTime;
            if (timeRemaining > 0f) return;
            ReleaseFreeze();
            Play(timeStop.endSprite, context.CasterPosition, 1.8f, 0.35f);
            if (timeLevel.hasMasteryEffect)
            {
                context.EnemyBullets?.ClearInRadius(context.CasterPosition, timeStop.masteryClearRadius);
                Ring(timeStop, timeStop.masterySprite, context.CasterPosition, timeStop.masteryClearRadius, 0.4f);
            }
            timeVisual.Stop();
        }

        private void ReleaseFreeze()
        {
            if (freezeHeld && context != null && context.EnemyBullets != null) context.EnemyBullets.SetTimeStopped(false);
            freezeHeld = false;
        }

        public void BeginMeteorShower(MeteorShowerSkillSO skill, SkillLevelData level)
        {
            shower = skill; showerLevel = level; showerRemaining = level.duration; meteorTimer = 1f / Mathf.Max(1, level.count);
        }

        private void TickMeteors(float deltaTime)
        {
            if (showerRemaining > 0f)
            {
                float activeDelta = Mathf.Min(deltaTime, showerRemaining);
                meteorTimer -= activeDelta;
                while (meteorTimer <= 0.0001f)
                {
                    ScheduleMeteor(-activeDelta - meteorTimer);
                    meteorTimer += 1f / Mathf.Max(1, showerLevel.count);
                }
                showerRemaining -= deltaTime;
            }
            for (int i = 0; i < meteors.Length; i++)
            {
                if (meteors[i].Skill == null) continue;
                var item = meteors[i];
                item.Age += deltaTime;
                if (!item.Falling && item.Age >= item.Skill.warningDuration)
                {
                    item.Falling = true;
                    Vector2 from = item.Position + Vector2.up * item.Skill.fallHeight;
                    var falling = Play(item.Skill.projectileSprite, from, 0.9f, item.Skill.fallDuration, 180f);
                    falling?.MoveTo(from, item.Position, item.Skill.fallDuration);
                }
                if (item.Age >= item.Skill.warningDuration + item.Skill.fallDuration)
                {
                    TargetRegistry.DamageInRadius(item.Position, item.Radius, item.Damage, DamageSource.Ultimate);
                    context.EnemyBullets?.ClearInRadius(item.Position, item.Radius);
                    Ring(item.Skill, item.Skill.impactSprite, item.Position, item.Radius, 0.35f);
                    for (int particle = 0; particle < 3; particle++)
                        Play(item.Skill.particleSprite, item.Position + Random.insideUnitCircle * item.Radius, 0.25f, 0.4f, Random.value * 360f);
                    if (item.Mastery)
                        context.Pools?.Effects?.PlayBurningZone(item.Skill.masterySprite, item.Position,
                            item.Radius * 2f / item.Skill.ringCoverage, item.Skill.burningDuration, item.Radius,
                            context.ScaleDamage(Mathf.RoundToInt(item.Skill.burningDamagePerSecond * item.Skill.burnTickInterval)),
                            item.Skill.burnTickInterval, DamageSource.Ultimate);
                    item.Skill = null;
                }
                meteors[i] = item;
            }
        }

        private void ScheduleMeteor(float initialAge)
        {
            int free = -1;
            for (int i = 0; i < meteors.Length; i++) if (meteors[i].Skill == null) { free = i; break; }
            if (free < 0) return;
            var bounds = ArenaBounds.Get();
            Vector2 position = new Vector2(Random.Range(bounds.xMin + 0.3f, bounds.xMax - 0.3f),
                Random.Range(bounds.yMin + 0.3f, bounds.yMax - 0.3f));
            if (TargetRegistry.Count > 0 && Random.value < 0.5f)
            {
                int index = Random.Range(0, TargetRegistry.Count);
                if (TargetRegistry.GetTarget(index).IsAlive) position = TargetRegistry.GetTransform(index).position;
            }
            meteors[free] = new Meteor { Skill = shower, Position = position,
                Age = initialAge,
                Damage = context.ScaleDamage(showerLevel.damage), Radius = showerLevel.radius,
                Mastery = showerLevel.hasMasteryEffect };
            Ring(shower, shower.warningSprite, position, showerLevel.radius, shower.warningDuration + shower.fallDuration, 0.75f);
        }

        public void BeginFamiliars(FamiliarSummonSkillSO skill, SkillLevelData level)
        {
            familiar = skill; familiarLevel = level; familiarRemaining = level.duration; familiarFireTimer = skill.fireInterval;
            int count = ActiveFamiliarCount;
            for (int i = 0; i < cats.Length; i++)
            {
                if (cats[i] == null) continue;
                blocked[i] = 0;
                cats[i].sprite = skill.loopSprite;
                cats[i].gameObject.SetActive(i < count);
            }
            Ring(skill, skill.castSprite, context.CasterPosition, 1f, 0.5f);
        }

        private void TickFamiliars(float deltaTime)
        {
            if (familiarRemaining <= 0f) return;
            int count = ActiveFamiliarCount;
            orbitAngle += deltaTime * familiar.orbitSpeed;
            for (int i = 0; i < count; i++)
            {
                float radians = (orbitAngle + i * 360f / count) * Mathf.Deg2Rad;
                Vector2 position = context.CasterPosition + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * familiar.orbitRadius;
                cats[i].transform.position = position;
                cats[i].flipX = context.AimDirection.x < 0f;
                if (familiarLevel.hasMasteryEffect && blocked[i] < familiar.maximumBlocks && context.EnemyBullets != null)
                {
                    int cleared = context.EnemyBullets.ClearInRadius(position, familiar.blockRadius, familiar.maximumBlocks - blocked[i]);
                    blocked[i] += cleared;
                    if (cleared > 0) Play(familiar.masterySprite, position, 0.5f, 0.2f);
                }
            }
            familiarFireTimer -= Mathf.Min(deltaTime, familiarRemaining);
            while (familiarFireTimer <= 0.0001f)
            {
                for (int i = 0; i < count; i++)
                {
                    var projectile = context.Pools?.GetPool(familiar.projectilePrefab, 96)?.Get(cats[i].transform.position);
                    if (projectile == null) continue;
                    Vector2 direction = context.AimPoint - (Vector2)cats[i].transform.position;
                    if (direction.sqrMagnitude < 0.0001f) direction = context.AimDirection;
                    projectile.Launch(direction.normalized, familiar.bulletSpeed, context.ScaleDamage(familiarLevel.damage),
                        DamageSource.Ultimate, familiar.bulletLifetime);
                    projectile.SetArt(familiar, context.Pools.Effects);
                }
                familiarFireTimer += Mathf.Max(0.02f, familiar.fireInterval);
            }
            familiarRemaining -= deltaTime;
            if (familiarRemaining <= 0f) HideCats();
        }

        private void HideCats()
        {
            for (int i = 0; i < cats.Length; i++) if (cats[i] != null) cats[i].gameObject.SetActive(false);
        }

        public void BeginBeam(StarBeamSkillSO skill, SkillLevelData level)
        {
            EndBeam(false);
            beam = skill; beamLevel = level; beamRemaining = level.duration; beamTick = 0f;
            if (context.Shooting != null) context.Shooting.SkillFireBlocked = true;
            if (context.Controller != null) context.Controller.SkillSpeedMultiplier = skill.movementMultiplier;
            beamBody = new Lease(Play(skill.loopSprite, context.CasterPosition, 1f, level.duration + 0.1f));
            beamStart = new Lease(Play(skill.castSprite, context.CasterPosition, 1f, level.duration + 0.1f));
            beamTip = new Lease(Play(skill.endSprite, context.CasterPosition, 1f, level.duration + 0.1f));
        }

        private void TickBeam(float deltaTime)
        {
            if (beamRemaining <= 0f) return;
            Vector2 direction = context.AimDirection.normalized;
            Vector2 start = context.CasterPosition;
            var bounds = ArenaBounds.Get();
            float length = float.MaxValue;
            if (Mathf.Abs(direction.x) > 0.0001f)
                length = Mathf.Min(length, ((direction.x > 0f ? bounds.xMax : bounds.xMin) - start.x) / direction.x);
            if (Mathf.Abs(direction.y) > 0.0001f)
                length = Mathf.Min(length, ((direction.y > 0f ? bounds.yMax : bounds.yMin) - start.y) / direction.y);
            length = Mathf.Clamp(length, 0.01f, 30f);
            beamEnd = start + direction * length;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float width = beamLevel.radius;
            beamBody.Pose((start + beamEnd) * 0.5f,
                new Vector2(width / Mathf.Max(0.01f, beam.bodyWidthCoverage), length / Mathf.Max(0.01f, beam.bodyLengthCoverage)), angle);
            beamStart.Pose(start, Vector2.one * width / Mathf.Max(0.01f, beam.capCoverage), angle);
            beamTip.Pose(beamEnd, Vector2.one * width / Mathf.Max(0.01f, beam.capCoverage), angle);
            context.EnemyBullets?.ClearAlongLine(start, beamEnd, width);
            beamTick += Mathf.Min(deltaTime, beamRemaining);
            float interval = Mathf.Max(0.02f, beam.tickInterval);
            while (beamTick + 0.0001f >= interval)
            {
                beamTick -= interval;
                for (int i = TargetRegistry.Count - 1; i >= 0; i--)
                {
                    if (i >= TargetRegistry.Count) continue;
                    var target = TargetRegistry.GetTarget(i);
                    Vector2 position = TargetRegistry.GetTransform(i).position;
                    if (CombatGeometry.DistanceToSegmentSquared(position, start, beamEnd) > width * width * 0.25f) continue;
                    Play(beam.impactSprite, position, 0.4f, 0.12f);
                    DamageEvents.DealPlayerDamage(target, context.ScaleDamage(beamLevel.damage), DamageSource.Ultimate);
                }
            }
            beamRemaining -= deltaTime;
            if (beamRemaining <= 0f) EndBeam(true);
        }

        private void EndBeam(bool mastery)
        {
            beamBody.Stop(); beamStart.Stop(); beamTip.Stop();
            if (context == null) return;
            if (context.Shooting != null) context.Shooting.SkillFireBlocked = false;
            if (context.Controller != null) context.Controller.SkillSpeedMultiplier = 1f;
            if (mastery && beam != null && beamLevel.hasMasteryEffect)
            {
                TargetRegistry.DamageInRadius(beamEnd, beam.masteryRadius, context.ScaleDamage(beam.masteryDamage), DamageSource.Ultimate);
                Ring(beam, beam.masterySprite, beamEnd, beam.masteryRadius, 0.4f);
            }
            beamRemaining = 0f;
        }

        private SkillVfx Play(Sprite sprite, Vector2 position, float size, float duration, float angle = 0f)
        {
            return context.Pools?.Effects?.Play(sprite, position, Vector2.one * size, duration, angle);
        }

        private SkillVfx Ring(SkillSO skill, Sprite sprite, Vector2 position, float radius, float duration, float opacity = 0.7f)
        {
            float size = radius * 2f / Mathf.Max(0.1f, skill.ringCoverage);
            return context.Pools?.Effects?.Play(sprite, position, Vector2.one * size, duration, opacity: opacity);
        }

        private void OnDisable()
        {
            Unsubscribe(); ReleaseFreeze(); EndBeam(false);
            if (context != null && context.Health != null) context.Health.ClearShield();
            shieldVisual.Stop(); slowVisual.Stop(); timeVisual.Stop(); HideCats();
            slowRemaining = timeRemaining = showerRemaining = familiarRemaining = 0f;
            for (int i = 0; i < meteors.Length; i++) meteors[i].Skill = null;
        }
    }
}
