# Repository Guidelines

## Project Structure & Module Organization
- Source: `Program.cs`, domain models in `Models/`, UI in `Forms/`, helpers in `Util/`.
- Data: EF Core context and migrations in `Models/AppDbContext.cs` and `Migrations/`.
- Assets: icons/docs in repo root (e.g., `logo.ico`, `help.pdf`).
- Build artifacts: `bin/` and `obj/` (generated). Local DB: `sylogos.db` (SQLite).

## Build, Test, and Development Commands
- `dotnet restore` — restore NuGet packages.
- `dotnet build -c Debug` — compile for development.
- `dotnet run --project SYLOGOS.csproj` — run the WinForms app.
- `dotnet ef migrations add <Name>` — add EF migration.
- `dotnet ef database update` — apply migrations to `sylogos.db`.
- `dotnet publish -c Release -r win-x86` — produce deployable build.
- `dotnet format` — format code to project conventions.

## Coding Style & Naming Conventions
- Indent with 4 spaces; UTF-8 encoding.
- C#: PascalCase for public types/members; camelCase for locals/fields; `_camelCase` for private fields.
- WinForms: name forms like `MainForm`, controls like `membersGrid`, event handlers `OnXyzClicked`.
- Keep UI logic in `Forms/`; data/domain logic in `Models/` or `Util/`.
- Prefer async APIs where available; avoid blocking UI thread.

## Contribution Rules
- All code (new and existing) must be documented. When you edit code that lacks documentation, add appropriate XML docs/comments as part of the change.
- NEVER create files unless they are absolutely necessary to achieve the goal. The same applies to services, classes, methods, and variables.
- ALWAYS prefer editing an existing file over creating a new one. The same applies to services, classes, methods, and variables.

## Testing Guidelines
- No formal test suite yet. Perform manual QA before PRs:
  - Launch app, create/edit members, run Queries, generate Receipt/Member Card (PDFs).
  - Verify migrations apply cleanly to a fresh `sylogos.db`.
- If adding tests, use `xUnit` with a new `Tests/` project and `dotnet test`.

## Commit & Pull Request Guidelines
- Commit messages: concise, imperative (e.g., "fix query filter", "add receipt export").
- Scope prefix optional (e.g., `forms:`, `models:`). Keep to one logical change per commit.
- PRs: include summary, linked issues, screenshots/GIFs for UI changes, and notes on DB changes/migrations.
- Include repro steps and risk/rollback notes for significant changes.

## Security & Configuration Tips
- Certificates: `sylogos.pfx` is for local/dev. Do not commit real certs or secrets; generate via `self_signed_cert_generation.ps1`.
- DB: avoid committing personal data in `sylogos.db`. Use a throwaway DB when possible.
- Secrets/config go in user-level settings or environment variables; do not hardcode.

## Agent-Specific Notes
- Keep changes minimal and focused; follow folder boundaries above.
- Update migrations and docs when altering models; validate app starts and core flows work.
