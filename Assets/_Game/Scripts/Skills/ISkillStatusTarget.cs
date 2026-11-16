namespace Houkimajo.Skills
{
    /// <summary>Quái thường nhận trạng thái qua giao tiếp này; boss không implement.</summary>
    public interface ISkillStatusTarget
    {
        bool IsSlowed { get; }
        void ApplySlow(float fraction, float duration);
        void ApplyStun(float duration);
    }
}
