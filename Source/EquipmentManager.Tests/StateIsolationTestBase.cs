using System;
using System.Collections.Generic;
using System.Reflection;

namespace EquipmentManager.Tests;

/// <summary>
///     Abstract base class for tests that mutate global or static state, providing snapshot/restore isolation
///     via [SetUp] and [TearDown] to ensure test independence.
///     EquipmentManager has no public singleton; the mutable static state is the cached
///     <see cref="EquipmentManagerGameComponent" /> reference held in the private static <c>_equipmentManager</c>
///     fields of the cache and rule types.
/// </summary>
[NonParallelizable]
public abstract class StateIsolationTestBase
{
    private const string EquipmentManagerFieldName = "_equipmentManager";

    /// <summary>
    ///     The types that hold a private static cached <see cref="EquipmentManagerGameComponent" /> reference.
    /// </summary>
    private static readonly Type[] CachingTypes =
    [
        typeof(PawnCache),
        typeof(ToolCache),
        typeof(ItemRule),
        typeof(EquipmentManagerMapComponent)
    ];

    private readonly Dictionary<Type, object?> _snapshot = new();

    /// <summary>
    ///     Gets the value of a static field via reflection.
    /// </summary>
    private static object? GetStaticFieldValue(Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
        return field?.GetValue(null);
    }

    /// <summary>
    ///     Restores the snapshotted mutable static state after each test.
    /// </summary>
    [TearDown]
    public void RestoreState()
    {
        foreach (var type in CachingTypes)
        {
            var field = type.GetField(EquipmentManagerFieldName, BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(null, _snapshot.TryGetValue(type, out var value) ? value : null);
            }
        }
    }

    /// <summary>
    ///     Snapshots the relevant mutable static state before each test.
    /// </summary>
    [SetUp]
    public void SnapshotState()
    {
        _snapshot.Clear();
        foreach (var type in CachingTypes)
        {
            var field = type.GetField(EquipmentManagerFieldName, BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
            {
                _snapshot[type] = field.GetValue(null);
            }
        }
    }
}
