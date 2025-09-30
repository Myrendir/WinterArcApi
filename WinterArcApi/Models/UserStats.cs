namespace WinterArcApi.Models;

public class UserStats
{
    public int Id { get; set; }
    public int TotalXp { get; set; } = 0;
    public int Level { get; set; } = 1;
    public int GoalsCompleted { get; set; } = 0;
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateTime LastCheckIn { get; set; } = DateTime.UtcNow;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
