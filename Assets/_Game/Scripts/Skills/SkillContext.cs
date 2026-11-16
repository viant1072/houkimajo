using UnityEngine;
using Houkimajo.Bullets;
using Houkimajo.Player;
using Houkimajo.Systems;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Mọi thứ skill cần biết lúc được dùng. SkillCaster tạo 1 lần rồi tái sử dụng (không
    /// cấp phát mỗi lần bấm skill). Gom về 1 object để thêm thông tin mới (vd. PlayerStats
    /// ở giai đoạn 3) không phải đổi chữ ký hàm Activate của cả 13 skill.
    /// </summary>
    public class SkillContext
    {
        public Transform Caster { get; }
        public PlayerAim Aim { get; }
        public PlayerController Controller { get; }
        public PlayerHealth Health { get; }
        public SkillPools Pools { get; }
        public SkillEffectController Runtime { get; }
        public PlayerShooting Shooting { get; }
        public Wallet Wallet { get; }

        /// <summary>Pool đạn địch — để skill xoá / làm chậm / đóng băng đạn địch.</summary>
        public BulletPool EnemyBullets { get; }

        /// <summary>Hệ số sát thương skill từ trang bị/thẻ bị động. Tạm = 1 tới giai đoạn 3.</summary>
        public float DamageMultiplier { get; set; } = 1f;

        /// <summary>Hệ số hồi chiêu (0.85 = giảm 15%). Tạm = 1 tới giai đoạn 3.</summary>
        public float CooldownMultiplier { get; set; } = 1f;

        public Vector2 CasterPosition => Caster.position;
        public Vector2 AimPoint => Aim != null ? Aim.AimPoint : CasterPosition + Vector2.up;
        public Vector2 AimDirection => Aim != null ? Aim.AimDirection : Vector2.up;

        public SkillContext(Transform caster, PlayerAim aim, PlayerController controller,
            PlayerHealth health, SkillPools pools, BulletPool enemyBullets)
        {
            Caster = caster;
            Aim = aim;
            Controller = controller;
            Health = health;
            Pools = pools;
            EnemyBullets = enemyBullets;
            Runtime = caster.GetComponent<SkillEffectController>();
            Shooting = caster.GetComponent<PlayerShooting>();
            Wallet = caster.GetComponent<Wallet>();
        }

        /// <summary>Sát thương đã nhân hệ số, làm tròn.</summary>
        public int ScaleDamage(int baseDamage)
        {
            return Mathf.RoundToInt(baseDamage * DamageMultiplier);
        }
    }
}
