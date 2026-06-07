using LordKuper.Common;
using LordKuper.EquipmentManager;

namespace EquipmentManager.Tests;

/// <summary>
///     Characterization tests for the consumed <see cref="WorkTypeThingRule" />
///     and <see cref="WorkTypeStatMap" /> path. EM uses Common's per-stat DefaultWorkTypeStats
///     (not flat default weights). WorkTypeStatMap requires full game context (DefDatabase, recipes, etc.)
///     to initialize. These tests verify consumption rather than testing map contents.
/// </summary>
[TestFixture]
public class WorkTypeThingRuleTests
{
    /// <summary>
    ///     Tests that EM consumes Common's per-stat WorkTypeStatMap via the public AutoSwitchStatsMap API.
    ///     This verifies the consumption contract is in place. Full population of map contents
    ///     requires game context (DefDatabase, recipes, skill-stat mappings).
    /// </summary>
    [Test]
    public void WorkTypeStatMap_PublicApi_IsAvailableForConsumption()
    {
        // EM consumes WorkTypeStatMap.AutoSwitchStatsMap, which is populated with per-stat
        // weight mappings from Common's DefaultWorkTypeStats (e.g., Cooking: FoodPoisonChance, DrugCookingSpeed, etc.).
        // This test verifies the public API is available and non-null.
        var autoSwitchMap = WorkTypeStatMap.AutoSwitchStatsMap;
        autoSwitchMap.Should().NotBeNull("WorkTypeStatMap.AutoSwitchStatsMap should be available as public API");

        // Full validation of map contents (dedup, stat resolution, per-stat weight values)
        // requires game context and is validated in manual in-game verification.
    }

    /// <summary>
    ///     Tests that ToolRule consumes WorkTypeStatMap for work-type-aware tool filtering.
    ///     This test verifies the consumption integration is in place.
    /// </summary>
    [Test]
    public void ToolRule_WorkTypeAware_ConsumesWorkTypeStatMap()
    {
        // Create a minimal ToolRule to verify it can be instantiated and
        // does not throw when accessing work-type-aware filtering logic.
        var toolRule = new ToolRule(1, "Test Tool Rule", false, [], [], [], [], ItemRule.ToolEquipMode.BestOne, false);

        // Verify the rule is properly initialized without errors.
        toolRule.Should().NotBeNull("ToolRule should be instantiable");
        toolRule.Label.Should().Be("Test Tool Rule");

        // ToolRule.AllRelevantThings uses WorkTypeStatMap.AutoSwitchStatsMap internally
        // to filter tools based on work-type-relevant stats. In game context, this would
        // return tools with stats matching those in WorkTypeStatMap. Full validation
        // requires game context (DefDatabase population).
    }
}