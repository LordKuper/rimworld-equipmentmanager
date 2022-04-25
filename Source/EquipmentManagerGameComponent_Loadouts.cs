using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace EquipmentManager
{
    internal partial class EquipmentManagerGameComponent
    {
        private List<Loadout> _loadouts;
        private List<PawnLoadout> _pawnLoadouts;

        public Loadout AddLoadout()
        {
            var id = _loadouts.Any() ? _loadouts.Max(l => l.Id) + 1 : 0;
            var loadout = new Loadout(id) {Label = $"{id}"};
            _loadouts.Add(loadout);
            return loadout;
        }

        public void AddLoadout(Loadout loadout)
        {
            var existingLoadout = _loadouts.FirstOrDefault(l => l.Id == loadout.Id);
            if (existingLoadout != null) { _ = _loadouts.Remove(existingLoadout); }
            _loadouts.Add(loadout);
        }

        public Loadout CopyLoadout(Loadout loadout)
        {
            var newLoadout = AddLoadout();
            newLoadout.Label = $"{loadout.Label} 2";
            newLoadout.Priority = loadout.Priority;
            newLoadout.PrimaryRuleType = loadout.PrimaryRuleType;
            newLoadout.PrimaryMeleeWeaponRuleId = loadout.PrimaryMeleeWeaponRuleId;
            newLoadout.PrimaryRangedWeaponRuleId = loadout.PrimaryRangedWeaponRuleId;
            newLoadout.RangedSidearmRules.AddRange(loadout.RangedSidearmRules);
            newLoadout.MeleeSidearmRules.AddRange(loadout.MeleeSidearmRules);
            newLoadout.ToolRuleId = loadout.ToolRuleId;
            newLoadout.DropUnassignedWeapons = loadout.DropUnassignedWeapons;
            foreach (var passionLimit in loadout.PassionLimits)
            {
                newLoadout.PassionLimits.Add(new PassionLimit(passionLimit.SkillDefName) {Value = passionLimit.Value});
            }
            foreach (var pawnCapacityLimit in loadout.PawnCapacityLimits)
            {
                newLoadout.PawnCapacityLimits.Add(new PawnCapacityLimit(pawnCapacityLimit.PawnCapacityDefName,
                    pawnCapacityLimit.MinValue, pawnCapacityLimit.MaxValue));
            }
            foreach (var pawnCapacityWeight in loadout.PawnCapacityWeights)
            {
                newLoadout.PawnCapacityWeights.Add(new PawnCapacityWeight(pawnCapacityWeight.PawnCapacityDefName,
                    pawnCapacityWeight.Weight));
            }
            foreach (var pawnTrait in loadout.PawnTraits) { newLoadout.PawnTraits.Add(pawnTrait.Key, pawnTrait.Value); }
            foreach (var pawnWorkCapacity in loadout.PawnWorkCapacities)
            {
                newLoadout.PawnWorkCapacities.Add(pawnWorkCapacity.Key, pawnWorkCapacity.Value);
            }
            foreach (var skillLimit in loadout.SkillLimits)
            {
                newLoadout.SkillLimits.Add(new SkillLimit(skillLimit.SkillDefName, skillLimit.MinValue,
                    skillLimit.MaxValue));
            }
            foreach (var skillWeight in loadout.SkillWeights)
            {
                newLoadout.SkillWeights.Add(new SkillWeight(skillWeight.SkillDefName, skillWeight.Weight));
            }
            foreach (var statLimit in loadout.StatLimits)
            {
                newLoadout.StatLimits.Add(new StatLimit(statLimit.StatDefName, statLimit.MinValue, statLimit.MaxValue));
            }
            foreach (var statWeight in loadout.StatWeights)
            {
                newLoadout.StatWeights.Add(new StatWeight(statWeight.StatDefName, statWeight.Weight,
                    statWeight.Protected));
            }
            return newLoadout;
        }

        public void DeleteLoadout(Loadout loadout)
        {
            foreach (var pawnLoadout in _pawnLoadouts.Where(pl => pl.LoadoutId == loadout.Id))
            {
                pawnLoadout.LoadoutId = null;
            }
            _ = _loadouts.Remove(loadout);
        }

        private void ExposeData_Loadouts()
        {
            if (Scribe.mode == LoadSaveMode.Saving) { _ = _pawnLoadouts?.RemoveAll(pl => pl.Pawn?.Destroyed ?? true); }
            Scribe_Collections.Look(ref _loadouts, "Loadouts", LookMode.Deep);
            Scribe_Collections.Look(ref _pawnLoadouts, "PawnLoadouts", LookMode.Deep);
        }

        public Loadout GetLoadout(int? id)
        {
            return id == null ? null : GetLoadouts().FirstOrDefault(loadout => loadout.Id == id);
        }

        public Loadout GetLoadout([NotNull] Pawn pawn)
        {
            if (pawn == null) { throw new ArgumentNullException(nameof(pawn)); }
            if (_pawnLoadouts == null) { _pawnLoadouts = new List<PawnLoadout>(); }
            return GetLoadout(GetPawnLoadout(pawn)?.LoadoutId);
        }

        public IEnumerable<Loadout> GetLoadouts()
        {
            return _loadouts ?? (_loadouts = new List<Loadout>(Loadout.DefaultLoadouts));
        }

        public PawnLoadout GetPawnLoadout([NotNull] Pawn pawn)
        {
            if (pawn == null) { throw new ArgumentNullException(nameof(pawn)); }
            if (_pawnLoadouts == null) { _pawnLoadouts = new List<PawnLoadout>(); }
            var pawnLoadout =
                _pawnLoadouts.FirstOrDefault(pl => pl.Pawn != null && pl.Pawn.thingIDNumber == pawn.thingIDNumber);
            if (pawnLoadout != null) { return pawnLoadout; }
            pawnLoadout = new PawnLoadout {Pawn = pawn, LoadoutId = null, Automatic = true};
            _pawnLoadouts.Add(pawnLoadout);
            return pawnLoadout;
        }

        public void SetPawnLoadout([NotNull] Pawn pawn, Loadout loadout, bool automatic)
        {
            if (pawn == null) { throw new ArgumentNullException(nameof(pawn)); }
            if (_pawnLoadouts == null) { _pawnLoadouts = new List<PawnLoadout>(); }
            var pawnLoadout =
                _pawnLoadouts.FirstOrDefault(pl => pl.Pawn != null && pl.Pawn.thingIDNumber == pawn.thingIDNumber);
            if (pawnLoadout != null)
            {
                pawnLoadout.LoadoutId = loadout?.Id;
                pawnLoadout.Automatic = automatic;
            }
            else { _pawnLoadouts.Add(new PawnLoadout {Pawn = pawn, LoadoutId = loadout?.Id, Automatic = automatic}); }
        }
    }
}