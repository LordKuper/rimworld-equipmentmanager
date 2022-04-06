using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class StatWeight : IExposable
    {
        public const float StatWeightCap = 2f;
        private bool _isInitialized;
        private bool _protected;
        private StatDef _statDef;
        private string _statDefName;
        public float Weight;

        [UsedImplicitly]
        public StatWeight() { }

        public StatWeight(string statDefName, bool isProtected)
        {
            _statDefName = statDefName;
            _protected = isProtected;
        }

        public StatWeight(string statDefName, float weight, bool isProtected)
        {
            _statDefName = statDefName;
            Weight = weight;
            _protected = isProtected;
        }

        public bool Protected => _protected;

        public StatDef StatDef
        {
            get
            {
                Initialize();
                return _statDef;
            }
        }

        public string StatDefName => _statDefName;

        public void ExposeData()
        {
            Scribe_Values.Look(ref _statDefName, nameof(StatDefName));
            Scribe_Values.Look(ref _protected, nameof(Protected));
            Scribe_Values.Look(ref Weight, nameof(Weight));
        }

        private void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            _statDef = StatHelper.GetStatDef(_statDefName);
        }
    }
}