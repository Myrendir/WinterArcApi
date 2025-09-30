namespace WinterArcApi.Services;

public interface IXpService
{
    int CalculateLevel(int totalXp);
    int GetXpForLevel(int level);
    int GetXpToNextLevel(int totalXp);
}

public class XpService : IXpService
{
    public int CalculateLevel(int totalXp)
    {
        return (int)Math.Floor(Math.Sqrt(totalXp / 100.0)) + 1;
    }

    public int GetXpForLevel(int level)
    {
        return (level - 1) * (level - 1) * 100;
    }

    public int GetXpToNextLevel(int totalXp)
    {
        var currentLevel = CalculateLevel(totalXp);
        var xpForNextLevel = GetXpForLevel(currentLevel + 1);
        return xpForNextLevel - totalXp;
    }
}
