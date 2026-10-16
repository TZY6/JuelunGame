using System;
namespace BetterDesign.Core
{
    public enum FactKind { Kills, ItemCount, Level, LocationVisited, DialogueCompleted }
    [Serializable]
    public struct ConditionConfig
    {
        public FactKind fact;
        public string targetId;
        public int required;
    }
    public interface IConditionFacts { int Read(FactKind fact,string targetId); }
    public static class ConditionEvaluator
    {
        public static bool Evaluate(ConditionConfig condition,IConditionFacts facts)
        {
            return facts != null && condition.required > 0 && facts.Read(condition.fact,condition.targetId) >= condition.required;
        }
    }
    [Serializable] public sealed class ProgressionSaveData { public int level = 1, experience, skillPoints, weapon, magicRank; }
    public sealed class ProgressionModel
    {
        public const int MaxLevel = 100;
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int SkillPoints { get; private set; }
        public int MagicRank { get; private set; }
        public bool TryUpgradeMagic()
        {
            if (Level < 2 || SkillPoints == 0 || MagicRank == 3) return false;
            SkillPoints--; MagicRank++; return true;
        }
        public int Weapon { get; private set; }
        public int RequiredExperience { get { return Level*30; } }
        public void Equip(int weapon) { if (weapon < 0 || weapon > 1) throw new ArgumentOutOfRangeException(nameof(weapon)); Weapon = weapon; }
        public void AddExperience(int amount)
        {
            if (amount <= 0 || Level == MaxLevel) return;
            Experience = Math.Min(1000000,Experience + Math.Min(amount,1000000));
            while (Level < MaxLevel && Experience >= RequiredExperience) { Experience -= RequiredExperience; Level++; SkillPoints++; }
            if (Level == MaxLevel) Experience = 0;
        }
        public ProgressionSaveData Capture() { return new ProgressionSaveData { level = Level, experience = Experience, skillPoints = SkillPoints, weapon = Weapon, magicRank = MagicRank }; }
        public static bool IsValid(ProgressionSaveData data)
        {
            return data != null && data.level >= 1 && data.level <= MaxLevel && data.experience >= 0 &&
                data.experience < data.level*30 && (data.level != MaxLevel || data.experience == 0) &&
                data.skillPoints >= 0 && data.magicRank >= 0 && data.magicRank <= 3 && data.skillPoints + data.magicRank == data.level-1 && data.weapon >= 0 && data.weapon <= 1;
        }
        public void Restore(ProgressionSaveData data)
        {
            if (!IsValid(data)) throw new ArgumentException("Invalid progression.");
            Level = data.level; Experience = data.experience; SkillPoints = data.skillPoints; Weapon = data.weapon; MagicRank = data.magicRank;
        }
    }
}
