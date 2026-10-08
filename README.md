# SH Entry Portal

A member entry and management portal for the sisterhood organization, built with Blazor Server (.NET) and Supabase (PostgreSQL + Auth).

## Features

- **Member Entry** — add new members with validated fields (name, contact info, role, status, join date)
- **Manage Members** — searchable/autocomplete member list, inline editing, Active/Inactive status toggle, pagination
- **Events** — add, search/edit, and delete sisterhood events (name, location, date/time, paid/free + price)
- **Admin Authentication** — real Supabase Auth login, backed by ASP.NET Core cookie authentication
- **Session security** — 5-minute idle timeout, logout on tab/browser close, manual logout
- **Audit trail** — every member/event creation, status change, edit, and deletion is logged with who made the change and when
- **Rate limiting** — login attempts are throttled to slow down brute-force attempts
- **CSRF protection** on the login form
- **Health check + keep-alive** — `/health` endpoint reports live Supabase connectivity (shown on the Home page), pinged on a schedule via GitHub Actions to prevent the Supabase free-tier project from auto-pausing due to inactivity

## Tech Stack

- **Frontend/Backend:** Blazor Server (.NET 10, C#)
- **Database:** Supabase (PostgreSQL), accessed via Entity Framework Core + Npgsql
- **Authentication:** Supabase Auth (credential verification) + ASP.NET Core cookie authentication (session management)
- **Hosting:** Azure App Service (Linux, F1 free tier)
- **CI/CD:** GitHub Actions (auto-deploys `main` branch to Azure)

## Project Structure

```
Components/
  Layout/         Shared layout and navigation
  Pages/          Routable pages (Home, Login, MemberEntry, ManageMembers, Events, ManageEvents)
Data/
  AppDbContext.cs EF Core database context (Members, AuditLogs, Events, EventAuditLogs)
Models/
  Generated/      Member/Event models, enums (MemberRole, MemberStatus), AuditLog, EventAuditLog
Services/
  MemberService.cs  Member CRUD + audit logging
  EventService.cs   Event CRUD + audit logging
  AuthService.cs    Supabase Auth credential verification
wwwroot/
  idle-logout.js  Client-side idle timeout / tab-close logout
Program.cs        App startup, auth, rate limiting, DB configuration, auth + health endpoints
.github/workflows/
  main_sh-entry-portal-app.yml  CI/CD — build and deploy to Azure
  keep-alive.yml                Scheduled ping of /health to keep Supabase/Azure warm
```

## Local Development Setup

### Prerequisites
- .NET 10 SDK
- A Supabase project (Postgres database + Auth enabled)

### Configuration
This app reads secrets from **user-secrets** locally (never committed to source control). Set them up with:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:SupabaseConnection" "<your Supabase Postgres connection string>"
dotnet user-secrets set "Supabase:AnonKey" "<your Supabase publishable/anon key>"
```

The Supabase project URL is stored in `appsettings.json` under `Supabase:Url` (safe to commit — it's not a secret).

### Database schema
The `members` table requires these columns: `id` (uuid), `first_name`, `last_name`, `phone`, `email`, `role` (enum: Member, Leadership, President, Vice President), `status` (enum: Active, Inactive, Pending, Honorary — Honorary is reserved for future use and hidden from the UI dropdown), `joined_on` (date), `created_at`, `updated_at`.

An `audit_log` table tracks changes: `id`, `member_id`, `action`, `changed_by`, `changed_at`.

### Running locally

```bash
dotnet run
```

Then open `http://localhost:5055`.

## Deployment

Deployed on **Azure App Service** (resource group `sh-entry-portal-rg`, app name `sh-entry-portal-app`). Any push to `main` triggers an automatic build and deploy via the GitHub Actions workflow in `.github/workflows/`.

Production secrets (Supabase connection string and anon key) are configured as Azure App Service application settings — never stored in the repository.

### Keeping Supabase/Azure warm

Supabase free-tier projects auto-pause after a period of inactivity. A scheduled GitHub Actions workflow (`.github/workflows/keep-alive.yml`) curls the app's `/health` endpoint every 6 hours, which performs a lightweight `CanConnectAsync` check against the database — keeping Supabase active and warming up the Azure App Service. The same check powers the "Portal Status" card on the Home page. Note: the Azure F1 free tier doesn't support "Always On", so the app can still cold-start between pings; upgrading to a Basic tier or higher removes that limitation.

## Security Notes

- Admin login is verified against Supabase Auth; sessions use short-lived, non-persistent cookies.
- The Supabase `service_role`/secret key must **never** be added to this app (client or server side) — only the publishable/anon key is used.
- Login attempts are rate-limited (5 per minute) to slow down brute-force attempts.
