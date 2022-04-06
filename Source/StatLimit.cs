using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class StatLimit : IExposable
    {
        public const float StatLimitCap = 1000f;
        private bool _isInitialized;
        private StatDef _statDef;
        private string _statDefName;
        public float? MaxValue;
        public string MaxValueBuffer;
        public float? MinValue;
        public string MinValueBuffer;

        [UsedImplicitly]
        public StatLimit() { }

        public StatLimit(string statDefName)
        {
            _statDefName = statDefName;
        }

        public StatLimit(string statDefName, float? minValue, float? maxValue)
        {
            _statDefName = statDefName;
            MinValue = minValue;
            MaxValue = maxValue;
        }

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
            Scribe_Values.Look(ref MinValue, nameof(MinValue));
            Scribe_Values.Look(ref MaxValue, nameof(MaxValue));
        }

        private void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            _statDef = StatHelper.GetStatDef(_statDefName);
        }
    }
}