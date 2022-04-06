using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PeteTimesSix.SimpleSidearms;
using PeteTimesSix.SimpleSidearms.Utilities;
using RimWorld;
using SimpleSidearms.rimworld;
using Verse;

namespace EquipmentManager
{
    [UsedImplicitly]
    internal class EquipmentManagerMapComponent : MapComponent
    {
        private static EquipmentManagerGameComponent _equipmentManager;
        private readonly RimworldTime _updateTime = new RimworldTime(-1, -1, -1);
        private HashSet<Pawn> _allPawns = new HashSet<Pawn>();
        private HashSet<PawnCache> _pawnCache = new HashSet<PawnCache>();
        public EquipmentManagerMapComponent(Map map) : base(map) { }

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        private void AssignAllTools(PawnCache pawn, ToolRule rule)
        {
            var workTypes = WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                .Where(wt => !pawn.Pawn.WorkTypeIsDisabled(wt)).ToList();
            var availableWeapons = rule.GetCurrentlyAvailableItems(map, workTypes).ToList();
            _ = availableWeapons.RemoveAll(thing => _pawnCache.Any(pc => pc.AssignedWeapons.Contains(thing)));
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                .Where(thing => rule.IsAvailable(thing, workTypes)).ToList();
            availableWeapons.AddRange(carriedWeapons);
            foreach (var weapon in availableWeapons.Where(weapon =>
                         pawn.AssignedWeapons.All(thing => thing.def != weapon.def) &&
                         StatCalculator.canCarrySidearmInstance((ThingWithComps) weapon, pawn.Pawn, out _)))
            {
                _ = pawn.AssignedWeapons.Add(weapon);
                if (carriedWeapons.Contains(weapon))
                {
                    CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn).InformOfAddedSidearm(weapon);
                }
                else
                {
                    if (Prefs.DevMode)
                    {
                        Log.Message(
                            $"Equipment Manager: {pawn.AssignedLoadout.Label}, {rule.Label}: setting {weapon.LabelCap} as tool for {pawn.Pawn.LabelCap}");
                    }
                    _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                        JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) weapon),
                        requestQueueing: new[] {JobDefOf.Equip, SidearmsDefOf.EquipSecondary}.Contains(pawn.Pawn.CurJob
                            ?.def));
                }
            }
        }

        private void AssignBestTool(PawnCache pawn, ToolRule rule)
        {
            var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
            var workTypes = WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                .Where(wt => !pawn.Pawn.WorkTypeIsDisabled(wt)).ToList();
            var availableWeapons = rule.GetCurrentlyAvailableItems(map, workTypes).ToList();
            _ = availableWeapons.RemoveAll(thing => _pawnCache.Any(pc => pc.AssignedWeapons.Contains(thing)));
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                .Where(thing => rule.IsAvailable(thing, workTypes)).ToList();
            availableWeapons.AddRange(carriedWeapons);
            var bestWeapon = availableWeapons
                .Where(thing => StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _))
                .OrderByDescending(thing => rule.GetStatScore(thing, workTypes))
                .ThenByDescending(thing => sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                .ThenBy(thing => thing.GetHashCode()).FirstOrDefault();
            if (bestWeapon == null) { return; }
            if (pawn.AssignedWeapons.Any(thing => thing.def == bestWeapon.def)) { return; }
            _ = pawn.AssignedWeapons.Add(bestWeapon);
            if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedSidearm(bestWeapon); }
            else
            {
                if (Prefs.DevMode)
                {
                    Log.Message(
                        $"Equipment Manager: {pawn.AssignedLoadout.Label}, {rule.Label}: setting {bestWeapon.LabelCap} as tool for {pawn.Pawn.LabelCap}");
                }
                _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                    JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) bestWeapon),
                    requestQueueing: new[] {JobDefOf.Equip, SidearmsDefOf.EquipSecondary}.Contains(
                        pawn.Pawn.CurJob?.def));
            }
        }

        private void AssignPrimaryMeleeWeapon(PawnCache pawn)
        {
            var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
            sidearmMemory.primaryWeaponMode = Enums.PrimaryWeaponMode.Melee;
            if (pawn.AssignedLoadout.PrimaryMeleeWeaponRuleId == null) { return; }
            var rule = EquipmentManager.GetMeleeWeaponRule((int) pawn.AssignedLoadout.PrimaryMeleeWeaponRuleId);
            if (rule == null) { return; }
            var availableWeapons = rule.GetCurrentlyAvailableItems(map).ToList();
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true).Where(weapon => rule.IsAvailable(weapon))
                .ToList();
            availableWeapons.AddRange(carriedWeapons);
            _ = availableWeapons.RemoveAll(thing =>
                _pawnCache.Any(pc => pc.AssignedWeapons.Contains(thing)) ||
                !StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _));
            var bestWeapon = availableWeapons.OrderByDescending(thing => rule.GetStatScore(thing))
                .ThenByDescending(thing => sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                .ThenBy(thing => thing.GetHashCode()).FirstOrDefault();
            if (bestWeapon == null)
            {
                if (Prefs.DevMode)
                {
                    Log.Message($"Equipment Manager: No primary weapon found for {pawn.Pawn.LabelCap}");
                }
                return;
            }
            _ = pawn.AssignedWeapons.Add(bestWeapon);
            if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedPrimary(bestWeapon); }
            else
            {
                if (Prefs.DevMode)
                {
                    Log.Message(
                        $"Equipment Manager: Setting {bestWeapon.LabelCap} as primary weapon for {pawn.Pawn.LabelCap}");
                }
                _ = pawn.Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, (LocalTargetInfo) bestWeapon));
            }
        }

        private void AssignPrimaryRangedWeapon(PawnCache pawn)
        {
            var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
            sidearmMemory.primaryWeaponMode = Enums.PrimaryWeaponMode.Ranged;
            if (pawn.AssignedLoadout.PrimaryRangedWeaponRuleId == null) { return; }
            var rule = EquipmentManager.GetRangedWeaponRule((int) pawn.AssignedLoadout.PrimaryRangedWeaponRuleId);
            if (rule == null) { return; }
            var availableWeapons = rule.GetCurrentlyAvailableItems(map).ToList();
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true).Where(weapon => rule.IsAvailable(weapon))
                .ToList();
            availableWeapons.AddRange(carriedWeapons);
            _ = availableWeapons.RemoveAll(thing =>
                _pawnCache.Any(pc => pc.AssignedWeapons.Contains(thing)) ||
                !StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _));
            var bestWeapon = availableWeapons.OrderByDescending(thing => rule.GetStatScore(thing))
                .ThenByDescending(thing => sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                .ThenBy(thing => thing.GetHashCode()).FirstOrDefault();
            if (bestWeapon == null)
            {
                if (Prefs.DevMode)
                {
                    Log.Message($"Equipment Manager: No primary weapon found for {pawn.Pawn.LabelCap}");
                }
                return;
            }
            _ = pawn.AssignedWeapons.Add(bestWeapon);
            if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedPrimary(bestWeapon); }
            else
            {
                if (Prefs.DevMode)
                {
                    Log.Message(
                        $"Equipment Manager: Setting {bestWeapon.LabelCap} as primary weapon for {pawn.Pawn.LabelCap}");
                }
                _ = pawn.Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, (LocalTargetInfo) bestWeapon));
            }
        }

        private void AssignToolsForWorkTypes(PawnCache pawn, ToolRule rule, List<WorkTypeDef> workTypes)
        {
            var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
            var availableWeapons = rule.GetCurrentlyAvailableItems(map, workTypes).ToList();
            _ = availableWeapons.RemoveAll(thing => _pawnCache.Any(pc => pc.AssignedWeapons.Contains(thing)));
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                .Where(thing => rule.IsAvailable(thing, workTypes)).ToList();
            availableWeapons.AddRange(carriedWeapons);
            foreach (var workType in workTypes)
            {
                var things = availableWeapons.Where(thing =>
                {
                    var canCarrySidearmInstance =
                        StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out var err);
                    if (!canCarrySidearmInstance)
                    {
                        if (Prefs.DevMode) { Log.Message($"Equipment Manager: Can not carry {thing.LabelCap}: {err}"); }
                    }
                    return canCarrySidearmInstance;
                }).ToList();
                var bestWeapon = things.OrderByDescending(thing => rule.GetStatScore(thing, new[] {workType}))
                    .ThenByDescending(thing => sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                    .ThenBy(thing => thing.GetHashCode()).FirstOrDefault();
                if (bestWeapon == null) { continue; }
                if (pawn.AssignedWeapons.Any(thing => thing.def == bestWeapon.def)) { continue; }
                _ = pawn.AssignedWeapons.Add(bestWeapon);
                if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedSidearm(bestWeapon); }
                else
                {
                    if (Prefs.DevMode)
                    {
                        Log.Message(
                            $"Equipment Manager: {pawn.AssignedLoadout.Label}, {rule.Label}: setting {bestWeapon.LabelCap} as tool for {pawn.Pawn.LabelCap} ({workType.labelShort})");
                    }
                    _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                        JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) bestWeapon),
                        requestQueueing: new[] {JobDefOf.Equip, SidearmsDefOf.EquipSecondary}.Contains(pawn.Pawn.CurJob
                            ?.def));
                }
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (!map.IsPlayerHome) { return; }
            if (Find.TickManager.CurTimeSpeed == TimeSpeed.Paused || Find.TickManager.TicksGame % 60 != 0) { return; }
            var mapTime = RimworldTime.GetMapTime(map);
            var hoursPassed = ((mapTime.Year - _updateTime.Year) * 60 * 24) + ((mapTime.Day - _updateTime.Day) * 24) +
                mapTime.Hour - _updateTime.Hour;
            if (hoursPassed < 6f) { return; }
            _updateTime.Year = mapTime.Year;
            _updateTime.Day = mapTime.Day;
            _updateTime.Hour = mapTime.Hour;
            UpdateCache();
            UpdateLoadouts();
            UpdatePrimaryWeapons();
            UpdateRangedSidearms();
            UpdateMeleeSidearms();
            UpdateTools();
            RemoveUnassignedWeapons();
        }

        private void RemoveUnassignedWeapons()
        {
            foreach (var pawn in _pawnCache.Where(pc => pc.IsCapable))
            {
                var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true).ToList();
                foreach (var weapon in carriedWeapons.Where(weapon => !pawn.AssignedWeapons.Contains(weapon)))
                {
                    if (Prefs.DevMode)
                    {
                        Log.Message($"Equipment Manager: Dropping {weapon.LabelCap} from {pawn.Pawn.LabelCap}");
                    }
                    WeaponAssingment.dropSidearm(pawn.Pawn, weapon, true);
                }
                var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
                foreach (var weapon in sidearmMemory.RememberedWeapons.Where(weapon =>
                             pawn.AssignedWeapons.All(thing => thing.toThingDefStuffDefPair() != weapon)))
                {
                    sidearmMemory.ForgetSidearmMemory(weapon);
                }
            }
        }

        private void UpdateCache()
        {
            if (_allPawns == null) { _allPawns = new HashSet<Pawn>(); }
            _allPawns.Clear();
            _allPawns.AddRange(map.mapPawns.FreeColonistsSpawned.Where(pawn =>
                pawn.Faction == Faction.OfPlayer && !pawn.HasExtraHomeFaction() && !pawn.HasExtraMiniFaction() &&
                pawn.GuestStatus == null));
            if (_pawnCache == null) { _pawnCache = new HashSet<PawnCache>(); }
            foreach (var pawn in _pawnCache.Where(pc => !_allPawns.Contains(pc.Pawn)).ToList())
            {
                _ = _pawnCache.Remove(pawn);
            }
            foreach (var pawn in _allPawns)
            {
                var pawnCache = _pawnCache.FirstOrDefault(pc => pc.Pawn == pawn);
                if (pawnCache == null)
                {
                    pawnCache = new PawnCache(pawn);
                    _ = _pawnCache.Add(pawnCache);
                }
                pawnCache.Update(_updateTime);
            }
            if (Prefs.DevMode)
            {
                Log.Message(
                    $"Equipment Manager: Pawns: {string.Join("; ", _pawnCache.Select(pc => $"{pc.Pawn.LabelShort} ({pc.IsCapable})"))}");
            }
        }

        private void UpdateLoadouts()
        {
            foreach (var loadout in EquipmentManager.GetLoadouts().Where(loadout => loadout.Priority > 0)
                         .OrderByDescending(loadout => loadout.Priority).ThenByDescending(loadout =>
                             loadout.PreferredSkills.Count + loadout.UndesirableSkills.Count))
            {
                var availablePawns = _pawnCache.Where(pc => pc.IsAvailable(loadout)).ToList();
                var prioritySum = availablePawns.Sum(pawn => pawn.AvailableLoadouts.Keys.Sum(l => l.Priority));
                var avgPriority = (float) prioritySum / availablePawns.Count;
                var priorityShare = loadout.Priority / avgPriority;
                var targetCount = (int) Math.Ceiling(availablePawns.Count * priorityShare);
                var assignedPawnsCount = availablePawns.Count(pc => pc.AssignedLoadout == loadout);
                while (assignedPawnsCount < targetCount)
                {
                    var pawn = availablePawns.Where(pc => pc.AssignedLoadout == null && pc.AutoLoadout)
                        .OrderByDescending(pc => pc.AvailableLoadouts[loadout]).FirstOrDefault();
                    if (pawn == null) { break; }
                    pawn.AssignedLoadout = loadout;
                    assignedPawnsCount++;
                }
            }
            foreach (var pawn in _pawnCache.Where(pc => pc.AutoLoadout && pc.AssignedLoadout != null))
            {
                EquipmentManager.SetPawnLoadout(pawn.Pawn, pawn.AssignedLoadout, true);
                pawn.AssignedWeapons.Clear();
            }
        }

        private void UpdateMeleeSidearms()
        {
            foreach (var pawn in _pawnCache.Where(pc => pc.IsCapable))
            {
                var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
                foreach (var rule in pawn.AssignedLoadout.MeleeSidearmRules.Select(EquipmentManager.GetMeleeWeaponRule)
                             .Where(rule => rule != null))
                {
                    var availableWeapons = rule.GetCurrentlyAvailableItems(map).ToList();
                    _ = availableWeapons.RemoveAll(thing => _pawnCache.Any(pc => pc.AssignedWeapons.Contains(thing)));
                    var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                        .Where(weapon => rule.IsAvailable(weapon)).ToList();
                    availableWeapons.AddRange(carriedWeapons);
                    switch (rule.EquipMode)
                    {
                        case ItemRule.WeaponEquipMode.BestOne:
                            var bestWeapon = availableWeapons
                                .Where(thing =>
                                    StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _))
                                .OrderByDescending(thing => rule.GetStatScore(thing))
                                .ThenByDescending(thing =>
                                    sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                                .ThenBy(thing => thing.GetHashCode()).FirstOrDefault();
                            if (bestWeapon == null)
                            {
                                if (Prefs.DevMode)
                                {
                                    Log.Message(
                                        $"Equipment Manager: {pawn.AssignedLoadout.Label}, {rule.Label}: no melee sidearm found for {pawn.Pawn.LabelCap}");
                                }
                                continue;
                            }
                            if (pawn.AssignedWeapons.Any(thing => thing.def == bestWeapon.def)) { continue; }
                            _ = pawn.AssignedWeapons.Add(bestWeapon);
                            if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedSidearm(bestWeapon); }
                            else
                            {
                                if (Prefs.DevMode)
                                {
                                    Log.Message(
                                        $"Equipment Manager: {pawn.AssignedLoadout.Label}, {rule.Label}: setting {bestWeapon.LabelCap} as melee sidearm for {pawn.Pawn.LabelCap}");
                                }
                                _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                                    JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) bestWeapon),
                                    requestQueueing: new[] {JobDefOf.Equip, SidearmsDefOf.EquipSecondary}.Contains(
                                        pawn.Pawn.CurJob?.def));
                            }
                            break;
                        case ItemRule.WeaponEquipMode.AllAvailable:
                            foreach (var weapon in availableWeapons.Where(weapon =>
                                         pawn.AssignedWeapons.All(thing => thing.def != weapon.def) &&
                                         StatCalculator.canCarrySidearmInstance((ThingWithComps) weapon, pawn.Pawn,
                                             out _)))
                            {
                                _ = pawn.AssignedWeapons.Add(weapon);
                                if (carriedWeapons.Contains(weapon)) { sidearmMemory.InformOfAddedSidearm(weapon); }
                                else
                                {
                                    if (Prefs.DevMode)
                                    {
                                        Log.Message(
                                            $"Equipment Manager: {pawn.AssignedLoadout.Label}, {rule.Label}: setting {weapon.LabelCap} as melee sidearm for {pawn.Pawn.LabelCap}");
                                    }
                                    _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                                        JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) weapon),
                                        requestQueueing: new[] {JobDefOf.Equip, SidearmsDefOf.EquipSecondary}.Contains(
                                            pawn.Pawn.CurJob?.def));
                                }
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void UpdatePrimaryWeapons()
        {
            foreach (var pawn in _pawnCache.Where(pc => pc.IsCapable))
            {
                switch (pawn.AssignedLoadout.PrimaryRuleType)
                {
                    case Loadout.PrimaryWeaponType.None:
                        break;
                    case Loadout.PrimaryWeaponType.RangedWeapon:
                        AssignPrimaryRangedWeapon(pawn);
                        break;
                    case Loadout.PrimaryWeaponType.MeleeWeapon:
                        AssignPrimaryMeleeWeapon(pawn);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void UpdateRangedSidearms()
        {
            foreach (var pawn in _pawnCache.Where(pc => pc.IsCapable))
            {
                foreach (var rule in pawn.AssignedLoadout.RangedSidearmRules
                             .Select(EquipmentManager.GetRangedWeaponRule).Where(rule => rule != null))
                {
                    var availableWeapons = rule.GetCurrentlyAvailableItems(map).ToList();
                    _ = availableWeapons.RemoveAll(thing => _pawnCache.Any(pc => pc.AssignedWeapons.Contains(thing)));
                    var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                        .Where(weapon => rule.IsAvailable(weapon)).ToList();
                    availableWeapons.AddRange(carriedWeapons);
                    switch (rule.EquipMode)
                    {
                        case ItemRule.WeaponEquipMode.BestOne:
                            var bestWeapon = availableWeapons
                                .Where(thing =>
                                    StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _))
                                .OrderByDescending(thing => rule.GetStatScore(thing)).FirstOrDefault();
                            if (bestWeapon == null || pawn.AssignedWeapons.Any(thing => thing.def == bestWeapon.def))
                            {
                                continue;
                            }
                            _ = pawn.AssignedWeapons.Add(bestWeapon);
                            if (carriedWeapons.Contains(bestWeapon))
                            {
                                CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn).InformOfAddedSidearm(bestWeapon);
                            }
                            else
                            {
                                if (Prefs.DevMode)
                                {
                                    Log.Message(
                                        $"Equipment Manager: {pawn.AssignedLoadout.Label}, {rule.Label}: setting {bestWeapon.LabelCap} as ranged sidearm for {pawn.Pawn.LabelCap}");
                                }
                                _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                                    JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) bestWeapon),
                                    requestQueueing: new[] {JobDefOf.Equip, SidearmsDefOf.EquipSecondary}.Contains(
                                        pawn.Pawn.CurJob?.def));
                            }
                            break;
                        case ItemRule.WeaponEquipMode.AllAvailable:
                            foreach (var weapon in availableWeapons
                                         .Where(weapon =>
                                             pawn.AssignedWeapons.All(thing => thing.def != weapon.def) &&
                                             StatCalculator.canCarrySidearmInstance((ThingWithComps) weapon, pawn.Pawn,
                                                 out _)).OrderByDescending(thing => rule.GetStatScore(thing)))
                            {
                                _ = pawn.AssignedWeapons.Add(weapon);
                                if (carriedWeapons.Contains(weapon))
                                {
                                    CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn).InformOfAddedSidearm(weapon);
                                }
                                else
                                {
                                    if (Prefs.DevMode)
                                    {
                                        Log.Message(
                                            $"Equipment Manager: {pawn.AssignedLoadout.Label}, {rule.Label}: setting {weapon.LabelCap} as ranged sidearm for {pawn.Pawn.LabelCap}");
                                    }
                                    _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                                        JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) weapon),
                                        requestQueueing: new[] {JobDefOf.Equip, SidearmsDefOf.EquipSecondary}.Contains(
                                            pawn.Pawn.CurJob?.def));
                                }
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void UpdateTools()
        {
            foreach (var pawn in _pawnCache.Where(pc => pc.IsCapable))
            {
                if (pawn.AssignedLoadout.ToolRuleId == null) { continue; }
                var rule = EquipmentManager.GetToolRule((int) pawn.AssignedLoadout.ToolRuleId);
                if (rule == null) { continue; }
                switch (rule.EquipMode)
                {
                    case ItemRule.ToolEquipMode.OneForEveryWorkType:
                        AssignToolsForWorkTypes(pawn, rule,
                            WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                                .Where(wt => wt.visible && !pawn.Pawn.WorkTypeIsDisabled(wt)).ToList());
                        break;
                    case ItemRule.ToolEquipMode.OneForEveryAssignedWorkType:
                        AssignToolsForWorkTypes(pawn, rule,
                            WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                                .Where(wt => wt.visible && pawn.Pawn.workSettings.WorkIsActive(wt)).ToList());
                        break;
                    case ItemRule.ToolEquipMode.BestOne:
                        AssignBestTool(pawn, rule);
                        break;
                    case ItemRule.ToolEquipMode.AllAvailable:
                        AssignAllTools(pawn, rule);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}