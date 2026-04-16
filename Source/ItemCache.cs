using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;

namespace EquipmentManager;

internal class ItemCache
{
    private const float UpdateTimer = 24f;
    private readonly RimworldTime _updateTime = new(-1, -1, -1);
    protected readonly Dictionary<StatDef, float> StatValues = new();

    public virtual bool Update([NotNull] RimworldTime time)
    {
        var hoursPassed = (time.Year - _updateTime.Year) * 60 * 24 +
            (time.Day - _updateTime.Day) * 24 + time.Hour - _updateTime.Hour;
        if (hoursPassed < UpdateTimer) { return false; }
        _updateTime.Year = time.Year;
        _updateTime.Day = time.Day;
        _updateTime.Hour = time.Hour;
        StatValues.Clear();
        return true;
    }
}