namespace WinterArcApi.DTOs;

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Username, string Password);
public record AuthResponse(string Token, int UserId, string Username);

public record CreateGoalRequest(string Title, string? Description, int XpReward = 100);
public record UpdateGoalRequest(string? Title, string? Description, int? XpReward, bool? IsCompleted);
public record GoalResponse(int Id, string Title, string? Description, int XpReward, DateTime CreatedAt, bool IsCompleted, int CheckInCount);

public record CreateCheckInRequest(string? Note);
public record CheckInResponse(int Id, DateTime CheckInDate, string? Note, int GoalId);

public record UserOverviewResponse(int Level, int TotalXp, int XpToNextLevel, int GoalsCompleted, int CurrentStreak, int LongestStreak, DateTime LastCheckIn);
