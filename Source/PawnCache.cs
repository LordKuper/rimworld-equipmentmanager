using System.Collections.Generic;
using JetBrains.Annotations;
using LordKuper.Common;
using LordKuper.Common.Cache;
using RimWorld;
using Verse;

namespace EquipmentManager;

internal class PawnCache : TimedCache
{
    private static EquipmentManagerGameComponent _equipmentManager;
    public readonly Dictionary<Thing, int> AssignedAmmo = new();
    public readonly Dictionary<Thing, string> AssignedWeapons = new();
    public Loadout AssignedLoadout;
    public bool AutoLoadout;
    public bool ShouldUpdateEquipment;

    public PawnCache(Pawn pawn) : base(6f, true)
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

    public override bool Update(RimWorldTime time)
    {
        var capable = !Pawn.Dead && !Pawn.Downed && !Pawn.InMentalState &&
            !Pawn.InContainerEnclosed && !Pawn.Drafted &&
            !HealthAIUtility.ShouldSeekMedicalRest(Pawn);
        var pawnLoadout = EquipmentManager.GetPawnLoadout(Pawn);
        AutoLoadout = pawnLoadout.Automatic;
        AssignedLoadout = AutoLoadout ? null : EquipmentManager.GetLoadout(pawnLoadout.LoadoutId);
        ShouldUpdateEquipment = capable && base.Update(time);
        if (!ShouldUpdateEquipment) { return false; }
        AvailableLoadouts.Clear();
        foreach (var loadout in EquipmentManager.GetLoadouts())
        {
            if (loadout.IsAvailable(Pawn))
            {
                AvailableLoadouts.Add(loadout, loadout.GetScore(Pawn));
            }
        }
        return true;
    }
}
