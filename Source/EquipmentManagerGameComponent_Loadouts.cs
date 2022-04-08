using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace EquipmentManager
{
    internal partial class EquipmentManagerGameComponent
    {
        private List<Loadout> _loadouts = new List<Loadout>(Loadout.DefaultLoadouts);
        private List<PawnLoadout> _pawnLoadouts = new List<PawnLoadout>();

        public Loadout AddLoadout()
        {
            var id = _loadouts.Any() ? _loadouts.Max(l => l.Id) + 1 : 0;
            var loadout = new Loadout(id, false) {Label = $"{id}"};
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
            newLoadout.Label = $"{loadout.Label} (copy)";
            newLoadout.Priority = loadout.Priority;
            newLoadout.PrimaryRuleType = loadout.PrimaryRuleType;
            newLoadout.PrimaryMeleeWeaponRuleId = loadout.PrimaryMeleeWeaponRuleId;
            newLoadout.PrimaryRangedWeaponRuleId = loadout.PrimaryRangedWeaponRuleId;
            newLoadout.RangedSidearmRules.AddRange(loadout.RangedSidearmRules);
            newLoadout.MeleeSidearmRules.AddRange(loadout.MeleeSidearmRules);
            newLoadout.ToolRuleId = loadout.ToolRuleId;
            newLoadout.PawnTraits.AddRange(loadout.PawnTraits);
            newLoadout.PawnCapacities.AddRange(loadout.PawnCapacities);
            newLoadout.PreferredSkills.AddRange(loadout.PreferredSkills);
            newLoadout.UndesirableSkills.AddRange(loadout.UndesirableSkills);
            newLoadout.DropUnassignedWeapons = loadout.DropUnassignedWeapons;
            return newLoadout;
        }

        public void DeleteLoadout(Loadout loadout)
        {
            foreach (var pawnLoadout in _pawnLoadouts.Where(pl => pl.LoadoutId == loadout.Id).ToList())
            {
                _ = _pawnLoadouts.Remove(pawnLoadout);
            }
            _ = _loadouts.Remove(loadout);
        }

        private void ExposeData_Loadouts()
        {
            _ = _pawnLoadouts.RemoveAll(pawnLoadout => pawnLoadout.Pawn?.Destroyed ?? true);
            Scribe_Collections.Look(ref _loadouts, "Loadouts", LookMode.Deep);
            Scribe_Collections.Look(ref _pawnLoadouts, "PawnLoadouts", LookMode.Deep);
        }

        public Loadout GetLoadout(int id)
        {
            return GetLoadouts().FirstOrDefault(loadout => loadout.Id == id);
        }

        public Loadout GetLoadout([NotNull] Pawn pawn)
        {
            return pawn == null
                ? throw new ArgumentNullException(nameof(pawn))
                : GetLoadout(GetPawnLoadout(pawn).LoadoutId);
        }

        public IEnumerable<Loadout> GetLoadouts()
        {
            if (_loadouts == null || _loadouts.Count == 0) { _loadouts = new List<Loadout>(Loadout.DefaultLoadouts); }
            return _loadouts;
        }

        public PawnLoadout GetPawnLoadout([NotNull] Pawn pawn)
        {
            if (pawn == null) { throw new ArgumentNullException(nameof(pawn)); }
            var pawnLoadout = _pawnLoadouts.FirstOrDefault(pl => pl.Pawn == pawn);
            if (pawnLoadout != null) { return pawnLoadout; }
            pawnLoadout = new PawnLoadout {Pawn = pawn, LoadoutId = 0, Automatic = true};
            _pawnLoadouts.Add(pawnLoadout);
            return pawnLoadout;
        }

        public void SetPawnLoadout([NotNull] Pawn pawn, [NotNull] Loadout loadout, bool automatic)
        {
            if (pawn == null) { throw new ArgumentNullException(nameof(pawn)); }
            if (loadout == null) { throw new ArgumentNullException(nameof(loadout)); }
            var pawnLoadout = _pawnLoadouts.FirstOrDefault(pl => pl.Pawn == pawn);
            if (pawnLoadout == null)
            {
                _pawnLoadouts.Add(new PawnLoadout {Pawn = pawn, LoadoutId = loadout.Id, Automatic = automatic});
            }
            else
            {
                pawnLoadout.LoadoutId = loadout.Id;
                pawnLoadout.Automatic = automatic;
            }
        }
    }
}