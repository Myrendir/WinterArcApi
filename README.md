# WinterArcApi
Minimal API + EF Core (PostgreSQL) + JWT. Powers the Winter Arc PWA.

## Features

- **User Authentication**: JWT-based authentication with secure password hashing (BCrypt)
- **Goal Management**: Full CRUD operations for user goals
- **Progress Tracking**: Check-ins with XP and level calculations
- **User Statistics**: Track XP, levels, streaks, and achievements
- **PostgreSQL Database**: Robust data persistence with Entity Framework Core
- **Docker Support**: Easy deployment with Docker and docker-compose

## Tech Stack

- ASP.NET 8 Minimal APIs
- Entity Framework Core 8
- PostgreSQL 15
- JWT Authentication
- BCrypt for password hashing
- Docker & Docker Compose

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose (for containerized deployment)
- PostgreSQL 15 (if running locally without Docker)

### Running with Docker Compose

```bash
docker-compose up --build
```

The API will be available at `http://localhost:8080`

### Running Locally

1. Install dependencies:
```bash
cd WinterArcApi
dotnet restore
```

2. Update connection string in `appsettings.json`

3. Apply migrations:
```bash
dotnet ef database update
```

4. Run the application:
```bash
dotnet run
```

## API Endpoints

### Authentication

#### Register
```http
POST /auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```

#### Login
```http
POST /auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "SecurePassword123"
}
```

### Goals

#### Get All Goals
```http
GET /goals
Authorization: Bearer {token}
```

#### Create Goal
```http
POST /goals
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Exercise Daily",
  "description": "Go for a 30-minute run",
  "xpReward": 150
}
```

#### Update Goal
```http
PUT /goals/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Exercise Daily - Updated",
  "isCompleted": true
}
```

#### Delete Goal
```http
DELETE /goals/{id}
Authorization: Bearer {token}
```

### Check-Ins

#### Create Check-In
```http
POST /goals/{id}/checkins
Authorization: Bearer {token}
Content-Type: application/json

{
  "note": "Completed 5k run today!"
}
```

### User Statistics

#### Get User Overview
```http
GET /me/overview
Authorization: Bearer {token}
```

Response includes:
- Current level
- Total XP
- XP needed for next level
- Goals completed
- Current streak
- Longest streak
- Last check-in date

## XP System

The XP and level system uses a square root-based progression:

- **Level Calculation**: `Level = floor(sqrt(TotalXP / 100)) + 1`
- **XP for Level**: `XP = (Level - 1)² × 100`

Examples:
- Level 1: 0 XP
- Level 2: 100 XP
- Level 3: 400 XP
- Level 4: 900 XP
- Level 5: 1,600 XP

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Database=winterarc;Username=postgres;Password=postgres` |
| `Jwt__Key` | JWT signing key | `WinterArc2024SecretKeyForJWTAuthentication!` |
| `Jwt__Issuer` | JWT issuer | `WinterArcApi` |
| `Jwt__Audience` | JWT audience | `WinterArcApp` |

## Database Schema

### Users
- `Id` (PK)
- `Username` (Unique)
- `Email` (Unique)
- `PasswordHash`
- `CreatedAt`

### Goals
- `Id` (PK)
- `Title`
- `Description`
- `XpReward`
- `CreatedAt`
- `IsCompleted`
- `UserId` (FK)

### CheckIns
- `Id` (PK)
- `CheckInDate`
- `Note`
- `GoalId` (FK)

### UserStats
- `Id` (PK)
- `TotalXp`
- `Level`
- `GoalsCompleted`
- `CurrentStreak`
- `LongestStreak`
- `LastCheckIn`
- `UserId` (FK)

## Development

### Swagger UI

When running in development mode, access Swagger UI at:
```
http://localhost:8080/swagger
```

### Adding Migrations

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

## License

This project is open source and available under the MIT License.
