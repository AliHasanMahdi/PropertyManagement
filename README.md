Here's the improved `README.md` file incorporating the new content while maintaining the existing structure and coherence:

# Project Title

## Overview

Provide a brief description of the project, its purpose, and its main features.

## Getting Started

### Prerequisites

- .NET 9 SDK installed
- Visual Studio 2022 (or VS Code) with ASP.NET workloads
- LocalDB or a SQL Server instance accessible by the solution

### Installation

1. Clone the repository:
   git clone https://github.com/yourusername/yourproject.git
   cd yourproject

2. Restore the NuGet packages:
   dotnet restore

## Running Project 1 — PropertyManagement.API

This section explains how to run the Web API (Project 1).

1. Open the solution in Visual Studio.
2. Set `PropertyManagement.API` as the startup project and run it.
3. Confirm the API Base URL from the API project's launch settings (Properties > Debug).

## Running Project 2 — PropertyManagement.MVC (Public Tracking Only)

This section explains how to run the MVC app (Project 2) and verify the public tracking/lookup page. Provide these instructions to a teammate so they can run and test only Project 2.

### Steps

1. **Start the Web API (Project 1)**
- Open the solution in Visual Studio.
- Set `PropertyManagement.API` as a startup project and run it. The API must be running for the MVC tracking page to query it.
- By default, the API Base URL is set to `https://localhost:7166/`. Confirm the running port from the API project's launch settings (Properties > Debug).

2. **Ensure the API database schema exists**
- Open a Package Manager Console targeting the `PropertyManagement.API` project and run:
  ```powershell
  Update-Database
  ```
- Or using dotnet CLI inside the API project folder:
  ```bash
  dotnet ef database update --project PropertyManagement.API --startup-project PropertyManagement.API
  ```

3. **Configure the MVC app to point to the running API**
- In `PropertyManagement.MVC/appsettings.json`, set `ApiSettings:BaseUrl` to the API URL (e.g. `https://localhost:7166/`).
- `Program.cs` registers an `HttpClient` named `"API"` that will use this value if present.

4. **Run the MVC app (Project 2)**
- Set `PropertyManagement.MVC` as the startup project and run it.
- Open the site and navigate to `Tracking` from the main navigation or visit `/Tracking`.

5. **Test the tracking lookup**
- Enter a reference value (lease id, tenant email, or tenant CPR) and click `Lookup`.
- If the API and DB contain data for that reference, you will see a JSON or raw response presented on the page.

### Troubleshooting

- If the page shows `API base URL is not configured`, ensure `ApiSettings:BaseUrl` is present in `appsettings.json` and the value is a valid https URL.
- If you get 404 responses from the API, confirm the API is running and the path `api/tracking/{reference}` is available.
- If database migrations are missing, run the migrations for the API project.

### Notes for Handoff

- This handoff covers only Project 2 (the MVC/Razor Pages public tracking page). The API and database must be running for full end-to-end verification.
- Default admin credentials are present in `appsettings.json` for seeding (used by Identity seeding). Domain sample data may need to be created in the DB for tracking tests.

## Configuration File Example

### JSON - PropertyManagement.MVC/appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PropertyManagementDB;Trusted_Connection=True;"
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7166/"
  },
  "AdminUser": {
    "Email": "admin@example.com",
    "Password": "Admin123!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Contributing

Provide guidelines for contributing to the project.

## License

Include licensing information for the project.