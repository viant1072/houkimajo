using System;
using UnityEngine;

namespace Houkimajo.Skills
{
    /// <summary>
    /// Skill đang lắp ở 3 ô + cấp của từng skill. Trước giai đoạn 3 (Progression) cấu hình
    /// tay trong Inspector của SkillCaster; sau đó Progression sẽ nạp từ file save.
    /// </summary>
    [Serializable]
    public class SkillLoadout
    {
        [SerializeField] private SkillSO qSkill;
        [SerializeField, Range(1, 5)] private int qLevel = 1;

        [SerializeField] private SkillSO eSkill;
        [SerializeField, Range(1, 5)] private int eLevel = 1;

        [SerializeField] private SkillSO rSkill;
        [SerializeField, Range(1, 5)] private int rLevel = 1;

        public SkillSO GetSkill(SkillSlot slot)
        {
            switch (slot)
            {
                case SkillSlot.Q: return qSkill;
                case SkillSlot.E: return eSkill;
                default: return rSkill;
            }
        }

        public int GetLevel(SkillSlot slot)
        {
            switch (slot)
            {
                case SkillSlot.Q: return qLevel;
                case SkillSlot.E: return eLevel;
                default: return rLevel;
            }
        }

        public void Set(SkillSlot slot, SkillSO skill, int level)
        {
            level = Mathf.Max(1, level);
            switch (slot)
            {
                case SkillSlot.Q: qSkill = skill; qLevel = level; break;
                case SkillSlot.E: eSkill = skill; eLevel = level; break;
                default: rSkill = skill; rLevel = level; break;
            }
        }
    }
}
