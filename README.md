<div align="center">

<img src="Assets/logo.png" alt="ChargeAlert logo" width="120" />

# ChargeAlert 🔋

**Stay Charged. Stay Informed.**

A lightweight Windows system-tray app for battery monitoring, customizable charging alerts, and low-battery reminders.

![Version](https://img.shields.io/badge/version-1.1.0-blue) ![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D6) ![Framework](https://img.shields.io/badge/.NET-8.0-512BD4)

</div>

## Overview

ChargeAlert helps laptop users keep an eye on their battery without constantly checking the taskbar. Set a charging alert at 80%, 90%, or 100%, get a notification when your battery runs low, and see your battery percentage in the system tray.

> **Note:** ChargeAlert sends notifications; it does **not** limit or stop charging at the selected percentage.

## Features

- **Custom charge-level alerts:** Notify at 80%, 90%, or 100% battery.
- **Full-charge popup:** Remains visible until you unplug, dismiss it, or snooze for 10 minutes.
- **Charging duration:** See how long your laptop has been plugged in.
- **Repeat reminders:** Choose 5, 10, 15, or 30 minutes, or disable reminders.
- **Low-battery alerts:** Choose 10%, 15%, 20%, 30%, or off. Alerts close when charging begins.
- **Charger connection notices:** Optional plug/unplug notifications.
- **Live tray indicator:** Show battery percentage with charging/low-battery colors, or use the app logo.
- **Fullscreen-aware notifications:** Stay quiet during fullscreen apps and games.
- **Start with Windows:** Optional automatic startup.
- **Custom appearance:** Configure popup placement and brief-notification duration.
- **Uninstall controls:** Uninstall from the About screen, with an option to retain settings.

## Download and install

Open the repository's **Releases** page and download one of the available builds:

| File | Use case |
| --- | --- |
| `ChargeAlertSetup.exe` | Recommended Windows installer, if included in the release |
| `ChargeAlert.exe` | Portable version, if included in the release |

For the installer, run `ChargeAlertSetup.exe` and follow the prompts. For the portable version, place `ChargeAlert.exe` in a folder you control and launch it directly.

**Requirements:** Windows 10 or 11 on a compatible battery-powered device. Self-contained release builds include the .NET runtime; building from source requires the .NET 8 SDK.

## How to use

1. Launch ChargeAlert. It runs in the Windows system tray.
2. Double-click the tray icon, or right-click it and choose **Settings**.
3. Set your preferred charging threshold and low-battery warning level.
4. Adjust reminders, notification preferences, and appearance as desired.
5. Keep ChargeAlert running to receive alerts.

## Settings

| Tab | Options |
| --- | --- |
| General | Startup, sounds, plug/unplug notices, fullscreen quiet mode, status and test popup |
| Alerts | Charging threshold, low-battery threshold, reminder interval |
| Appearance | Popup position, short-notice duration, tray icon style |
| About | Settings folder, reset options, uninstall |

Settings are stored in `%AppData%\ChargeAlert\settings.json`.

## Build from source

Requires Windows and the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

```powershell
dotnet restore ChargeAlert.csproj
dotnet run --project ChargeAlert.csproj
```

Publish a self-contained, single-file Windows x64 executable:

```powershell
dotnet publish ChargeAlert.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `bin\Release\net8.0-windows\win-x64\publish\ChargeAlert.exe`

A Windows setup installer can be built separately using an installer tool such as Inno Setup; the source project alone does not generate `ChargeAlertSetup.exe`.

## Uninstall

Open **Settings → About → Uninstall ChargeAlert**. ChargeAlert can remove its startup entry and settings (unless you choose to keep them). If installed using a setup program, its installer-managed uninstaller is used instead.

## Technology

- C#
- .NET 8
- WPF (Windows Presentation Foundation)
- Windows Forms integration for the system tray

## Feedback and issues

Found a bug or have an idea? Open an issue in this repository with your Windows version, ChargeAlert version, steps to reproduce, and relevant screenshots or error messages.

## License

No license is included in the uploaded project. Add a `LICENSE` file if you want to specify terms for reuse and contributions.

---

<div align="center"><strong>ChargeAlert — Stay Charged. Stay Informed.</strong></div>
