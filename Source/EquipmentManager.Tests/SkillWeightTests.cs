using LordKuper.EquipmentManager;

namespace EquipmentManager.Tests;

/// <summary>
///     Tests for the <see cref="SkillWeight" /> class covering construction and value storage.
///     These exercise pure data behaviour and do not touch the RimWorld <c>DefDatabase</c>.
/// </summary>
[TestFixture]
public class SkillWeightTests
{
    /// <summary>
    ///     Tests that the name-only constructor stores the skill def name and defaults the weight to zero.
    /// </summary>
    [Test]
    public void Constructor_NameOnly_StoresNameAndZeroWeight()
    {
        var skillWeight = new SkillWeight("Mining");
        skillWeight.SkillDefName.Should().Be("Mining");
        skillWeight.Weight.Should().Be(0f);
    }

    /// <summary>
    ///     Tests that the name-and-weight constructor stores both values.
    /// </summary>
    [Test]
    public void Constructor_NameAndWeight_StoresBoth()
    {
        var skillWeight = new SkillWeight("Shooting", 1.5f);
        skillWeight.SkillDefName.Should().Be("Shooting");
        skillWeight.Weight.Should().Be(1.5f);
    }

    /// <summary>
    ///     Tests that the weight cap constant matches the documented limit.
    /// </summary>
    [Test]
    public void WeightCap_IsTwo()
    {
        SkillWeight.WeightCap.Should().Be(2f);
    }
}