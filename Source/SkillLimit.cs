using System.Globalization;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace EquipmentManager
{
    internal class SkillLimit : IExposable
    {
        private const float ValueCap = 20f;
        private bool _isInitialized;
        private SkillDef _skillDef;
        private string _skillDefName;
        public float? MaxValue;
        public string MaxValueBuffer;
        public float? MinValue;
        public string MinValueBuffer;

        [UsedImplicitly]
        public SkillLimit() { }

        public SkillLimit(string skillDefName)
        {
            _skillDefName = skillDefName;
        }

        public SkillLimit(string skillDefName, float? minValue, float? maxValue)
        {
            _skillDefName = skillDefName;
            MinValue = minValue;
            MaxValue = maxValue;
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
            Scribe_Values.Look(ref MinValue, nameof(MinValue));
            Scribe_Values.Look(ref MaxValue, nameof(MaxValue));
        }

        private void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            _skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(_skillDefName);
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