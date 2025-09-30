using WinterArcApi.Services;

namespace WinterArcApi.Tests;

public class XpServiceTests
{
    private readonly XpService _xpService;

    public XpServiceTests()
    {
        _xpService = new XpService();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(100, 2)]
    [InlineData(400, 3)]
    [InlineData(900, 4)]
    [InlineData(1600, 5)]
    [InlineData(2500, 6)]
    public void CalculateLevel_ShouldReturnCorrectLevel(int totalXp, int expectedLevel)
    {
        var level = _xpService.CalculateLevel(totalXp);
        Assert.Equal(expectedLevel, level);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 100)]
    [InlineData(3, 400)]
    [InlineData(4, 900)]
    [InlineData(5, 1600)]
    [InlineData(6, 2500)]
    public void GetXpForLevel_ShouldReturnCorrectXp(int level, int expectedXp)
    {
        var xp = _xpService.GetXpForLevel(level);
        Assert.Equal(expectedXp, xp);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(50, 50)]
    [InlineData(100, 300)]
    [InlineData(200, 200)]
    [InlineData(400, 500)]
    public void GetXpToNextLevel_ShouldReturnCorrectXpNeeded(int currentXp, int expectedXpNeeded)
    {
        var xpNeeded = _xpService.GetXpToNextLevel(currentXp);
        Assert.Equal(expectedXpNeeded, xpNeeded);
    }
}
