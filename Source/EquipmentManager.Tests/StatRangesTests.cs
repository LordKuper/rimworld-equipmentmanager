using LordKuper.Common;
using RimWorld;

namespace EquipmentManager.Tests;

/// <summary>
///     Tests for the consumed <see cref="StatRanges" /> behavior,
///     verifying the first-sample seeding correctness: a first observed value must
///     seed a degenerate range [v, v], not [0, v].
///     StatRanges is a process-global static cache that normalizes values to [0,1]
///     based on accumulated min/max ranges. This test uses a mock StatDef to verify
///     the core logic without requiring full game context.
/// </summary>
[TestFixture, NonParallelizable]
public class StatRangesTests
{
    [SetUp]
    public void SetUp()
    {
        StatRanges.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        StatRanges.Clear();
    }

    private static StatDef GetTestStat()
    {
        // Create a dummy StatDef for testing. In a real game context, this would come
        // from DefDatabase. For this test, we create a minimal valid instance.
        return new StatDef { defName = "TestStat" };
    }

    /// <summary>
    ///     Tests that the first observed value v yields a range [v, v],
    ///     asserting that first sample produces a degenerate self-range, not [0, v].
    ///     This test verifies that the range is seeded to [v, v] (not [0, v]).
    ///     The NormalizeValue function returns 0 for a degenerate range (range.max == range.min).
    /// </summary>
    [Test]
    public void NormalizeStatValue_FirstValue_SeededToSelfRange()
    {
        var stat = GetTestStat();
        const float firstValue = 3.5f;

        // First observation: normalize the value.
        // Since it's the first value, the range [min, max] should be [3.5, 3.5] (not [0, 3.5]).
        // MathHelper.NormalizeValue returns 0f when the range is degenerate (max - min < 0.001).
        var normalized = StatRanges.NormalizeStatValue(stat, firstValue);

        // With a degenerate range [v, v], NormalizeValue returns 0 (seeded to [v,v], not [0,v]).
        normalized.Should().Be(0.0f, "first value in degenerate range [v,v] returns 0 via NormalizeValue");
    }

    /// <summary>
    ///     Tests that a second observation expands the range correctly.
    ///     First value 3.5 → range [3.5, 3.5], normalized to 0.
    ///     Second value 5.0 → range expands to [3.5, 5.0].
    ///     Original 3.5 should now normalize to (3.5 - 3.5) / (5.0 - 3.5) = 0 / 1.5 = 0.
    ///     New 5.0 should normalize to (5.0 - 3.5) / (5.0 - 3.5) = 1.5 / 1.5 = 1.
    /// </summary>
    [Test]
    public void NormalizeStatValue_SecondValue_ExpandsRangeAndAdapts()
    {
        var stat = GetTestStat();
        const float firstValue = 3.5f;
        const float secondValue = 5.0f;

        // First observation: degenerate range [3.5, 3.5] → 0.
        var firstNormalized = StatRanges.NormalizeStatValue(stat, firstValue);
        firstNormalized.Should().Be(0.0f);

        // Second observation: higher value expands the range to [3.5, 5.0].
        var secondNormalized = StatRanges.NormalizeStatValue(stat, secondValue);

        // Range is now [3.5, 5.0]. Second value 5.0 normalizes to (5.0 - 3.5) / (5.0 - 3.5) = 1.0.
        secondNormalized.Should().Be(1.0f, "max value in expanded range [3.5, 5.0] normalizes to 1.0");

        // Re-observing the first value with the expanded range should yield 0.
        var firstReObserved = StatRanges.NormalizeStatValue(stat, firstValue);
        firstReObserved.Should().Be(0.0f, "min value in expanded range [3.5, 5.0] normalizes to 0.0");
    }

    /// <summary>
    ///     Tests that observing a value below the current min expands the min.
    /// </summary>
    [Test]
    public void NormalizeStatValue_BelowMin_ExpandsMinBoundary()
    {
        var stat = GetTestStat();

        // Start with 5.0 → range [5.0, 5.0] → 0.
        StatRanges.NormalizeStatValue(stat, 5.0f);

        // Expand up to [5.0, 7.0].
        StatRanges.NormalizeStatValue(stat, 7.0f);

        // Expand down: observe 2.0 → range becomes [2.0, 7.0].
        var normalized = StatRanges.NormalizeStatValue(stat, 2.0f);

        // 2.0 is the new min in [2.0, 7.0], normalizes to (2.0 - 2.0) / (7.0 - 2.0) = 0.
        normalized.Should().Be(0.0f);

        // Original 5.0 should now normalize to (5.0 - 2.0) / (7.0 - 2.0) = 3.0 / 5.0 = 0.6.
        var reObserved = StatRanges.NormalizeStatValue(stat, 5.0f);
        reObserved.Should().Be(0.6f);
    }

    /// <summary>
    ///     Tests that multiple different stats maintain independent ranges.
    /// </summary>
    [Test]
    public void NormalizeStatValue_MultipleStats_IndependentRanges()
    {
        var massStat = new StatDef { defName = "Mass" };
        var valueStat = new StatDef { defName = "MarketValue" };

        // Establish range for Mass: [2.0, 4.0].
        StatRanges.NormalizeStatValue(massStat, 2.0f);
        StatRanges.NormalizeStatValue(massStat, 4.0f);

        // Establish range for MarketValue: [100, 200].
        StatRanges.NormalizeStatValue(valueStat, 100f);
        StatRanges.NormalizeStatValue(valueStat, 200f);

        // Mass 3.0 should normalize to (3.0 - 2.0) / (4.0 - 2.0) = 1.0 / 2.0 = 0.5.
        var normalizedMass = StatRanges.NormalizeStatValue(massStat, 3.0f);
        normalizedMass.Should().Be(0.5f);

        // MarketValue 150 should normalize to (150 - 100) / (200 - 100) = 50 / 100 = 0.5.
        var normalizedValue = StatRanges.NormalizeStatValue(valueStat, 150f);
        normalizedValue.Should().Be(0.5f);
    }

    /// <summary>
    ///     Tests that Clear() properly resets all accumulated ranges.
    /// </summary>
    [Test]
    public void Clear_ResetsAllRanges()
    {
        var stat = GetTestStat();

        // Accumulate a range [2.0, 5.0].
        StatRanges.NormalizeStatValue(stat, 2.0f);
        StatRanges.NormalizeStatValue(stat, 5.0f);
        var before = StatRanges.NormalizeStatValue(stat, 3.0f);
        // range [2.0, 5.0]: 3.0 → (3.0-2.0)/(5.0-2.0) = 1.0/3.0 = 0.333...
        before.Should().BeApproximately(1.0f / 3.0f, 0.001f);

        // Clear the cache.
        StatRanges.Clear();

        // After clearing, the same value 3.0 treated as first value in new range [3.0, 3.0].
        // Degenerate range returns 0 via NormalizeValue.
        var after = StatRanges.NormalizeStatValue(stat, 3.0f);
        after.Should().Be(0.0f, "after Clear, first value in new degenerate range [v,v] returns 0");
    }
}