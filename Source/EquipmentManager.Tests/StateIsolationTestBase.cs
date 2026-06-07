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
        // Note: Include only types with static _equipmentManager fields. EquipmentManagerMapComponent
        // has an instance field, not static, so it's not included here.
    ];

    private readonly Dictionary<Type, object?> _snapshot = new();

    /// <summary>
    ///     Restores the snapshotted mutable static state after each test.
    ///     Fails loudly if any caching type's _equipmentManager field is missing (renamed/removed),
    ///     so isolation drift is caught rather than silently skipped.
    /// </summary>
    [TearDown]
    public void RestoreState()
    {
        foreach (var type in CachingTypes)
        {
            var field = type.GetField(EquipmentManagerFieldName, BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Expected caching type {type.Name} to have a static field '{EquipmentManagerFieldName}', " +
                    $"but it was not found. This indicates a refactoring mismatch; check that the field name " +
                    $"has not been renamed or removed from {type.FullName}.");
            }

            field.SetValue(null, _snapshot.TryGetValue(type, out var value) ? value : null);
        }
    }

    /// <summary>
    ///     Snapshots the relevant mutable static state before each test.
    ///     Fails loudly if any caching type's _equipmentManager field is missing (renamed/removed),
    ///     so isolation drift is caught rather than silently skipped.
    /// </summary>
    [SetUp]
    public void SnapshotState()
    {
        _snapshot.Clear();
        foreach (var type in CachingTypes)
        {
            var field = type.GetField(EquipmentManagerFieldName, BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Expected caching type {type.Name} to have a static field '{EquipmentManagerFieldName}', " +
                    $"but it was not found. This indicates a refactoring mismatch; check that the field name " +
                    $"has not been renamed or removed from {type.FullName}.");
            }

            _snapshot[type] = field.GetValue(null);
        }
    }
}