# SH Entry Portal

A member entry and management portal for the sisterhood organization, built with Blazor Server (.NET) and Supabase (PostgreSQL + Auth).

## Features

- **Member Entry** — add new members with validated fields (name, contact info, role, status, join date)
- **Manage Members** — searchable/autocomplete member list, inline editing, Active/Inactive status toggle, pagination
- **Admin Authentication** — real Supabase Auth login, backed by ASP.NET Core cookie authentication
- **Session security** — 5-minute idle timeout, logout on tab/browser close, manual logout
- **Audit trail** — every member creation, status change, and edit is logged with who made the change and when
- **Rate limiting** — login attempts are throttled to slow down brute-force attempts
- **CSRF protection** on the login form

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
  Pages/          Routable pages (Home, Login, MemberEntry, ManageMembers)
Data/
  AppDbContext.cs EF Core database context (Members, AuditLogs)
Models/
  Generated/      Member model, enums (MemberRole, MemberStatus), AuditLog
Services/
  MemberService.cs  Member CRUD + audit logging
  AuthService.cs    Supabase Auth credential verification
wwwroot/
  idle-logout.js  Client-side idle timeout / tab-close logout
Program.cs        App startup, auth, rate limiting, DB configuration, auth endpoints
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
The `members` table requires these columns: `id` (uuid), `first_name`, `last_name`, `phone`, `email`, `role` (enum: Member, Officer, President, Vice President, Secretary, Treasurer, Chaplain), `status` (enum: Active, Inactive, Pending, Honorary), `joined_on` (date), `created_at`, `updated_at`.

An `audit_log` table tracks changes: `id`, `member_id`, `action`, `changed_by`, `changed_at`.

### Running locally

```bash
dotnet run
```

Then open `http://localhost:5055`.

## Deployment

Deployed on **Azure App Service** (resource group `sh-entry-portal-rg`, app name `sh-entry-portal-app`). Any push to `main` triggers an automatic build and deploy via the GitHub Actions workflow in `.github/workflows/`.

Production secrets (Supabase connection string and anon key) are configured as Azure App Service application settings — never stored in the repository.

## Security Notes

- Admin login is verified against Supabase Auth; sessions use short-lived, non-persistent cookies.
- The Supabase `service_role`/secret key must **never** be added to this app (client or server side) — only the publishable/anon key is used.
- Login attempts are rate-limited (5 per minute) to slow down brute-force attempts.
