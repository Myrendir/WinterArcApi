namespace WinterArcApi.Models;

public class CheckIn
{
    public int Id { get; set; }
    public DateTime CheckInDate { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
    
    public int GoalId { get; set; }
    public Goal Goal { get; set; } = null!;
}
