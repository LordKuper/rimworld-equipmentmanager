using System;
using System.Collections.Generic;
using System.Linq;
using LordKuper.Common;
using LordKuper.Common.Filters.Limits;
using RimWorld;
using Verse;

namespace EquipmentManager;

internal class ItemRule : IExposable
{
    protected static readonly SimpleCurve HitPointsCurve =
    [
        new CurvePoint(0f, 0f),
        new CurvePoint(0.25f, 0.1f),
        new CurvePoint(0.5f, 0.25f),
        new CurvePoint(0.75f, 1f)
    ];

    private static EquipmentManagerGameComponent? _equipmentManager;
    // Scribe_Collections.Look sets these to null when no saved data exists; ??= in Initialize()
    // restores them to empty before any first access.
    protected HashSet<string>? BlacklistedItemsDefNames = [];
    protected HashSet<ThingDef>? GloballyAvailableItems = [];

    // Label is populated by Scribe on load (IExposable lifecycle); = null! asserts the field
    // is always set before any read, consistent with the RimWorld load contract.
    public string Label = null!;
    protected List<StatLimit>? StatLimits = [];
    protected List<StatWeight>? StatWeights = [];
    protected HashSet<string>? WhitelistedItemsDefNames = [];
    // Scribe does not serialize _blacklistedItems/_whitelistedItems directly; they are rebuilt
    // from the DefName sets in UpdateExclusiveItems(). Still need nullable guards post-Scribe.
    private HashSet<ThingDef>? _blacklistedItems = [];
    private int _id;
    private bool _initialized;
    private bool _protected;
    private HashSet<ThingDef>? _whitelistedItems = [];

    protected ItemRule(int id, string label, bool isProtected, List<StatWeight> statWeights, List<StatLimit> statLimits,
        HashSet<string> whitelistedItemsDefNames, HashSet<string> blacklistedItemsDefNames)
    {
        _id = id;
        Label = label;
        _protected = isProtected;
        StatWeights = statWeights;
        StatLimits = statLimits;
        WhitelistedItemsDefNames = whitelistedItemsDefNames;
        BlacklistedItemsDefNames = blacklistedItemsDefNames;
    }

    protected ItemRule() { }

    protected ItemRule(int id, bool isProtected)
    {
        _id = id;
        _protected = isProtected;
    }

    protected static EquipmentManagerGameComponent EquipmentManager =>
        _equipmentManager ??= Current.Game.GetComponent<EquipmentManagerGameComponent>();

    public int Id => _id;
    public bool Protected => _protected;

    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref _id, nameof(Id));
        // Use a nullable temp so Scribe can write null on new-game; restore non-null after.
        var label = Label;
        Scribe_Values.Look(ref label, nameof(Label));
        Label = label ?? Label;
        Scribe_Values.Look(ref _protected, nameof(Protected));
        Scribe_Collections.Look(ref StatWeights, nameof(StatWeights), LookMode.Deep);
        Scribe_Collections.Look(ref StatLimits, nameof(StatLimits), LookMode.Deep);
        Scribe_Collections.Look(ref WhitelistedItemsDefNames, nameof(WhitelistedItemsDefNames), LookMode.Value);
        Scribe_Collections.Look(ref BlacklistedItemsDefNames, nameof(BlacklistedItemsDefNames), LookMode.Value);
    }

    public void AddBlacklistedItem(ThingDef thingDef)
    {
        if (thingDef == null) { throw new ArgumentNullException(nameof(thingDef)); }
        Initialize();
        if (!BlacklistedItemsDefNames!.Add(thingDef.defName)) { return; }
        _ = WhitelistedItemsDefNames!.Remove(thingDef.defName);
        UpdateExclusiveItems();
    }

    public void AddWhitelistedItem(ThingDef thingDef)
    {
        if (thingDef == null) { throw new ArgumentNullException(nameof(thingDef)); }
        Initialize();
        if (!WhitelistedItemsDefNames!.Add(thingDef.defName)) { return; }
        _ = BlacklistedItemsDefNames!.Remove(thingDef.defName);
        UpdateExclusiveItems();
    }

    public void DeleteBlacklistedItem(string defName)
    {
        Initialize();
        _ = BlacklistedItemsDefNames!.Remove(defName);
        UpdateExclusiveItems();
    }

    public void DeleteStatLimit(string statDefName)
    {
        Initialize();
        _ = StatLimits!.RemoveAll(limit => limit.StatDefName == statDefName);
    }

    public void DeleteStatWeight(string statDefName)
    {
        Initialize();
        _ = StatWeights!.RemoveAll(weight => weight.StatDefName == statDefName);
    }

    public void DeleteWhitelistedItem(string defName)
    {
        Initialize();
        _ = WhitelistedItemsDefNames!.Remove(defName);
        UpdateExclusiveItems();
    }

    public IReadOnlyCollection<ThingDef> GetBlacklistedItems()
    {
        Initialize();
        return _blacklistedItems!;
    }

    /// <summary>
    ///     Returns the base set of default stat weights applied to all item rules.
    ///     When Combat Extended is active the Bulk stat is included; otherwise it is omitted.
    ///     Subclasses override this method to prepend their own stat weights and call
    ///     <c>base.GetDefaultStatWeights()</c> to include the base set.
    /// </summary>
    protected internal virtual IEnumerable<StatWeight> GetDefaultStatWeights()
    {
        return CombatExtendedHelper.CombatExtended
            ? new[]
            {
                new StatWeight("Mass", -0.1f, false), new StatWeight("Bulk", -0.1f, false),
                new StatWeight("MarketValue", 0.1f, false)
            }
            : new[] { new StatWeight("Mass", -0.1f, false), new StatWeight("MarketValue", 0.1f, false) };
    }

    public IReadOnlyList<StatLimit> GetStatLimits()
    {
        Initialize();
        return StatLimits!;
    }

    public IReadOnlyList<StatWeight> GetStatWeights()
    {
        Initialize();
        return StatWeights!;
    }

    public IReadOnlyCollection<ThingDef> GetWhitelistedItems()
    {
        Initialize();
        return _whitelistedItems!;
    }

    protected void Initialize()
    {
        if (_initialized) { return; }
        _initialized = true;
        StatWeights ??= [];
        StatLimits ??= [];
        NormalizeLegacyCustomStatDefNames();
        _whitelistedItems ??= [];
        WhitelistedItemsDefNames ??= [];
        _blacklistedItems ??= [];
        BlacklistedItemsDefNames ??= [];
        GloballyAvailableItems ??= [];
        UpdateExclusiveItems();
    }

    internal void NormalizeLegacyCustomStatDefNames()
    {
        StatWeights = StatWeights?.Select(LegacyCustomStatDefs.NormalizeStatWeight).ToList() ?? [];
        StatLimits = StatLimits?.Select(LegacyCustomStatDefs.NormalizeStatLimit).ToList() ?? [];
    }

    protected static void ResetEquipmentManagerCache()
    {
        _equipmentManager = null;
    }

    public void SetStatLimit(StatDef statDef, float? min, float? max)
    {
        if (statDef == null) { throw new ArgumentNullException(nameof(statDef)); }
        Initialize();
        var statLimit = StatLimits!.FirstOrDefault(limit => limit.StatDef == statDef);
        if (statLimit == null)
        {
            statLimit = new StatLimit(statDef.defName);
            StatLimits!.Add(statLimit);
        }
        statLimit.MinValue = min;
        statLimit.MinValueBuffer = min.ToString();
        statLimit.MaxValue = max;
        statLimit.MaxValueBuffer = max.ToString();
    }

    public void SetStatWeight(StatDef statDef, float weight, bool isProtected)
    {
        if (statDef == null) { throw new ArgumentNullException(nameof(statDef)); }
        Initialize();
        var statWeight = StatWeights!.FirstOrDefault(sw => sw.StatDef == statDef);
        if (statWeight == null)
        {
            statWeight = new StatWeight(statDef.defName, 0f, isProtected);
            StatWeights!.Add(statWeight);
        }
        statWeight.Weight = weight;
    }

    private void UpdateExclusiveItems()
    {
        _whitelistedItems!.Clear();
        foreach (var def in WhitelistedItemsDefNames!.Select(DefDatabase<ThingDef>.GetNamedSilentFail)
                     .Where(def => def != null)) { _ = _whitelistedItems!.Add(def); }
        _blacklistedItems!.Clear();
        foreach (var def in BlacklistedItemsDefNames!.Select(DefDatabase<ThingDef>.GetNamedSilentFail)
                     .Where(def => def != null)) { _ = _blacklistedItems!.Add(def); }
    }

    internal enum ToolEquipMode
    {
        OneForEveryWorkType,
        OneForEveryAssignedWorkType,
        BestOne,
        AllAvailable
    }

    internal enum WeaponEquipMode
    {
        BestOne,
        AllAvailable
    }
}