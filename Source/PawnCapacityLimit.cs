using System.Globalization;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace EquipmentManager
{
    internal class PawnCapacityLimit : IExposable
    {
        private const float ValueCap = 500f;
        private bool _isInitialized;
        private PawnCapacityDef _pawnCapacityDef;
        private string _pawnCapacityDefName;
        public float? MaxValue;
        public string MaxValueBuffer;
        public float? MinValue;
        public string MinValueBuffer;

        [UsedImplicitly]
        public PawnCapacityLimit() { }

        public PawnCapacityLimit(string pawnCapacityDefName)
        {
            _pawnCapacityDefName = pawnCapacityDefName;
        }

        public PawnCapacityLimit(string pawnCapacityDefName, float? minValue, float? maxValue)
        {
            _pawnCapacityDefName = pawnCapacityDefName;
            MinValue = minValue;
            MaxValue = maxValue;
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
            Scribe_Values.Look(ref MinValue, nameof(MinValue));
            Scribe_Values.Look(ref MaxValue, nameof(MaxValue));
        }

        private void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            _pawnCapacityDef = DefDatabase<PawnCapacityDef>.GetNamedSilentFail(_pawnCapacityDefName);
        }

        public static float? Parse(ref string buffer)
        {
            if (!float.TryParse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out var limit))
            {
                return null;
            }
            var value = Mathf.Clamp(limit, -1 * ValueCap, ValueCap);
            buffer = $"{value:N2}";
            return value;
        }
    }
}