# Azure Deployment Guide — Property Management System

## What You Need Before Starting
- Azure account with active credits (provided by tutor)
- Azure CLI installed **or** use Azure Portal
- .NET 9 SDK installed locally

---

## Step 1 — Create 3 Azure App Services + 1 SQL Server

In the Azure Portal, create:

| Resource | Name suggestion | Runtime |
|---|---|---|
| App Service 1 | `pm-api` | .NET 9 |
| App Service 2 | `pm-mvc` | .NET 9 |
| App Service 3 | `pm-reporting` | .NET 9 |
| SQL Server | `sql-property-management` | Already exists ✅ |

> **Cost tip:** Use the **Free (F1)** App Service plan for all three.  
> The SQL Server already exists — do NOT create a new one.

---

## Step 2 — Update Configuration with Azure URLs

Once you have your 3 App Service URLs (e.g. `https://pm-api.azurewebsites.net`),
update **`appsettings.Production.json`** in each project:

### PropertyManagement.API → `appsettings.Production.json`
```json
"AllowedOrigins": {
  "MvcUrl":       "https://pm-mvc.azurewebsites.net",
  "ReportingUrl": "https://pm-reporting.azurewebsites.net"
}
```

### PropertyManagement.MVC → `appsettings.Production.json`
```json
"ApiSettings": {
  "BaseUrl": "https://pm-api.azurewebsites.net/"
}
```

### PropertyManagement.Reporting → `appsettings.Production.json`
```json
"ApiSettings": {
  "BaseUrl": "https://pm-api.azurewebsites.net/"
}
```

---

## Step 3 — Apply Database Migrations

Run this **once** from the `PropertyManagement.API` folder:

```bash
cd PropertyManagement.API
dotnet ef database update --connection "Server=tcp:sql-property-management.database.windows.net,1433;Initial Catalog=PropertyManagementDB;Persist Security Info=False;User ID=managementAdmin;Password=u202202670!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

> ✅ The app also auto-migrates on first startup, so this is a safety step.

---

## Step 4 — Publish Each Project

### Option A — Visual Studio (Easiest)
1. Right-click `PropertyManagement.API` → **Publish**
2. Choose **Azure App Service (Windows)**
3. Select the `pm-api` App Service you created
4. Click **Publish**
5. Repeat for `PropertyManagement.MVC` → `pm-mvc`
6. Repeat for `PropertyManagement.Reporting` → `pm-reporting`

### Option B — Azure CLI
```bash
# From solution root:

# Publish API
dotnet publish PropertyManagement.API -c Release -o ./publish/api
cd publish/api && zip -r api.zip .
az webapp deploy --resource-group <your-rg> --name pm-api --src-path api.zip

# Publish MVC
dotnet publish PropertyManagement.MVC -c Release -o ./publish/mvc
cd ../mvc && zip -r mvc.zip .
az webapp deploy --resource-group <your-rg> --name pm-mvc --src-path mvc.zip

# Publish Reporting
dotnet publish PropertyManagement.Reporting -c Release -o ./publish/reporting
cd ../reporting && zip -r reporting.zip .
az webapp deploy --resource-group <your-rg> --name pm-reporting --src-path reporting.zip
```

---

## Step 5 — Set Environment Variable on Each App Service

In Azure Portal → App Service → **Configuration** → **Application settings**,
add this for all 3 App Services:

| Name | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |

This tells .NET to use `appsettings.Production.json` instead of `appsettings.json`.

---

## Step 6 — Allow Azure App Service IPs in SQL Firewall

In Azure Portal → SQL Server `sql-property-management` → **Networking**:
- Turn **ON**: "Allow Azure services and resources to access this server"
- This lets all 3 App Services reach the database.

---

## Step 7 — Verify Deployment

| Check | URL |
|---|---|
| API Swagger | `https://pm-api.azurewebsites.net/swagger` |
| MVC App | `https://pm-mvc.azurewebsites.net` |
| Reporting App | `https://pm-reporting.azurewebsites.net` |
| Public Tracking | `https://pm-mvc.azurewebsites.net/Tracking` |

### Test Login Credentials
| Role | Email | Password |
|---|---|---|
| Property Manager | admin@example.com | Admin123! |
| Maintenance Staff | staff@example.com | Staff123! |
| Tenant 1 | tenant1@example.com | Tenant123! |
| Tenant 2 | tenant2@example.com | Tenant123! |

---

## Common Problems & Fixes

| Problem | Fix |
|---|---|
| 500 error on startup | Check App Service logs → Log Stream. Usually a missing config value. |
| "Cannot connect to SQL" | Confirm firewall rule (Step 6) is enabled |
| Login works but redirects to /Account/Login again | `ASPNETCORE_ENVIRONMENT` not set to `Production` — cookie keys mismatch |
| Reporting app 401 on all API calls | JWT token expired or API BaseUrl wrong in appsettings.Production.json |
| SignalR board not updating | CORS — MVC URL not in API's AllowedOrigins config (Step 2) |
| Swagger not visible | Already enabled in all environments ✅ go to /swagger directly |

---

## Azure Credits — How to Save Them

- Keep all App Services on **Free (F1)** tier
- **Stop** App Services when not actively demoing (Portal → Overview → Stop)
- Do NOT create extra databases or storage accounts
- Delete any test App Services you created while experimenting
