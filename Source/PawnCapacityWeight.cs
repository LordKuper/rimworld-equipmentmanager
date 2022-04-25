using JetBrains.Annotations;
using Verse;

namespace EquipmentManager
{
    internal class PawnCapacityWeight : IExposable
    {
        public const float WeightCap = 2f;
        private bool _isInitialized;
        private PawnCapacityDef _pawnCapacityDef;
        private string _pawnCapacityDefName;
        public float Weight;

        [UsedImplicitly]
        public PawnCapacityWeight() { }

        public PawnCapacityWeight(string pawnCapacityDefName)
        {
            _pawnCapacityDefName = pawnCapacityDefName;
        }

        public PawnCapacityWeight(string pawnCapacityDefName, float weight)
        {
            _pawnCapacityDefName = pawnCapacityDefName;
            Weight = weight;
        }

        public PawnCapacityDef PawnCapacityDef
        {
            get
            {
                Initialize();
                return _pawnCapacityDef;
            }
        }

        public string PawnCapacityDefName => _pawnCapacityDefName;

        public void ExposeData()
        {
            Scribe_Values.Look(ref _pawnCapacityDefName, nameof(PawnCapacityDefName));
            Scribe_Values.Look(ref Weight, nameof(Weight));
        }

        private void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            _pawnCapacityDef = DefDatabase<PawnCapacityDef>.GetNamedSilentFail(_pawnCapacityDefName);
        }
    }
}