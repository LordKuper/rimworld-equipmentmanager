using System;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class RangedWeaponCache : ItemCache
    {
        public RangedWeaponCache([NotNull] Thing thing)
        {
            Thing = thing ?? throw new ArgumentNullException(nameof(thing));
        }

        private float AccuracyClose { get; set; }
        private float AccuracyLong { get; set; }
        private float AccuracyMedium { get; set; }
        private float AccuracyShort { get; set; }
        public float ArmorPenetration { get; private set; }
        public int BurstShotCount { get; private set; }
        private float Cooldown { get; set; }
        public float Damage { get; private set; }
        private float Dps { get; set; }
        public float Dpsa { get; private set; }
        public float DpsaClose { get; private set; }
        public float DpsaLong { get; private set; }
        public float DpsaMedium { get; private set; }
        public float DpsaShort { get; private set; }
        public float MaxRange { get; private set; }
        private float MinRange { get; set; }
        public float StoppingPower { get; private set; }
        private Thing Thing { get; }
        public int TicksBetweenBurstShots { get; private set; }
        public float Warmup { get; private set; }

        public override void Update(RimworldTime time)
        {
            base.Update(time);
            var hoursPassed = ((time.Year - UpdateTime.Year) * 60 * 24) + ((time.Day - UpdateTime.Day) * 24) +
                time.Hour - UpdateTime.Hour;
            if (hoursPassed < UpdateTimer) { return; }
            UpdateTime.Year = time.Year;
            UpdateTime.Day = time.Day;
            UpdateTime.Hour = time.Hour;
            try
            {
                if (Thing.def?.Verbs != null)
                {
                    var verb = Thing.def.Verbs.FirstOrDefault(vp => vp.range > 0);
                    if (verb == null)
                    {
                        Log.Warning(
                            $"Equipment Manager: Could not find correct ranged weapon verb on the first try for weapon '{Thing.LabelCap}' ({Thing.def.defName})");
                        verb = Thing.def.Verbs.FirstOrDefault();
                    }
                    if (verb == null)
                    {
                        Log.Error(
                            $"Equipment Manager: Could not find ranged weapon verb for weapon '{Thing.LabelCap}' ({Thing.def.defName})");
                        return;
                    }
                    Warmup = verb.warmupTime;
                    MaxRange = verb.range;
                    MinRange = verb.minRange;
                    Damage = verb.defaultProjectile?.projectile?.GetDamageAmount(Thing) ?? 0f;
                    ArmorPenetration = verb.defaultProjectile?.projectile?.GetArmorPenetration(Thing) ?? 0f;
                    BurstShotCount = verb.burstShotCount <= 0 ? 1 : verb.burstShotCount;
                    TicksBetweenBurstShots = verb.ticksBetweenBurstShots <= 0 ? 10 : verb.ticksBetweenBurstShots;
                    StoppingPower = verb.defaultProjectile?.projectile?.StoppingPower ?? 0f;
                    Cooldown = Thing.GetStatValue(StatDefOf.RangedWeapon_Cooldown);
                    Dps = (float) Math.Round(
                        Damage * (double) BurstShotCount / ((((Cooldown + (double) Warmup) * 60f) +
                            (BurstShotCount * TicksBetweenBurstShots)) / 60f), 2);
                }
                if (MinRange <= 3f && MaxRange >= 3f)
                {
                    AccuracyClose = (float) Math.Round(Thing.GetStatValue(StatDefOf.AccuracyTouch) * 100f, 2);
                }
                if (MinRange <= 12f && MaxRange >= 12f)
                {
                    AccuracyShort = (float) Math.Round(Thing.GetStatValue(StatDefOf.AccuracyShort) * 100f, 2);
                }
                if (MinRange <= 25f && MaxRange >= 25f)
                {
                    AccuracyMedium = (float) Math.Round(Thing.GetStatValue(StatDefOf.AccuracyMedium) * 100f, 2);
                }
                if (MinRange <= 40f && MaxRange >= 40f)
                {
                    AccuracyLong = (float) Math.Round(Thing.GetStatValue(StatDefOf.AccuracyLong) * 100f, 2);
                }
                var totalAccuracy = 0f;
                var rangeCount = 0;
                if (AccuracyClose > 0f)
                {
                    DpsaClose = Dps * AccuracyClose / 100f;
                    totalAccuracy += AccuracyClose;
                    rangeCount++;
                }
                if (AccuracyShort > 0f)
                {
                    DpsaShort = Dps * AccuracyShort / 100f;
                    totalAccuracy += AccuracyShort;
                    rangeCount++;
                }
                if (AccuracyMedium > 0f)
                {
                    DpsaMedium = Dps * AccuracyMedium / 100f;
                    totalAccuracy += AccuracyMedium;
                    rangeCount++;
                }
                if (AccuracyLong > 0f)
                {
                    DpsaLong = Dps * AccuracyLong / 100f;
                    totalAccuracy += AccuracyLong;
                    rangeCount++;
                }
                Dpsa = rangeCount == 0 ? 0f : Dps * (totalAccuracy / rangeCount) / 100f;
            }
            catch (Exception exception)
            {
                Log.Error(
                    $"Equipment Manager: Could not update cache of '{Thing.LabelCap}' ({Thing.def?.defName}): {exception.Message}");
            }
        }
    }
}