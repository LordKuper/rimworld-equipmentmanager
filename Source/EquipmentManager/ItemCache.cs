using System.Collections.Generic;
using LordKuper.Common.Cache;
using LordKuper.Common;
using RimWorld;

namespace EquipmentManager;

internal class ItemCache : TimedCache
{
    protected readonly Dictionary<StatDef, float> StatValues = new();

    protected ItemCache() : base(24f, true) { }

    public override bool Update(RimWorldTime time)
    {
        if (!base.Update(time)) { return false; }
        StatValues.Clear();
        return true;
    }
}
