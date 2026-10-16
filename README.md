# ChargeAlert

A small, lightweight Windows tray app that tells you when your laptop battery is
full, so you can unplug the charger and keep your battery healthy.

When the battery reaches your chosen level, a popup slides in. It stays on screen
until you unplug the charger, then it disappears automatically.

## Features

- **Full-charge popup** that closes by itself when you unplug the charger
- **Charge limit alerts** at 80%, 90% or 100% to help protect battery health
- **Smart reminders** every 5/10/15/30 minutes if you leave the charger plugged in
- **Snooze** the alert for 10 minutes or dismiss it
- **Low-battery popup** at 10/15/20/30%, closes when you plug in
- **Plug/unplug notifications** with estimated time remaining
- **Live tray icon** showing the battery percentage (green when charging, red when low)
- Shows how long the laptop was plugged in
- Popup position: top center or bottom right
- Optional sound alerts
- Stays quiet while a fullscreen app or game is in front
- Launch at startup
- Settings saved automatically in `%AppData%\ChargeAlert\settings.json`

## Requirements

- Windows 10 or 11 (laptop)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Run

    dotnet run

## Build a single .exe

    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

The executable will be in `bin\Release\net8.0-windows\win-x64\publish\ChargeAlert.exe`.

## Usage

Right-click the tray icon to change settings or use **Test full-charge popup**.
Left-click the icon for a quick battery status.

## Tech

C# · WPF · .NET 8 · Windows Forms (tray icon)

## License

MIT
