# ProcessDiary - Implementation Plan

Feature specifications and design details. Standing rules live in `copilot-instructions.md`.

## 1. Activity Tracking

Track per session:

- Foreground process name
- Executable name
- Window title
- Start timestamp, end timestamp, duration

Example:

```text
Visual Studio
09:00 -> 11:15

Counter-Strike 2
11:18 -> 13:40
```

Rules:

- When the foreground application changes, a running automated recording ends the previous session and starts a new one for the new process.
- Do not record explorer.exe or your own application.

## 2. Screenshot Capture

Triggers:

- Fixed interval (for example every 5 minutes or 30 seconds)
- Activity change
- Manual trigger

Modes:

- Active window capture
- Whole desktop capture

Rules:

- Every screenshot is linked to an activity session.
- Filename contains process name (for example `chrome` for chrome.exe), timestamp (for example `2024-06-01T14-30-00`) and extension (fixed `.png` for now).
- Prefer Windows Graphics Capture APIs; fall back to Win32 capture only when necessary.

```csharp
public interface IScreenshotService
{
	Task<ScreenshotResult> CaptureAsync(
		CancellationToken cancellationToken);
}
```

## 3. Process Detection

```csharp
public interface IForegroundWindowService
{
	ForegroundWindowInfo GetCurrent();
}
```

Implementation may use GetForegroundWindow, GetWindowText, GetWindowThreadProcessId.

## 4. Timeline

Show application changes, time spent, and screenshots.

## 5. Statistics and Reports

- Time per application
- Time per category

Filters:

- Day
- Week
- Since start of recording
- Since start of ProcessDiary recording
- Last 24 hours
- Between two date times

Reports are always the same; only the filter changes.

## 6. Recording Indicator

Surfaces: tray icon, overlay icon (must itself not be captured in screenshots), status badge, main window indicator.

States: Recording, Paused, Disabled, Error.

## 7. Storage

- SQLite database in local app data by default.
- Screenshots in `%USERPROFILE%\Pictures\ProcessDiary\YYYY\MM\DD\`.
- Screenshots folder is changeable in settings.
- Normalized tables.
- Handle missing screenshot files gracefully.

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

## 8. Privacy Features to Implement

- Pause recording
- Delete all data
- Delete selected screenshots
- Delete selected sessions
- Automatic deletion of old data

## 9. Logging Events

startup, shutdown, capture events, session transitions, storage failures.

## 10. Future Features (design for, do not implement)

- Process blacklist
- Activity categories
- Tags
- Search
- Exports
- PDF reports
- Timeline analytics
- Optional cloud sync (only if explicitly requested)
