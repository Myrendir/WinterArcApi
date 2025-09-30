namespace WinterArcApi.Models;

public class Goal
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int XpReward { get; set; } = 100;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; } = false;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
}
