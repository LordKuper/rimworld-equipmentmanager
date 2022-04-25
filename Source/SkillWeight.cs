using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class SkillWeight : IExposable
    {
        public const float WeightCap = 2f;
        private bool _isInitialized;
        private SkillDef _skillDef;
        private string _skillDefName;
        public float Weight;

        [UsedImplicitly]
        public SkillWeight() { }

        public SkillWeight(string skillDefName)
        {
            _skillDefName = skillDefName;
        }

        public SkillWeight(string skillDefName, float weight)
        {
            _skillDefName = skillDefName;
            Weight = weight;
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
            Scribe_Values.Look(ref Weight, nameof(Weight));
        }

        private void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            _skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(_skillDefName);
        }
    }
}