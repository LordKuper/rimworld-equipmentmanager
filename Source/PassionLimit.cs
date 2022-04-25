using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    public enum PassionValue
    {
        None,
        Minor,
        Major,
        Any
    }

    internal class PassionLimit : IExposable
    {
        private bool _isInitialized;
        private SkillDef _skillDef;
        private string _skillDefName;
        public PassionValue Value = PassionValue.None;

        [UsedImplicitly]
        public PassionLimit() { }

        public PassionLimit(string skillDefName)
        {
            _skillDefName = skillDefName;
        }

        public SkillDef SkillDef
        {
            get
            {
                Initialize();
                return _skillDef;
            }
        }

        public string SkillDefName => _skillDefName;

        public void ExposeData()
        {
            Scribe_Values.Look(ref _skillDefName, nameof(SkillDefName));
            Scribe_Values.Look(ref Value, nameof(Value));
        }

        private void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            _skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(_skillDefName);
            if (SkillDef == null) { }
        }
    }
}