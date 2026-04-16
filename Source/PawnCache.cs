using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager;

internal class PawnCache
{
    private static EquipmentManagerGameComponent _equipmentManager;
    private readonly RimworldTime _updateTime = new(-1, -1, -1);
    public readonly Dictionary<Thing, int> AssignedAmmo = new();
    public readonly Dictionary<Thing, string> AssignedWeapons = new();
    public Loadout AssignedLoadout;
    public bool AutoLoadout;
    public bool ShouldUpdateEquipment;

    public PawnCache(Pawn pawn)
    {
        Pawn = pawn;
    }

    public Dictionary<Loadout, float> AvailableLoadouts { get; } = new();

    private static EquipmentManagerGameComponent EquipmentManager =>
        _equipmentManager ??= Current.Game.GetComponent<EquipmentManagerGameComponent>();

    public Pawn Pawn { get; }

    public bool IsAvailable([NotNull] Loadout loadout)
    {
        return AvailableLoadouts.ContainsKey(loadout);
    }

    public void Update([NotNull] RimworldTime time)
    {
        var capable = !Pawn.Dead && !Pawn.Downed && !Pawn.InMentalState &&
            !Pawn.InContainerEnclosed && !Pawn.Drafted &&
            !HealthAIUtility.ShouldSeekMedicalRest(Pawn);
        var pawnLoadout = EquipmentManager.GetPawnLoadout(Pawn);
        AutoLoadout = pawnLoadout.Automatic;
        AssignedLoadout = AutoLoadout ? null : EquipmentManager.GetLoadout(pawnLoadout.LoadoutId);
        var hoursPassed = (time.Year - _updateTime.Year) * 60 * 24 +
            (time.Day - _updateTime.Day) * 24 + time.Hour - _updateTime.Hour;
        ShouldUpdateEquipment = capable && hoursPassed > 6f;
        if (!ShouldUpdateEquipment) { return; }
        _updateTime.Year = time.Year;
        _updateTime.Day = time.Day;
        _updateTime.Hour = time.Hour;
        AvailableLoadouts.Clear();
        foreach (var loadout in EquipmentManager.GetLoadouts())
        {
            if (loadout.IsAvailable(Pawn))
            {
                AvailableLoadouts.Add(loadout, loadout.GetScore(Pawn));
            }
        }
    }
}