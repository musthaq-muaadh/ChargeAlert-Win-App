# ChargeAlert

A small tray app for laptops. When the battery reaches full, a popup slides in:
**"Battery full - you can unplug the charger now."** It stays on screen until you
pull the charger out, then it disappears by itself.

## Features
- **Full-charge popup** that auto-closes the moment you unplug (or press Dismiss / Snooze 10 min).
- Shows how long the laptop was plugged in.
- **Re-reminds** every 5/10/15/30 min if you leave it plugged in (or turn it off).
- **Charge limit**: alert at 80%, 90% or 100% to protect battery health.
- **Low-battery popup** (10/15/20/30% or off) that closes when you plug in.
- Small toasts when the charger is plugged / unplugged, with time remaining.
- **Live tray icon** showing the battery % (green while charging, red when low).
  Left-click it for a status popup; hover for a tooltip.
- Sound on alerts, popup position (top center or bottom right),
  stays quiet while a fullscreen app/game is in front, launch at startup.
- Settings saved to `%AppData%\ChargeAlert\settings.json`.

Right-click the tray icon for all options, including "Test full-charge popup".

## Requirements
Windows 10/11 laptop and the .NET 8 SDK.

## Run
    dotnet run

## Build a single .exe
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

Result: `bin\Release\net8.0-windows\win-x64\publish\ChargeAlert.exe`
