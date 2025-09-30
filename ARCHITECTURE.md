# WinterArc API Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                          Client Layer                            │
│                     (Web/Mobile/Desktop)                         │
└───────────────────────────────┬─────────────────────────────────┘
                                │
                                │ HTTP/HTTPS + JWT
                                │
┌───────────────────────────────▼─────────────────────────────────┐
│                      API Layer (Minimal API)                     │
├──────────────────────────────────────────────────────────────────┤
│  Auth Endpoints           │  Goal Endpoints                      │
│  • POST /auth/register   │  • GET    /goals                     │
│  • POST /auth/login      │  • POST   /goals                     │
│                          │  • PUT    /goals/{id}                │
│                          │  • DELETE /goals/{id}                │
│                          │                                       │
│  CheckIn Endpoints       │  Stats Endpoints                     │
│  • POST /goals/{id}/     │  • GET /me/overview                 │
│    checkins              │                                      │
└───────────────┬──────────────────────────────┬──────────────────┘
                │                              │
                │                              │
    ┌───────────▼──────────┐       ┌──────────▼──────────┐
    │   Service Layer      │       │  Authentication      │
    ├──────────────────────┤       ├─────────────────────┤
    │  • XpService         │       │  • JwtService        │
    │    - CalculateLevel  │       │    - GenerateToken   │
    │    - GetXpForLevel   │       │    - ValidateToken   │
    │    - GetXpToNext     │       │  • BCrypt Hashing    │
    └───────────┬──────────┘       └──────────┬──────────┘
                │                              │
                │                              │
    ┌───────────▼──────────────────────────────▼──────────┐
    │              Data Access Layer (EF Core)             │
    ├──────────────────────────────────────────────────────┤
    │                    AppDbContext                       │
    │  • Users DbSet                                        │
    │  • Goals DbSet                                        │
    │  • CheckIns DbSet                                     │
    │  • UserStats DbSet                                    │
    └───────────────────────────┬──────────────────────────┘
                                │
                                │ Npgsql Provider
                                │
    ┌───────────────────────────▼──────────────────────────┐
    │              PostgreSQL Database                      │
    ├──────────────────────────────────────────────────────┤
    │  Tables:                                              │
    │  • Users (Id, Username, Email, PasswordHash, ...)     │
    │  • Goals (Id, Title, Description, XpReward, ...)      │
    │  • CheckIns (Id, CheckInDate, Note, GoalId)          │
    │  • UserStats (Id, TotalXp, Level, Streaks, ...)      │
    │                                                       │
    │  Relationships:                                       │
    │  • User 1:N Goals                                     │
    │  • User 1:1 UserStats                                 │
    │  • Goal 1:N CheckIns                                  │
    └───────────────────────────────────────────────────────┘
```

## Data Flow Examples

### 1. User Registration Flow
```
Client → POST /auth/register
   ↓
Validate input
   ↓
Hash password (BCrypt)
   ↓
Create User entity
   ↓
Save to database
   ↓
Create UserStats entity
   ↓
Save to database
   ↓
Generate JWT token
   ↓
Return token + user info
```

### 2. Create Goal Flow
```
Client → POST /goals (with JWT)
   ↓
Validate JWT token
   ↓
Extract userId from token
   ↓
Create Goal entity
   ↓
Link to user
   ↓
Save to database
   ↓
Return goal details
```

### 3. Check-In Flow (with XP)
```
Client → POST /goals/{id}/checkins (with JWT)
   ↓
Validate JWT token
   ↓
Verify goal ownership
   ↓
Create CheckIn entity
   ↓
Get user stats
   ↓
Award XP (from goal)
   ↓
Calculate new level (√ formula)
   ↓
Update streak logic
   ↓
Mark goal completed
   ↓
Save all changes
   ↓
Return checkin details
```

## Technology Stack

- **Framework**: ASP.NET 8 Minimal APIs
- **Database**: PostgreSQL 15
- **ORM**: Entity Framework Core 8
- **Authentication**: JWT Bearer Tokens
- **Password Hashing**: BCrypt.Net-Next
- **API Documentation**: Swagger/OpenAPI
- **Containerization**: Docker & Docker Compose
- **Testing**: xUnit

## Key Design Decisions

1. **Minimal APIs**: Chosen for simplicity and performance
2. **Sqrt-based XP**: Balanced progression curve
3. **Automatic Migrations**: Simplified deployment
4. **JWT Authentication**: Stateless, scalable auth
5. **Cascade Deletes**: Automatic cleanup of related data
6. **UTC Timestamps**: Consistent time handling
7. **Unique Constraints**: Username and email uniqueness enforced at DB level
