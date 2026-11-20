using System;

namespace Houkimajo.Systems
{
    /// <summary>Cấp đã mua theo ID ổn định của skill, không phụ thuộc vị trí Q/E/R.</summary>
    [Serializable]
    public class SkillUpgradeSave
    {
        public string skillId;
        public int level;
    }
}
