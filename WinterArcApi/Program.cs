using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WinterArcApi.Data;
using WinterArcApi.DTOs;
using WinterArcApi.Models;
using WinterArcApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IXpService, XpService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Auth endpoints
app.MapPost("/auth/register", async (RegisterRequest request, AppDbContext db, IJwtService jwtService) =>
{
    if (await db.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
    {
        return Results.BadRequest(new { message = "Username or email already exists" });
    }

    var user = new User
    {
        Username = request.Username,
        Email = request.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    var stats = new UserStats { UserId = user.Id };
    db.UserStats.Add(stats);
    await db.SaveChangesAsync();

    var token = jwtService.GenerateToken(user);
    return Results.Ok(new AuthResponse(token, user.Id, user.Username));
}).WithName("Register").WithOpenApi();

app.MapPost("/auth/login", async (LoginRequest request, AppDbContext db, IJwtService jwtService) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
    
    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.BadRequest(new { message = "Invalid credentials" });
    }

    var token = jwtService.GenerateToken(user);
    return Results.Ok(new AuthResponse(token, user.Id, user.Username));
}).WithName("Login").WithOpenApi();

// Goal endpoints
app.MapGet("/goals", async (AppDbContext db, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    
    var goals = await db.Goals
        .Where(g => g.UserId == userId)
        .Include(g => g.CheckIns)
        .Select(g => new GoalResponse(
            g.Id,
            g.Title,
            g.Description,
            g.XpReward,
            g.CreatedAt,
            g.IsCompleted,
            g.CheckIns.Count
        ))
        .ToListAsync();
    
    return Results.Ok(goals);
}).RequireAuthorization().WithName("GetGoals").WithOpenApi();

app.MapPost("/goals", async (CreateGoalRequest request, AppDbContext db, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    
    var goal = new Goal
    {
        Title = request.Title,
        Description = request.Description,
        XpReward = request.XpReward,
        UserId = userId
    };

    db.Goals.Add(goal);
    await db.SaveChangesAsync();

    return Results.Ok(new GoalResponse(
        goal.Id,
        goal.Title,
        goal.Description,
        goal.XpReward,
        goal.CreatedAt,
        goal.IsCompleted,
        0
    ));
}).RequireAuthorization().WithName("CreateGoal").WithOpenApi();

app.MapPut("/goals/{id}", async (int id, UpdateGoalRequest request, AppDbContext db, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    
    var goal = await db.Goals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
    if (goal == null)
    {
        return Results.NotFound(new { message = "Goal not found" });
    }

    if (request.Title != null) goal.Title = request.Title;
    if (request.Description != null) goal.Description = request.Description;
    if (request.XpReward.HasValue) goal.XpReward = request.XpReward.Value;
    if (request.IsCompleted.HasValue) goal.IsCompleted = request.IsCompleted.Value;

    await db.SaveChangesAsync();

    var checkInCount = await db.CheckIns.CountAsync(c => c.GoalId == id);
    
    return Results.Ok(new GoalResponse(
        goal.Id,
        goal.Title,
        goal.Description,
        goal.XpReward,
        goal.CreatedAt,
        goal.IsCompleted,
        checkInCount
    ));
}).RequireAuthorization().WithName("UpdateGoal").WithOpenApi();

app.MapDelete("/goals/{id}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    
    var goal = await db.Goals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
    if (goal == null)
    {
        return Results.NotFound(new { message = "Goal not found" });
    }

    db.Goals.Remove(goal);
    await db.SaveChangesAsync();

    return Results.NoContent();
}).RequireAuthorization().WithName("DeleteGoal").WithOpenApi();

// CheckIn endpoint
app.MapPost("/goals/{id}/checkins", async (int id, CreateCheckInRequest request, AppDbContext db, IXpService xpService, ClaimsPrincipal user) =>
{
    var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    
    var goal = await db.Goals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
    if (goal == null)
    {
        return Results.NotFound(new { message = "Goal not found" });
    }

    var checkIn = new CheckIn
    {
        GoalId = id,
        Note = request.Note
    };

    db.CheckIns.Add(checkIn);

    var stats = await db.UserStats.FirstOrDefaultAsync(s => s.UserId == userId);
    if (stats != null)
    {
        stats.TotalXp += goal.XpReward;
        stats.Level = xpService.CalculateLevel(stats.TotalXp);
        
        var daysSinceLastCheckIn = (DateTime.UtcNow - stats.LastCheckIn).Days;
        if (daysSinceLastCheckIn == 1)
        {
            stats.CurrentStreak++;
            if (stats.CurrentStreak > stats.LongestStreak)
            {
                stats.LongestStreak = stats.CurrentStreak;
            }
        }
        else if (daysSinceLastCheckIn > 1)
        {
            stats.CurrentStreak = 1;
        }
        
        stats.LastCheckIn = DateTime.UtcNow;

        if (goal.IsCompleted == false)
        {
            goal.IsCompleted = true;
            stats.GoalsCompleted++;
        }
    }

    await db.SaveChangesAsync();

    return Results.Ok(new CheckInResponse(
        checkIn.Id,
        checkIn.CheckInDate,
        checkIn.Note,
        checkIn.GoalId
    ));
}).RequireAuthorization().WithName("CreateCheckIn").WithOpenApi();

// User overview endpoint
app.MapGet("/me/overview", async (AppDbContext db, IXpService xpService, ClaimsPrincipal user, DateTime? from, DateTime? to) =>
{
    var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    
    var stats = await db.UserStats.FirstOrDefaultAsync(s => s.UserId == userId);
    if (stats == null)
    {
        return Results.NotFound(new { message = "User stats not found" });
    }

    var xpToNextLevel = xpService.GetXpToNextLevel(stats.TotalXp);

    return Results.Ok(new UserOverviewResponse(
        stats.Level,
        stats.TotalXp,
        xpToNextLevel,
        stats.GoalsCompleted,
        stats.CurrentStreak,
        stats.LongestStreak,
        stats.LastCheckIn
    ));
}).RequireAuthorization().WithName("GetUserOverview").WithOpenApi();

app.Run();

