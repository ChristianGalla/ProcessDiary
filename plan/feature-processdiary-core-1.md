---
goal: Implement ProcessDiary core features (activity tracking, screenshots, timeline, reports, recording indicator, storage, privacy controls)
version: 1.2
date_created: 2026-10-17
last_updated: 2026-10-17
owner: ProcessDiary maintainers
status: 'Planned'
tags: [feature, architecture, winui, sqlite, privacy]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

ProcessDiary is a Windows 11 WinUI 3 app that builds a visual diary of computer activity (foreground application sessions plus screenshots). Standing rules live in `.github/copilot-instructions.md`.

## 1. Requirements & Constraints

- **REQ-001**: Track per session: process name, executable, window title, start, end, duration.
- **REQ-002**: On foreground change, a running recording ends the previous session and starts a new one; ignore `explorer.exe` and ProcessDiary itself.
- **REQ-003**: Screenshot triggers: interval, activity change, manual. Modes: active window, whole desktop. Each screenshot links to a session.
- **REQ-004**: Screenshot filename: `{processName}_{yyyy-MM-ddTHH-mm-ss}.png`.
- **REQ-005**: Timeline and statistics (time per application/category) with filters: day, week, since start of recording, since start of ProcessDiary recording, last 24 hours, between two date times. Reports are identical; only the filter changes.
- **REQ-006**: Recording indicator (tray, overlay, status badge, main window); states Recording, Paused, Disabled, Error.
- **REQ-007**: SQLite in local app data; screenshots in `%USERPROFILE%\Pictures\ProcessDiary\YYYY\MM\DD\` (configurable); normalized tables; handle missing files gracefully.
- **REQ-008**: Log startup, shutdown, capture events, session transitions, storage failures.
- **SEC-001**: Local-first, no network transmission, no keystroke/clipboard/password capture, no hidden recording, no undocumented telemetry.
- **SEC-002**: Overlay icon must not appear in screenshots; never log screenshot contents or sensitive data.
- **PRI-001**: Pause recording, delete all data, delete selected screenshots/sessions, automatic deletion of old data.
- **CON-001**: Windows 11, WinUI 3, Windows App SDK, MSIX, .NET 8. No paid or trial-restricted libraries.
- **CON-002**: Future features (blacklist, categories, tags, search, exports, PDF reports, timeline analytics, optional cloud sync) are designed for, not implemented.
- **GUD-001**: Layers UI / Application / Domain / Infrastructure; Domain has no WinUI reference; Win32 isolated behind interfaces.
- **GUD-002**: async/await, DI, immutable DTOs, cancellation tokens, nullable reference types; no static services, service locators, blocking calls, or async void (except UI events).
- **PAT-001**: MVVM with CommunityToolkit.Mvvm; views talk only to view models.
- **PAT-002**: Logging with Microsoft.Extensions.Logging and NLog.

## 2. Implementation Steps

### Implementation Phase 1 - Domain and Application

- GOAL-001: Define entities, interfaces and use cases without WinUI/Win32 dependencies.

| Task     | Description | Completed | Date |
| -------- | ----------- | --------- | ---- |
| TASK-001 | Create entities `ProcessSession` (Guid Id, ProcessName, ExecutablePath, WindowTitle, DateTimeOffset StartedAt, EndedAt, TimeSpan Duration), `Screenshot` (Guid Id, Guid ProcessSessionId, FilePath, DateTimeOffset CapturedAt, int Width, int Height), `ApplicationMetadata` (Guid Id, ProcessName, DisplayName, Category). | | |
| TASK-002 | Create `IForegroundWindowService { ForegroundWindowInfo GetCurrent(); }`, `IScreenshotService { Task<ScreenshotResult> CaptureAsync(CancellationToken cancellationToken); }`, repository interfaces, `RecordingState` enum. | | |
| TASK-003 | Create pure session-transition logic (end/start session, ignore explorer.exe and own process). | | |
| TASK-004 | Create `ReportFilter` and report use cases (time per application, time per category). | | |

### Implementation Phase 2 - Infrastructure

- GOAL-002: Implement storage, process detection and capture.

| Task     | Description | Completed | Date |
| -------- | ----------- | --------- | ---- |
| TASK-005 | Implement SQLite persistence with normalized tables (EF Core or Dapper). | | |
| TASK-006 | Implement `IForegroundWindowService` with GetForegroundWindow, GetWindowText, GetWindowThreadProcessId (P/Invoke via CsWin32). | | |
| TASK-007 | Implement `IScreenshotService` with Windows Graphics Capture (Vortice.Windows and SkiaSharp); Win32 fallback only if necessary. | | |
| TASK-008 | Implement screenshot file storage, naming, configurable folder, missing-file handling. | | |
| TASK-009 | Implement capture scheduler (interval, activity change, manual) honoring pause and cancellation. | | |
| TASK-010 | Configure NLog and log events per REQ-008. | | |

### Implementation Phase 3 - UI and privacy

- GOAL-003: Deliver MVVM screens, indicator and privacy controls.

| Task     | Description | Completed | Date |
| -------- | ----------- | --------- | ---- |
| TASK-011 | Set up Hosting/DI composition root and navigation shell. | | |
| TASK-012 | Implement Timeline and Statistics views and view models. | | |
| TASK-013 | Implement recording indicator (tray via H.NotifyIcon.WinUI; overlay excluded from captures; badge; window). | | |
| TASK-014 | Implement Settings view (folder, interval, capture mode, retention). | | |
| TASK-015 | Implement pause, delete all, delete selected screenshots/sessions, automatic retention job. | | |

## 3. Alternatives

- **ALT-001**: Built-in `Windows.Graphics.Capture` and imaging APIs with no extra dependency, instead of Vortice.Windows (MIT) / SkiaSharp (MIT).
- **ALT-002**: Hand-written `DllImport` instead of Microsoft.Windows.CsWin32 (MIT).
- **ALT-003**: Custom Shell_NotifyIcon interop instead of H.NotifyIcon.WinUI (MIT).
- **ALT-004**: Dapper vs EF Core for SQLite; decide in TASK-005.

## 4. Dependencies

- **DEP-001**: CommunityToolkit.Mvvm, Microsoft.Extensions.Hosting/DependencyInjection/Logging, NLog, SQLite provider, Windows App SDK.
- **DEP-002**: Vortice.Windows (MIT) - approved by user.
- **DEP-003**: SkiaSharp (MIT) - approved by user.
- **DEP-004**: Microsoft.Windows.CsWin32 (MIT) - approved by user.
- **DEP-005**: H.NotifyIcon.WinUI (MIT) - approved by user.

## 5. Files

- **FILE-001**: Domain project - entities, interfaces, transition logic.
- **FILE-002**: Application project - use cases, DTOs.
- **FILE-003**: Infrastructure project - SQLite, Win32, capture, file system.
- **FILE-004**: WinUI app project - views, view models, DI, tray/overlay.
- **FILE-005**: Test project - unit and integration tests.

## 6. Testing

- **TEST-001**: Unit tests for session transitions and ignored processes.
- **TEST-002**: Unit tests for report filters and aggregation.
- **TEST-003**: Unit tests for screenshot filename generation.
- **TEST-004**: Integration tests for SQLite repositories, deletion and retention.
- **TEST-005**: Missing screenshot file handling test.
- **TEST-006**: Overlay icon absent from captures; no network calls by default.

## 7. Risks & Assumptions

- **RISK-001**: Windows Graphics Capture may not capture some protected windows.
- **RISK-002**: Window titles may be sensitive; keep out of logs.
- **ASSUMPTION-001**: .NET 8 and Windows 11 only.

## 8. Related Specifications / Further Reading

- [Windows Graphics Capture](https://learn.microsoft.com/windows/uwp/audio-video-camera/screen-capture)
- `.github/copilot-instructions.md`
