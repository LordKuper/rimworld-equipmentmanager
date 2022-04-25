using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PeteTimesSix.SimpleSidearms;
using PeteTimesSix.SimpleSidearms.Utilities;
using RimWorld;
using SimpleSidearms.rimworld;
using Verse;
using Verse.AI;

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
            var availableWeapons = rule.GetCurrentlyAvailableItems(map, workTypes, _updateTime).ToList();
            _ = availableWeapons.RemoveAll(thing => _pawnCache.Any(pc => pc.AssignedWeapons.ContainsKey(thing)));
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                .Where(thing => rule.IsAvailable(thing, workTypes, _updateTime)).ToList();
            availableWeapons.AddRange(carriedWeapons);
            foreach (var thing in availableWeapons.ToList())
            {
                var biocodable = thing.TryGetComp<CompBiocodable>();
                if (biocodable != null && biocodable.Biocoded && biocodable.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                    continue;
                }
                var bladelink = thing.TryGetComp<CompBladelinkWeapon>();
                if (bladelink != null && bladelink.Biocoded && bladelink.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                }
            }
            foreach (var weapon in availableWeapons.Where(weapon =>
                         pawn.AssignedWeapons.Keys.All(thing => thing.def != weapon.def) &&
                         (carriedWeapons.Contains(weapon) ||
                             StatCalculator.canCarrySidearmInstance((ThingWithComps) weapon, pawn.Pawn, out _))))
            {
                pawn.AssignedWeapons.Add(weapon, "tool");
                if (carriedWeapons.Contains(weapon))
                {
                    CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn).InformOfAddedSidearm(weapon);
                }
                else
                {
                    _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                        JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) weapon),
                        requestQueueing: ShouldRequestQueueing(pawn));
                }
            }
        }

        private void AssignBestTool(PawnCache pawn, ToolRule rule)
        {
            var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
            var workTypes = WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                .Where(wt => !pawn.Pawn.WorkTypeIsDisabled(wt)).ToList();
            var availableWeapons = rule.GetCurrentlyAvailableItems(map, workTypes, _updateTime).ToList();
            _ = availableWeapons.RemoveAll(thing => _pawnCache.Any(pc => pc.AssignedWeapons.ContainsKey(thing)));
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                .Where(thing => rule.IsAvailable(thing, workTypes, _updateTime)).ToList();
            availableWeapons.AddRange(carriedWeapons);
            foreach (var thing in availableWeapons.ToList())
            {
                var biocodable = thing.TryGetComp<CompBiocodable>();
                if (biocodable != null && biocodable.Biocoded && biocodable.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                    continue;
                }
                var bladelink = thing.TryGetComp<CompBladelinkWeapon>();
                if (bladelink != null && bladelink.Biocoded && bladelink.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                }
            }
            var bestWeapon = availableWeapons
                .Where(thing =>
                    carriedWeapons.Contains(thing) ||
                    StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _))
                .OrderByDescending(thing => rule.GetThingScore(thing, workTypes, _updateTime))
                .ThenByDescending(thing => sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                .ThenBy(thing => thing.GetHashCode()).FirstOrDefault();
            if (bestWeapon == null) { return; }
            if (pawn.AssignedWeapons.Keys.Any(thing => thing.def == bestWeapon.def)) { return; }
            pawn.AssignedWeapons.Add(bestWeapon, "tool");
            if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedSidearm(bestWeapon); }
            else
            {
                _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                    JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) bestWeapon),
                    requestQueueing: ShouldRequestQueueing(pawn));
            }
        }

        private void AssignPrimaryMeleeWeapon(PawnCache pawn)
        {
            var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
            sidearmMemory.primaryWeaponMode = Enums.PrimaryWeaponMode.Melee;
            if (pawn.AssignedLoadout.PrimaryMeleeWeaponRuleId == null) { return; }
            var rule = EquipmentManager.GetMeleeWeaponRule((int) pawn.AssignedLoadout.PrimaryMeleeWeaponRuleId);
            if (rule == null) { return; }
            var availableWeapons = rule.GetCurrentlyAvailableItems(map, _updateTime).ToList();
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                .Where(weapon => rule.IsAvailable(weapon, _updateTime)).ToList();
            availableWeapons.AddRange(carriedWeapons);
            _ = availableWeapons.RemoveAll(thing =>
                _pawnCache.Any(pc => pc != pawn && pc.AssignedWeapons.ContainsKey(thing)));
            foreach (var thing in availableWeapons.ToList())
            {
                var biocodable = thing.TryGetComp<CompBiocodable>();
                if (biocodable != null && biocodable.Biocoded && biocodable.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                    continue;
                }
                var bladelink = thing.TryGetComp<CompBladelinkWeapon>();
                if (bladelink != null && bladelink.Biocoded && bladelink.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                }
            }
            var bestWeapon = availableWeapons.OrderByDescending(thing => rule.GetThingScore(thing, _updateTime))
                .ThenByDescending(thing => sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                .ThenByDescending(thing => carriedWeapons.Contains(thing)).ThenBy(thing => thing.GetHashCode())
                .FirstOrDefault();
            if (bestWeapon == null) { return; }
            pawn.AssignedWeapons.Add(bestWeapon, "primary");
            if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedPrimary(bestWeapon); }
            else
            {
                _ = pawn.Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, (LocalTargetInfo) bestWeapon),
                    requestQueueing: ShouldRequestQueueing(pawn));
            }
        }

        private void AssignPrimaryRangedWeapon(PawnCache pawn)
        {
            var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
            sidearmMemory.primaryWeaponMode = Enums.PrimaryWeaponMode.Ranged;
            if (pawn.AssignedLoadout.PrimaryRangedWeaponRuleId == null) { return; }
            var rule = EquipmentManager.GetRangedWeaponRule((int) pawn.AssignedLoadout.PrimaryRangedWeaponRuleId);
            if (rule == null) { return; }
            var availableWeapons = rule.GetCurrentlyAvailableItems(map, _updateTime).ToList();
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                .Where(weapon => rule.IsAvailable(weapon, _updateTime)).ToList();
            availableWeapons.AddRange(carriedWeapons);
            _ = availableWeapons.RemoveAll(thing =>
                _pawnCache.Any(pc => pc != pawn && pc.AssignedWeapons.ContainsKey(thing)));
            foreach (var thing in availableWeapons.ToList())
            {
                var biocodable = thing.TryGetComp<CompBiocodable>();
                if (biocodable != null && biocodable.Biocoded && biocodable.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                    continue;
                }
                var bladelink = thing.TryGetComp<CompBladelinkWeapon>();
                if (bladelink != null && bladelink.Biocoded && bladelink.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                }
            }
            var weaponScores =
                availableWeapons.ToDictionary(thing => thing, thing => rule.GetThingScore(thing, _updateTime));
            var bestWeapon = availableWeapons.OrderByDescending(thing => weaponScores[thing])
                .ThenByDescending(thing => sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                .ThenByDescending(thing => carriedWeapons.Contains(thing)).ThenBy(thing => thing.GetHashCode())
                .FirstOrDefault();
            if (bestWeapon == null) { return; }
            pawn.AssignedWeapons.Add(bestWeapon, "primary");
            if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedPrimary(bestWeapon); }
            else
            {
                _ = pawn.Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, (LocalTargetInfo) bestWeapon),
                    requestQueueing: ShouldRequestQueueing(pawn));
            }
            UpdateAmmo(pawn, bestWeapon, rule);
        }

        private void AssignToolsForWorkTypes(PawnCache pawn, ToolRule rule, List<WorkTypeDef> workTypes)
        {
            var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
            var availableWeapons = rule.GetCurrentlyAvailableItems(map, workTypes, _updateTime).ToList();
            _ = availableWeapons.RemoveAll(thing =>
                _pawnCache.Any(pc => pc != pawn && pc.AssignedWeapons.ContainsKey(thing)));
            _ = availableWeapons.RemoveAll(thing =>
                !StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _));
            var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                .Where(thing => rule.IsAvailable(thing, workTypes, _updateTime)).ToList();
            availableWeapons.AddRange(carriedWeapons);
            foreach (var thing in availableWeapons.ToList())
            {
                var biocodable = thing.TryGetComp<CompBiocodable>();
                if (biocodable != null && biocodable.Biocoded && biocodable.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                    continue;
                }
                var bladelink = thing.TryGetComp<CompBladelinkWeapon>();
                if (bladelink != null && bladelink.Biocoded && bladelink.CodedPawn != pawn.Pawn)
                {
                    _ = availableWeapons.Remove(thing);
                }
            }
            foreach (var workType in workTypes)
            {
                var things = availableWeapons.Where(thing => carriedWeapons.Contains(thing) ||
                    StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _)).ToList();
                var bestWeapon = things
                    .OrderByDescending(thing => rule.GetThingScore(thing, new[] {workType}, _updateTime))
                    .ThenByDescending(thing => sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                    .ThenBy(thing => thing.GetHashCode()).FirstOrDefault();
                if (bestWeapon == null) { continue; }
                if (pawn.AssignedWeapons.Keys.Any(thing => thing.def == bestWeapon.def)) { continue; }
                pawn.AssignedWeapons.Add(bestWeapon, $"tool_{workType.labelShort}");
                if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedSidearm(bestWeapon); }
                else
                {
                    _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                        JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) bestWeapon),
                        requestQueueing: ShouldRequestQueueing(pawn));
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
            EquipmentManager.LogMessage(
                $"Updating equipment at year={_updateTime.Year}, day={_updateTime.Day}, hour={_updateTime.Hour:N1} ====================");
            UpdatePawnCache();
            UpdateLoadouts();
            UpdatePrimaryWeapons();
            UpdateRangedSidearms();
            UpdateMeleeSidearms();
            UpdateTools();
            foreach (var pawn in _pawnCache.Where(pc => pc.AssignedWeapons.Any()))
            {
                EquipmentManager.LogMessage(
                    $"Assigned weapons for {pawn.Pawn.LabelShortCap} = {string.Join(", ", pawn.AssignedWeapons.Select(pair => $"{pair.Key.LabelCap} ({pair.Value})"))}");
            }
            RemoveUnassignedWeapons();
        }

        private void RemoveUnassignedWeapons()
        {
            foreach (var pawn in _pawnCache.Where(pc =>
                         pc.ShouldUpdateEquipment && (pc.AssignedLoadout?.DropUnassignedWeapons ?? false)))
            {
                var unassignedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                    .Where(weapon => !pawn.AssignedWeapons.ContainsKey(weapon)).ToList();
                if (unassignedWeapons.Any())
                {
                    EquipmentManager.LogMessage(
                        $"Dropping {string.Join(", ", unassignedWeapons.Select(thing => thing.LabelCapNoCount))} from {pawn.Pawn.LabelShortCap}'s inventory");
                }
                foreach (var weapon in unassignedWeapons) { WeaponAssingment.dropSidearm(pawn.Pawn, weapon, true); }
                var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
                foreach (var weapon in sidearmMemory.RememberedWeapons.Where(weapon =>
                             pawn.AssignedWeapons.Keys.All(thing => thing.toThingDefStuffDefPair() != weapon)))
                {
                    sidearmMemory.ForgetSidearmMemory(weapon);
                }
            }
        }

        private static bool ShouldRequestQueueing(PawnCache pawn)
        {
            return pawn.Pawn.CurJob != null;
        }

        private void UpdateAmmo(PawnCache pawn, Thing weapon, RangedWeaponRule rule)
        {
            if (!CombatExtendedHelper.EnableAmmoSystem) { return; }
            var weaponCache = EquipmentManager.GetRangedWeaponCache(weapon, _updateTime);
            var ammoDefs = weaponCache.AmmoTypes.ToList();
            var pawnAmmo = pawn.Pawn.inventory.innerContainer.InnerListForReading
                .Where(thing => ammoDefs.Contains(thing.def)).ToList();
            var mapAmmo = ammoDefs.SelectMany(def => map?.listerThings.ThingsOfDef(def)).ToList();
            var currentAmmo = pawnAmmo.Sum(thing => thing.stackCount) + pawn.AssignedAmmo
                .Where(pair => ammoDefs.Contains(pair.Key.def)).Sum(pair => pair.Value);
            EquipmentManager.LogMessage(
                $"{pawn.Pawn.LabelShortCap}'s ammo count for {weapon.LabelCapNoCount} = {currentAmmo}");
            var targetAmmoCount = weaponCache.IsAmmo ? 5 : rule.AmmoCount;
            if (currentAmmo < targetAmmoCount)
            {
                foreach (var ammoDef in ammoDefs.OrderByDescending(def => def.BaseMarketValue))
                {
                    foreach (var ammoStack in mapAmmo.Where(thing => thing.def == ammoDef))
                    {
                        if (ammoStack.IsForbidden(pawn.Pawn)) { continue; }
                        var assignedCount = _pawnCache.Sum(pc => pc.AssignedAmmo.TryGetValue(ammoStack));
                        if (assignedCount >= ammoStack.stackCount) { continue; }
                        var countToPickup = Math.Min(ammoStack.stackCount - assignedCount,
                            targetAmmoCount - currentAmmo);
                        if (countToPickup <= 0 || !pawn.Pawn.CanReserve(ammoStack, 10, countToPickup)) { continue; }
                        pawn.AssignedAmmo.Add(ammoStack, countToPickup);
                        currentAmmo += countToPickup;
                        EquipmentManager.LogMessage(
                            $"Assigning {countToPickup} of {ammoStack.LabelShortCap} to {pawn.Pawn.LabelShortCap} (ammo count = {currentAmmo})");
                        var job = JobMaker.MakeJob(JobDefOf.TakeCountToInventory, ammoStack);
                        job.count = countToPickup;
                        _ = pawn.Pawn.jobs.TryTakeOrderedJob(job, requestQueueing: ShouldRequestQueueing(pawn));
                    }
                }
            }
            else
            {
                while (currentAmmo > targetAmmoCount)
                {
                    var ammoStack = pawnAmmo.OrderBy(thing => thing.MarketValue).First();
                    var countToDrop = Math.Min(ammoStack.stackCount, currentAmmo - targetAmmoCount);
                    if (countToDrop > 0) { pawn.Pawn.inventory.DropCount(ammoStack.def, countToDrop, unforbid: true); }
                    currentAmmo -= countToDrop;
                    EquipmentManager.LogMessage(
                        $"Dropping {countToDrop} of {ammoStack.LabelShortCap} from {pawn.Pawn.LabelShortCap}'s inventory (ammo count = {currentAmmo})");
                }
            }
        }

        private void UpdateLoadouts()
        {
            foreach (var loadout in EquipmentManager.GetLoadouts().Where(loadout => loadout.Priority > 0)
                         .OrderByDescending(loadout => loadout.PassionLimits.Count + loadout.PawnCapacityLimits.Count +
                             loadout.PawnCapacityWeights.Count + loadout.PawnTraits.Count +
                             loadout.PawnWorkCapacities.Count + loadout.SkillLimits.Count + loadout.SkillWeights.Count +
                             loadout.StatLimits.Count + loadout.StatWeights.Count)
                         .ThenByDescending(loadout => loadout.Priority))
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
                        .OrderByDescending(pc => pc.AvailableLoadouts[loadout]).ThenBy(pc => pc.Pawn.GetHashCode())
                        .FirstOrDefault();
                    if (pawn == null) { break; }
                    pawn.AssignedLoadout = loadout;
                    assignedPawnsCount++;
                }
            }
            foreach (var pawn in _pawnCache.Where(pc => pc.AutoLoadout && pc.AssignedLoadout != null))
            {
                EquipmentManager.SetPawnLoadout(pawn.Pawn, pawn.AssignedLoadout, true);
                pawn.AssignedWeapons.Clear();
                pawn.AssignedAmmo.Clear();
            }
            EquipmentManager.LogMessage(
                $"Equipment Manager: {string.Join(", ", _pawnCache.Where(pc => pc.AssignedLoadout != null).Select(pc => $"{pc.Pawn.LabelShortCap} = {pc.AssignedLoadout.Label}"))}");
        }

        private void UpdateMeleeSidearms()
        {
            foreach (var pawn in _pawnCache.Where(pc => pc.ShouldUpdateEquipment))
            {
                var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
                foreach (var rule in pawn.AssignedLoadout.MeleeSidearmRules.Select(EquipmentManager.GetMeleeWeaponRule)
                             .Where(rule => rule != null))
                {
                    var availableWeapons = rule.GetCurrentlyAvailableItems(map, _updateTime).ToList();
                    _ = availableWeapons.RemoveAll(thing =>
                        !StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _));
                    var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                        .Where(weapon => rule.IsAvailable(weapon, _updateTime)).ToList();
                    availableWeapons.AddRange(carriedWeapons);
                    _ = availableWeapons.RemoveAll(thing =>
                        _pawnCache.Any(pc => pc.AssignedWeapons.ContainsKey(thing)));
                    foreach (var thing in availableWeapons.ToList())
                    {
                        var biocodable = thing.TryGetComp<CompBiocodable>();
                        if (biocodable != null && biocodable.Biocoded && biocodable.CodedPawn != pawn.Pawn)
                        {
                            _ = availableWeapons.Remove(thing);
                            continue;
                        }
                        var bladelink = thing.TryGetComp<CompBladelinkWeapon>();
                        if (bladelink != null && bladelink.Biocoded && bladelink.CodedPawn != pawn.Pawn)
                        {
                            _ = availableWeapons.Remove(thing);
                        }
                    }
                    switch (rule.EquipMode)
                    {
                        case ItemRule.WeaponEquipMode.BestOne:
                            var bestWeapon = availableWeapons
                                .Where(thing => carriedWeapons.Contains(thing) ||
                                    StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _))
                                .OrderByDescending(thing => rule.GetThingScore(thing, _updateTime))
                                .ThenByDescending(thing =>
                                    sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                                .ThenByDescending(thing => carriedWeapons.Contains(thing))
                                .ThenBy(thing => thing.GetHashCode()).FirstOrDefault();
                            if (bestWeapon == null) { continue; }
                            if (pawn.AssignedWeapons.Keys.Any(thing => thing.def == bestWeapon.def)) { continue; }
                            pawn.AssignedWeapons.Add(bestWeapon, "melee sidearm");
                            if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedSidearm(bestWeapon); }
                            else
                            {
                                _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                                    JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) bestWeapon),
                                    requestQueueing: ShouldRequestQueueing(pawn));
                            }
                            break;
                        case ItemRule.WeaponEquipMode.AllAvailable:
                            foreach (var weapon in availableWeapons.Where(weapon =>
                                         pawn.AssignedWeapons.Keys.All(thing => thing.def != weapon.def) &&
                                         (carriedWeapons.Contains(weapon) ||
                                             StatCalculator.canCarrySidearmInstance((ThingWithComps) weapon, pawn.Pawn,
                                                 out _))))
                            {
                                pawn.AssignedWeapons.Add(weapon, "melee sidearm");
                                if (carriedWeapons.Contains(weapon)) { sidearmMemory.InformOfAddedSidearm(weapon); }
                                else
                                {
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

        private void UpdatePawnCache()
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
            EquipmentManager.LogMessage(
                $"Equipment Manager: Pawns: {string.Join("; ", _pawnCache.Select(pc => $"{pc.Pawn.LabelShortCap} ({pc.AssignedLoadout?.Label ?? "None"}, {(pc.AutoLoadout ? "auto" : "manual")}) [{(pc.ShouldUpdateEquipment ? "updating" : "not updating")}]"))}");
        }

        private void UpdatePrimaryWeapons()
        {
            foreach (var pawn in _pawnCache.Where(pc => pc.ShouldUpdateEquipment))
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
            foreach (var pawn in _pawnCache.Where(pc => pc.ShouldUpdateEquipment))
            {
                foreach (var rule in pawn.AssignedLoadout.RangedSidearmRules
                             .Select(EquipmentManager.GetRangedWeaponRule).Where(rule => rule != null))
                {
                    var sidearmMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn.Pawn);
                    var availableWeapons = rule.GetCurrentlyAvailableItems(map, _updateTime).ToList();
                    var carriedWeapons = pawn.Pawn.getCarriedWeapons(true, true)
                        .Where(weapon => rule.IsAvailable(weapon, _updateTime)).ToList();
                    availableWeapons.AddRange(carriedWeapons);
                    _ = availableWeapons.RemoveAll(thing =>
                        _pawnCache.Any(pc => pc != pawn && pc.AssignedWeapons.ContainsKey(thing)));
                    foreach (var thing in availableWeapons.ToList())
                    {
                        var biocodable = thing.TryGetComp<CompBiocodable>();
                        if (biocodable != null && biocodable.Biocoded && biocodable.CodedPawn != pawn.Pawn)
                        {
                            _ = availableWeapons.Remove(thing);
                            continue;
                        }
                        var bladelink = thing.TryGetComp<CompBladelinkWeapon>();
                        if (bladelink != null && bladelink.Biocoded && bladelink.CodedPawn != pawn.Pawn)
                        {
                            _ = availableWeapons.Remove(thing);
                        }
                    }
                    switch (rule.EquipMode)
                    {
                        case ItemRule.WeaponEquipMode.BestOne:
                            var bestWeapon = availableWeapons
                                .Where(thing => carriedWeapons.Contains(thing) ||
                                    StatCalculator.canCarrySidearmInstance((ThingWithComps) thing, pawn.Pawn, out _))
                                .OrderByDescending(thing => rule.GetThingScore(thing, _updateTime))
                                .ThenByDescending(thing =>
                                    sidearmMemory.RememberedWeapons.Contains(thing.toThingDefStuffDefPair()))
                                .ThenByDescending(thing => carriedWeapons.Contains(thing))
                                .ThenBy(thing => thing.GetHashCode()).FirstOrDefault();
                            if (bestWeapon == null ||
                                pawn.AssignedWeapons.Keys.Any(thing => thing.def == bestWeapon.def)) { continue; }
                            pawn.AssignedWeapons.Add(bestWeapon, "ranged sidearm");
                            if (carriedWeapons.Contains(bestWeapon)) { sidearmMemory.InformOfAddedSidearm(bestWeapon); }
                            else
                            {
                                _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                                    JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) bestWeapon),
                                    requestQueueing: ShouldRequestQueueing(pawn));
                            }
                            UpdateAmmo(pawn, bestWeapon, rule);
                            break;
                        case ItemRule.WeaponEquipMode.AllAvailable:
                            foreach (var weapon in availableWeapons.Where(weapon =>
                                         pawn.AssignedWeapons.Keys.All(thing => thing.def != weapon.def) &&
                                         (carriedWeapons.Contains(weapon) ||
                                             StatCalculator.canCarrySidearmInstance((ThingWithComps) weapon, pawn.Pawn,
                                                 out _))).OrderByDescending(thing =>
                                         rule.GetThingScore(thing, _updateTime)))
                            {
                                pawn.AssignedWeapons.Add(weapon, "ranged sidearm");
                                if (carriedWeapons.Contains(weapon)) { sidearmMemory.InformOfAddedSidearm(weapon); }
                                else
                                {
                                    _ = pawn.Pawn.jobs.TryTakeOrderedJob(
                                        JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, (LocalTargetInfo) weapon),
                                        requestQueueing: ShouldRequestQueueing(pawn));
                                }
                                UpdateAmmo(pawn, weapon, rule);
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
            foreach (var pawn in _pawnCache.Where(pc => pc.ShouldUpdateEquipment))
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