using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class PawnCache
    {
        private static EquipmentManagerGameComponent _equipmentManager;
        private readonly RimworldTime _updateTime = new RimworldTime(-1, -1, -1);
        public readonly HashSet<Thing> AssignedWeapons = new HashSet<Thing>();
        public Loadout AssignedLoadout;
        public bool AutoLoadout;
        public bool IsCapable;

        public PawnCache(Pawn pawn)
        {
            Pawn = pawn;
        }

        public Dictionary<Loadout, float> AvailableLoadouts { get; } = new Dictionary<Loadout, float>();

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        public Pawn Pawn { get; }

        public bool IsAvailable(Loadout loadout)
        {
            return AvailableLoadouts.ContainsKey(loadout);
        }

        public void Update(RimworldTime time)
        {
            var hoursPassed = ((time.Year - _updateTime.Year) * 60 * 24) + ((time.Day - _updateTime.Day) * 24) +
                time.Hour - _updateTime.Hour;
            _updateTime.Year = time.Year;
            _updateTime.Day = time.Day;
            _updateTime.Hour = time.Hour;
            if (hoursPassed > 24f)
            {
                AvailableLoadouts.Clear();
                foreach (var loadout in EquipmentManager.GetLoadouts())
                {
                    if (loadout.IsAvailable(Pawn)) { AvailableLoadouts.Add(loadout, loadout.GetScore(Pawn)); }
                }
            }
            IsCapable = !Pawn.Dead && !Pawn.Downed && !Pawn.InMentalState && !Pawn.InContainerEnclosed &&
                !Pawn.Drafted && !HealthAIUtility.ShouldSeekMedicalRest(Pawn);
            var pawnLoadout = EquipmentManager.GetPawnLoadout(Pawn);
            AutoLoadout = pawnLoadout.Automatic;
            if (!AutoLoadout)
            {
                AssignedLoadout = EquipmentManager.GetLoadout(pawnLoadout.LoadoutId) ??
                    EquipmentManager.GetLoadouts().First();
            }
        }
    }
}