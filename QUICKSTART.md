# WinterArc API - Quick Start Guide

## 🚀 Quick Start

### Using Docker (Recommended)
```bash
docker-compose up --build
```
API available at: http://localhost:8080

### Local Development
```bash
# Install dependencies
cd WinterArcApi
dotnet restore

# Run migrations
dotnet ef database update

# Start the API
dotnet run
```

## 📋 Quick API Reference

### Authentication
```bash
# Register
curl -X POST http://localhost:8080/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"user","email":"user@example.com","password":"pass123"}'

# Login
curl -X POST http://localhost:8080/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user","password":"pass123"}'
```

### Goals (Requires JWT Token)
```bash
# List goals
curl -X GET http://localhost:8080/goals \
  -H "Authorization: Bearer YOUR_TOKEN"

# Create goal
curl -X POST http://localhost:8080/goals \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Exercise","description":"Daily workout","xpReward":100}'

# Update goal
curl -X PUT http://localhost:8080/goals/1 \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Updated Title","isCompleted":true}'

# Delete goal
curl -X DELETE http://localhost:8080/goals/1 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Progress Tracking
```bash
# Create check-in
curl -X POST http://localhost:8080/goals/1/checkins \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"note":"Completed today!"}'

# Get user overview
curl -X GET http://localhost:8080/me/overview \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 🔧 Configuration

### Environment Variables
```bash
# PostgreSQL Connection
ConnectionStrings__DefaultConnection="Host=localhost;Database=winterarc;Username=postgres;Password=postgres"

# JWT Settings
Jwt__Key="YourSecretKeyMinimum32Characters!"
Jwt__Issuer="WinterArcApi"
Jwt__Audience="WinterArcApp"
```

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=winterarc;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "WinterArc2024SecretKeyForJWTAuthentication!",
    "Issuer": "WinterArcApi",
    "Audience": "WinterArcApp"
  }
}
```

## 📊 XP Level Calculator

| Level | Total XP Required | XP to Gain |
|-------|------------------|------------|
| 1     | 0                | -          |
| 2     | 100              | 100        |
| 3     | 400              | 300        |
| 4     | 900              | 500        |
| 5     | 1,600            | 700        |
| 6     | 2,500            | 900        |
| 10    | 8,100            | -          |
| 20    | 36,100           | -          |

Formula: `Level = floor(sqrt(TotalXP / 100)) + 1`

## 🧪 Testing

### Run Unit Tests
```bash
cd WinterArcApi.Tests
dotnet test
```

### Integration Testing
```bash
# Start API first, then:
./test-api.sh
```

### Swagger UI
http://localhost:8080/swagger (Development only)

## 📁 Project Structure

```
WinterArcApi/
├── WinterArcApi/           # Main API project
│   ├── Models/            # Entity models
│   ├── DTOs/              # Data transfer objects
│   ├── Data/              # DbContext
│   ├── Services/          # Business logic
│   ├── Migrations/        # EF Core migrations
│   └── Program.cs         # API endpoints
├── WinterArcApi.Tests/     # Unit tests
├── Dockerfile             # Container definition
├── docker-compose.yml     # Docker orchestration
└── test-api.sh           # Integration tests
```

## 🔐 Security Notes

- Passwords are hashed with BCrypt (cost factor 11)
- JWT tokens expire after 7 days
- Users can only access their own data
- All endpoints except auth require valid JWT token
- Use HTTPS in production

## 🐛 Troubleshooting

### Database Connection Issues
```bash
# Check PostgreSQL is running
docker ps | grep postgres

# Recreate database
docker-compose down -v
docker-compose up --build
```

### Migration Issues
```bash
# Remove and recreate migrations
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Build Issues
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## 📚 Additional Resources

- Full API Documentation: `README.md`
- Implementation Details: `API_SUMMARY.md`
- Architecture Diagram: `ARCHITECTURE.md`
- HTTP Requests: `WinterArcApi/WinterArcApi.http`

## 🤝 Support

For issues or questions:
1. Check the documentation files
2. Review the test files for examples
3. Check Swagger UI for endpoint details
4. Examine the migration files for schema
