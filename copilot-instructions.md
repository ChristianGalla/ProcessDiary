# ProcessDiary - GitHub Copilot Instructions

## Project Overview

ProcessDiary is a Windows 11 desktop application that helps users build a visual diary of their computer activities.

The application periodically captures screenshots and records foreground application activity to create a searchable timeline of work, gaming, learning, and general computer usage.

ProcessDiary is intentionally designed as a transparent activity journaling tool, not a surveillance or monitoring tool.

Core principles:

- User-controlled
- Privacy-first
- Local-first
- Visible recording status
- No hidden background monitoring
- No keystroke logging
- No network transmission by default

---

## Technology Stack

Target platform:

- Windows 11
- .NET 9+
- WinUI 3
- Windows App SDK
- MSIX packaging

Preferred libraries:

- CommunityToolkit.Mvvm
- CommunityToolkit.WinUI
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- SQLite via EF Core or Dapper

Avoid unnecessary dependencies.

---

## Architecture

Use clean architecture principles.

Layers:

### UI

WinUI 3 Views and ViewModels.

Responsibilities:

- Settings
- Timeline visualization
- Screenshot browser
- Reports
- Recording status

Never place business logic in code-behind.

### Application

Contains:

- Use cases
- Services
- Commands
- DTOs

### Domain

Contains:

- Entities
- Business rules
- Interfaces

Domain must not reference WinUI.

### Infrastructure

Contains:

- SQLite storage
- Windows APIs
- Screenshot capture
- Process detection
- File system access

---

## Primary Features

### Activity Tracking

Track:

- Foreground process name
- Executable name
- Window title
- Start timestamp
- End timestamp
- Duration

Example:

```text
Visual Studio
09:00 -> 11:15

Counter-Strike 2
11:18 -> 13:40
```

### Screenshot Capture

Capture screenshots on:

- Fixed interval (for example every 5 minutes or 30 seconds)
- Activity change
- Manual trigger

Support:

- Active window capture
- Whole desktop capture

Screenshots must be linked to an activity session.

The filename should include:
- process name (for example chrome for chrome.exe)
- timestamp (for example 2024-06-01T14-30-00)
- file extension (for now fixed .png)

### Timeline

Generate timelines showing:

- Application changes
- Time spent
- Screenshots

### Statistics

Examples:

- Time per application
- Time per category

Filter by:
- Day
- Week
- Since start of recording
- Since start ProcessDiary of recording
- Last 24 hours
- Between two date times

Reports always are the same, only the filter can change.

### Additional requirements
- If the foreground application is changed, the a running automated recording should end the previous session and continue with a new one for the new process.
- Don't record explorer.exe

---

## Privacy Requirements

Privacy is a first-class feature.

Always:

- Store data locally
- Make recording state visible
- Allow pausing recording
- Allow deleting all data
- Allow deleting selected screenshots
- Allow deleting selected sessions
- Allow automatic deletion of old data

Never implement:

- Keystroke logging
- Password capture
- Clipboard surveillance
- Hidden recording modes
- Undocumented telemetry

If a feature could be privacy-invasive, favor user control and transparency.

---

## Recording Indicator

The application must always expose recording state.

Examples:

- Tray icon
- Overlay icon (must iteslf not be captured in screenshots)
- Status badge
- Main window indicator

States:

- Recording
- Paused
- Disabled
- Error

Recording should never be invisible.

---

## Storage

Use SQLite.

The sql file should be stored in local app data by default and the screenshots in the default image location in a ProcessDiary folder.
For the screenshots folder, use the following path:
```text
%USERPROFILE%\Pictures\ProcessDiary\YYYY\MM\DD\
```

The user must be able to change the screenshots folder in settings.

Tables should be normalized.

The application should handle if the screenshot file is missing.

Preferred entities:

### ProcessSession

```csharp
Guid Id
string ProcessName
string ExecutablePath
string WindowTitle
DateTimeOffset StartedAt
DateTimeOffset EndedAt
TimeSpan Duration
```

### Screenshot

```csharp
Guid Id
Guid ProcessSessionId
string FilePath
DateTimeOffset CapturedAt
int Width
int Height
```

### ApplicationMetadata

```csharp
Guid Id
string ProcessName
string DisplayName
string Category
```

---

## Coding Guidelines

Prefer:

- async/await
- dependency injection
- immutable DTOs
- cancellation tokens
- nullable reference types

Avoid:

- static service classes
- service locators
- blocking calls
- async void except UI events

---

## MVVM Rules

All screens must use MVVM.

Use:

- ObservableObject
- RelayCommand
- AsyncRelayCommand

Never access repositories directly from Views.

Views communicate through ViewModels only.

---

## Logging

Use Microsoft.Extensions.Logging and nLog.

Log:

- startup
- shutdown
- capture events
- session transitions
- storage failures

Do not log:

- screenshot image contents
- sensitive user data

---

## Screenshot Capturing

Prefer modern Windows Graphics Capture APIs.

Fallback to Win32 capture only when necessary.

Design capture behind abstractions:

```csharp
public interface IScreenshotService
{
    Task<ScreenshotResult> CaptureAsync(
        CancellationToken cancellationToken);
}
```

Never directly couple ViewModels to Windows capture APIs.

---

## Process Detection

Process tracking should be implemented behind:

```csharp
public interface IForegroundWindowService
{
    ForegroundWindowInfo GetCurrent();
}
```

The implementation may use:

- GetForegroundWindow
- GetWindowText
- GetWindowThreadProcessId

Keep Win32 code isolated.

---

## User Experience

The application should feel:

- Professional
- Lightweight
- Transparent
- Trustworthy

Avoid:

- Gamer-only terminology
- Surveillance terminology
- Technical jargon in the UI

Use language such as:

- Activity
- Timeline
- Session
- Screenshot
- Application
- Diary

Avoid:

- Tracking target
- Monitoring victim
- Surveillance
- Spy mode

---

## Testing

Create:

- Unit tests for domain logic
- Integration tests for repositories
- UI tests when practical

Business rules should be testable without WinUI.

---

## Future Features

Design for future support of:

- Blacklist of processes that should not be recorded
- Activity categories
- Tags
- Search
- Exports
- PDF reports
- Timeline analytics
- Optional cloud sync

Do not implement cloud features unless explicitly requested.

Local-first remains the default architecture.

---

## Copilot Guidance

When generating code:

- Favor clarity over cleverness.
- Favor composition over inheritance.
- Keep classes small and focused.
- Use interfaces for platform-specific services.
- Maintain strict separation between UI and infrastructure.
- Assume Windows 11 as the only supported platform.
- Generate production-quality code, not samples or prototypes.