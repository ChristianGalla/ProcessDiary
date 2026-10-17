# ProcessDiary - GitHub Copilot Instructions

ProcessDiary is a Windows 11 desktop app that builds a visual diary of computer activity (screenshots plus foreground application sessions).
It is a transparent journaling tool, not a surveillance tool.

## Core Principles

- User-controlled, privacy-first, local-first
- Recording status always visible; never invisible
- No hidden background monitoring
- No network transmission by default

## Privacy Rules

Never implement: keystroke logging, password capture, clipboard surveillance, hidden recording modes, undocumented telemetry.
Always: store data locally, show recording state, allow pausing and deleting data.
If a feature could be privacy-invasive, favor user control and transparency.
Do not implement cloud features unless explicitly requested.

## Technology

- Windows 11 only, WinUI 3, Windows App SDK, MSIX
- Target framework: match the solution (currently .NET 8)
- Preferred: CommunityToolkit.Mvvm, CommunityToolkit.WinUI, Microsoft.Extensions.Hosting/DependencyInjection/Logging, SQLite (EF Core or Dapper)
- Avoid unnecessary dependencies

## Architecture

Clean architecture with layers:

- UI: WinUI 3 views and view models. No business logic in code-behind.
- Application: use cases, services, commands, DTOs.
- Domain: entities, business rules, interfaces. Must not reference WinUI.
- Infrastructure: SQLite, Windows APIs, screenshot capture, process detection, file system.

Keep Win32 code isolated. Platform-specific services sit behind interfaces. ViewModels never couple to Windows capture APIs.

## Coding Guidelines

Prefer: async/await, dependency injection, immutable DTOs, cancellation tokens, nullable reference types.
Avoid: static service classes, service locators, blocking calls, async void (except UI events).

## MVVM

- All screens use MVVM with ObservableObject, RelayCommand, AsyncRelayCommand.
- Views never access repositories; they talk to ViewModels only.

## Logging

Use Microsoft.Extensions.Logging with NLog.
Do not log screenshot contents or sensitive user data.

## User Experience

Professional, lightweight, transparent, trustworthy. Avoid technical jargon.
Use: Activity, Timeline, Session, Screenshot, Application, Diary.
Avoid: Tracking target, Monitoring victim, Surveillance, Spy mode, gamer-only terminology.

## Testing

- Unit tests for domain logic
- Integration tests for repositories
- UI tests when practical
- Business rules must be testable without WinUI

## Copilot Guidance

- Favor clarity over cleverness and composition over inheritance.
- Keep classes small and focused.
- Maintain strict separation between UI and infrastructure.
