# VisitorManagementSystem

Professional C# Windows Forms visitor management system for a university Visual Programming course. The application is designed for a reception or security desk where multiple employees and multiple laptops use the same shared Azure SQL Database.

## Technologies

- C# and Windows Forms
- .NET 8
- Entity Framework Core Code First
- Azure SQL Database
- SQL Server EF Core provider
- Visual Studio solution structure
- Git and GitHub-ready configuration

## Main Features

- Employee login with hashed passwords
- Default account: `admin` / `Admin123!`
- Visitor registration with unique identity number
- Turkish TC Kimlik No checksum validation and 99-starting foreign identity number support
- Visitor photo upload to `VisitorPhotos/`
- TODO-marked webcam `Take Photo` button, with `Upload Photo` implemented now
- Returning visitor detection by identity number
- Visitor sign-in with automatic `EntryTime`
- Visitor sign-out with automatic `ExitTime` and duration calculation
- Duplicate active visit prevention
- Blacklist management; active blacklist records block entry
- Active visitors screen
- Completed visit history with date, resident, visitor, and search filtering
- Resident management
- Privacy masking for listing screens: identity numbers are displayed as `12********1`

## Azure SQL Connection

The app reads the database connection from `appsettings.json`. This file is intentionally ignored by Git because it contains the real Azure SQL password.

Create `VisitorManagementSystem/appsettings.json` locally:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:visitorserver.database.windows.net,1433;Initial Catalog=VisitorManagementDatabase;Persist Security Info=False;User ID=VisitorManagementServer;Password=YOUR_REAL_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

Every laptop/user must create their own local `appsettings.json`. Make sure the Azure SQL firewall allows the laptop's public IP address.

## Database Structure

- `Employee`: login user, password hash, active state
- `Visitor`: identity number, full name, photo path
- `Resident`: identity number, full name, photo path, room/house number
- `Visit`: visitor, resident, reason, entry time, exit time, duration, status
- `BlacklistEntry`: visitor, reason, added date, active state

Rules enforced in the model/services:

- `Visitor.IdentityNumber` is unique
- `Resident.IdentityNumber` is unique
- A visitor can have only one active visit at a time
- Blacklisted visitors cannot enter
- Entry and exit times are generated automatically
- Duration is calculated automatically in minutes
- Seed data includes the default admin employee and sample residents

## Forms

- `LoginForm`
- `MainDashboardForm`
- `VisitorsForm`
- `VisitorEntryForm`
- `VisitorExitForm`
- `ActiveVisitorsForm`
- `VisitHistoryForm`
- `ResidentsForm`
- `BlacklistForm`
- `SettingsForm`

## Services

Forms call services, and services call the database:

`Forms -> Services -> AppDbContext -> Azure SQL`

Services:

- `AuthService`
- `VisitorService`
- `VisitService`
- `ResidentService`
- `BlacklistService`
- `DurationService`
- `IdentityValidationService`
- `ImageService`
- `PasswordHashService`
- `DataMaskingService`

## Collections Used

- `List<Visitor>` for loaded visitor lists
- `List<Visit>` for loaded visit lists
- `Dictionary<string, Visitor>` for fast visitor lookup by identity number
- `Queue<Visit>` in `AppState.EntryQueue` as an entry queue example
- `Stack<string>` in `AppState.OperationHistory` for operation history/logging

## Migrations

Install or use the EF tool, then run from the repository root:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project VisitorManagementSystem
dotnet ef database update --project VisitorManagementSystem
```

The app also attempts `Database.Migrate()` at startup, so pending migrations are applied when the connection is valid.

## Run

```powershell
dotnet restore VisitorManagementSystem.sln
dotnet build VisitorManagementSystem.sln
dotnet run --project VisitorManagementSystem
```

Open the generated solution `VisitorManagementSystem.sln` in Visual Studio for normal course development.
