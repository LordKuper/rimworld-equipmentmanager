namespace EquipmentManager.Tests;

/// <summary>
///     Tests for the <see cref="PassionLimit" /> class covering construction and default values.
///     These exercise pure data behaviour and do not touch the RimWorld <c>DefDatabase</c>.
/// </summary>
[TestFixture]
public class PassionLimitTests
{
    /// <summary>
    ///     Tests that the parameterless constructor defaults the passion value to <see cref="PassionValue.None" />.
    /// </summary>
    [Test]
    public void Constructor_Parameterless_DefaultsToNone()
    {
        var passionLimit = new PassionLimit();
        passionLimit.Value.Should().Be(PassionValue.None);
    }

    /// <summary>
    ///     Tests that the name constructor stores the skill def name and defaults the value to
    ///     <see cref="PassionValue.None" />.
    /// </summary>
    [Test]
    public void Constructor_Name_StoresNameAndDefaultsValue()
    {
        var passionLimit = new PassionLimit("Mining");
        passionLimit.SkillDefName.Should().Be("Mining");
        passionLimit.Value.Should().Be(PassionValue.None);
    }
}