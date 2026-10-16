using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace ChargeAlert
{
    enum PopupKind { None, Full, Low, Toast }

    sealed class BatteryMonitor : IDisposable
    {
        static readonly Color Green = Color.FromRgb(52, 199, 89);
        static readonly Color Red = Color.FromRgb(255, 69, 58);
        static readonly Color Blue = Color.FromRgb(10, 132, 255);
        static readonly Color Amber = Color.FromRgb(255, 159, 10);

        readonly Settings _s = Settings.Load();
        readonly WinForms.NotifyIcon _tray;
        readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(3) };

        PopupWindow? _popup;
        PopupKind _popupKind = PopupKind.None;

        bool _firstTick = true, _wasOnline, _noBatteryNotified;
        bool _fullShown, _lowShown;
        DateTime? _plugStart, _nextFullAlert;
        int _lastPct = -1;
        bool _lastOnline, _lastLow;

        public BatteryMonitor()
        {
            _tray = BuildTray();

            _timer.Tick += (_, _) => Tick();
            _timer.Start();

            // react instantly when the charger is plugged / unplugged
            Microsoft.Win32.SystemEvents.PowerModeChanged += (_, _) =>
                Application.Current.Dispatcher.BeginInvoke(Tick);

            Tick();
        }

        // ---------------- core logic ----------------

        void Tick()
        {
            var ps = WinForms.SystemInformation.PowerStatus;

            if (ps.BatteryChargeStatus.HasFlag(WinForms.BatteryChargeStatus.NoSystemBattery))
            {
                _tray.Text = "ChargeAlert - no battery detected";
                if (!_noBatteryNotified)
                {
                    _noBatteryNotified = true;
                    Show(PopupKind.Toast, "🖥️", "No battery found",
                         "ChargeAlert is made for laptops, so it has nothing to watch here.", 100, Blue, false, false);
                }
                return;
            }

            int pct = ps.BatteryLifePercent <= 1 ? (int)Math.Round(ps.BatteryLifePercent * 100) : 0;
            bool online = ps.PowerLineStatus == WinForms.PowerLineStatus.Online;
            bool quiet = _s.QuietInFullscreen && Native.FullscreenAppActive();

            // ----- plug / unplug transitions -----
            if (_firstTick)
            {
                _firstTick = false;
                _wasOnline = online;
                if (online) _plugStart = DateTime.Now;
            }
            else if (online != _wasOnline)
            {
                _wasOnline = online;
                if (online)
                {
                    _plugStart = DateTime.Now;
                    _lowShown = false;
                    if (_popupKind == PopupKind.Low) ClosePopup();
                    if (_s.PlugToasts && !quiet)
                        Show(PopupKind.Toast, "🔌", "Charger connected", $"{pct}% and charging", pct, Green, false, false);
                }
                else
                {
                    _plugStart = null;
                    _fullShown = false;
                    _nextFullAlert = null;
                    // the full-charge popup goes away as soon as the charger is removed
                    if (_popupKind == PopupKind.Full) ClosePopup();
                    if (_s.PlugToasts && !quiet)
                        Show(PopupKind.Toast, "🔋", "Charger unplugged", $"{pct}%{RemainingText(ps)}", pct, Blue, false, false);
                }
            }

            // ----- full-charge alert -----
            if (online && pct >= _s.FullAt)
            {
                if (!_fullShown && !quiet)
                {
                    _fullShown = true;
                    ShowFull(pct);
                }
                else if (_fullShown && _nextFullAlert != null && DateTime.Now >= _nextFullAlert
                         && _popupKind != PopupKind.Full && !quiet)
                {
                    ShowFull(pct);
                }
            }
            else if (online && pct <= _s.FullAt - 5)
            {
                _fullShown = false; // allow another alert next time it climbs back up
                _nextFullAlert = null;
            }

            // ----- low-battery alert -----
            if (!online && _s.LowAt > 0)
            {
                if (pct <= _s.LowAt && !_lowShown && !quiet)
                {
                    _lowShown = true;
                    Show(PopupKind.Low, "🪫", "Battery low",
                         $"{pct}% left{RemainingText(ps)}. Plug in your charger.", pct, Red, true, false);
                }
                else if (pct > _s.LowAt + 5) _lowShown = false;
            }

            UpdateTray(pct, online, ps);
        }

        void ShowFull(int pct)
        {
            string took = "";
            if (_plugStart is { } start)
            {
                var d = DateTime.Now - start;
                if (d.TotalMinutes >= 1) took = $" (plugged in for {Dur(d)})";
            }
            Show(PopupKind.Full, "🔋", pct >= 100 ? "Battery full" : $"Battery at {pct}%",
                 $"Charged to {pct}%{took}. You can unplug the charger now.", pct, Green, true, true);
        }

        void OnDismissed(PopupKind kind)
        {
            if (kind == PopupKind.Full && _fullShown)
                _nextFullAlert = _s.RemindMinutes > 0 ? DateTime.Now.AddMinutes(_s.RemindMinutes) : null;
        }

        void OnSnoozed(PopupKind kind)
        {
            if (kind == PopupKind.Full && _fullShown)
                _nextFullAlert = DateTime.Now.AddMinutes(10);
        }

        static string RemainingText(WinForms.PowerStatus ps) =>
            ps.BatteryLifeRemaining > 0 ? $" (~{Dur(TimeSpan.FromSeconds(ps.BatteryLifeRemaining))} left)" : "";

        static string Dur(TimeSpan t) =>
            t.TotalHours >= 1 ? $"{(int)t.TotalHours}h {t.Minutes}m" : $"{Math.Max(1, (int)t.TotalMinutes)}m";

        // ---------------- popups ----------------

        void Show(PopupKind kind, string icon, string title, string message, int pct, Color accent,
                  bool persistent, bool snooze)
        {
            ClosePopup();

            var p = new PopupWindow(icon, title, message, pct, accent, persistent, snooze, _s.PopupBottomRight);
            p.Dismissed += () => OnDismissed(kind);
            p.Snoozed += () => OnSnoozed(kind);
            p.Closed += (_, _) =>
            {
                if (ReferenceEquals(_popup, p)) { _popup = null; _popupKind = PopupKind.None; }
            };

            _popup = p;
            _popupKind = kind;
            p.Show();

            if (_s.Sound && kind != PopupKind.Toast)
            {
                if (kind == PopupKind.Full) System.Media.SystemSounds.Asterisk.Play();
                else System.Media.SystemSounds.Exclamation.Play();
            }
        }

        void ClosePopup()
        {
            var old = _popup;
            _popup = null;
            _popupKind = PopupKind.None;
            old?.CloseAnimated();
        }

        // ---------------- tray ----------------

        void UpdateTray(int pct, bool online, WinForms.PowerStatus ps)
        {
            bool low = !online && _s.LowAt > 0 && pct <= _s.LowAt;

            string state = online ? (pct >= 100 ? "fully charged" : "charging") : "on battery" + RemainingText(ps);
            _tray.Text = Truncate($"Battery {pct}% - {state}", 63);

            if (pct == _lastPct && online == _lastOnline && low == _lastLow) return;
            _lastPct = pct; _lastOnline = online; _lastLow = low;

            var old = _tray.Icon;
            _tray.Icon = TrayIcons.Make(pct, online, low);
            old?.Dispose();
        }

        static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max);

        WinForms.NotifyIcon BuildTray()
        {
            var tray = new WinForms.NotifyIcon { Icon = System.Drawing.SystemIcons.Application, Text = "ChargeAlert", Visible = true };
            var menu = new WinForms.ContextMenuStrip();

            menu.Items.Add("Show battery status", null, (_, _) => Application.Current.Dispatcher.Invoke(ShowStatus));
            menu.Items.Add("Test full-charge popup", null, (_, _) => Application.Current.Dispatcher.Invoke(() =>
            {
                _fullShown = true;
                ShowFull(Math.Max(_lastPct, 0));
            }));
            menu.Items.Add(new WinForms.ToolStripSeparator());

            menu.Items.Add(Choice("Full-charge alert at", new[] { 80, 90, 100 },
                v => $"{v}%", () => _s.FullAt, v => { _s.FullAt = v; _fullShown = false; }));

            menu.Items.Add(Choice("Low-battery alert at", new[] { 0, 10, 15, 20, 30 },
                v => v == 0 ? "Off" : $"{v}%", () => _s.LowAt, v => { _s.LowAt = v; _lowShown = false; }));

            menu.Items.Add(Choice("Remind me while still plugged in", new[] { 0, 5, 10, 15, 30 },
                v => v == 0 ? "Off" : $"every {v} min", () => _s.RemindMinutes, v => _s.RemindMinutes = v));

            menu.Items.Add(Choice("Popup position", new[] { 0, 1 },
                v => v == 0 ? "Top center" : "Bottom right", () => _s.PopupBottomRight ? 1 : 0, v => _s.PopupBottomRight = v == 1));

            menu.Items.Add(new WinForms.ToolStripSeparator());
            menu.Items.Add(Toggle("Play sound", () => _s.Sound, v => _s.Sound = v));
            menu.Items.Add(Toggle("Notify when charger plugged / unplugged", () => _s.PlugToasts, v => _s.PlugToasts = v));
            menu.Items.Add(Toggle("Stay quiet during fullscreen apps", () => _s.QuietInFullscreen, v => _s.QuietInFullscreen = v));

            var startup = new WinForms.ToolStripMenuItem("Launch at startup") { CheckOnClick = true, Checked = IsStartupEnabled() };
            startup.CheckedChanged += (_, _) => SetStartup(startup.Checked);
            menu.Items.Add(startup);

            menu.Items.Add(new WinForms.ToolStripSeparator());
            menu.Items.Add("Exit", null, (_, _) =>
            {
                tray.Visible = false;
                tray.Dispose();
                Application.Current.Shutdown();
            });

            tray.ContextMenuStrip = menu;
            tray.MouseClick += (_, e) =>
            {
                if (e.Button == WinForms.MouseButtons.Left) Application.Current.Dispatcher.Invoke(ShowStatus);
            };
            return tray;
        }

        void ShowStatus()
        {
            var ps = WinForms.SystemInformation.PowerStatus;
            if (ps.BatteryChargeStatus.HasFlag(WinForms.BatteryChargeStatus.NoSystemBattery)) return;

            int pct = ps.BatteryLifePercent <= 1 ? (int)Math.Round(ps.BatteryLifePercent * 100) : 0;
            bool online = ps.PowerLineStatus == WinForms.PowerLineStatus.Online;
            string msg = online ? (pct >= 100 ? "Fully charged, plugged in" : "Charging") : "On battery" + RemainingText(ps);
            Show(PopupKind.Toast, online ? "🔌" : "🔋", $"Battery {pct}%", msg, pct, online ? Green : (pct <= 20 ? Red : Blue), false, false);
        }

        WinForms.ToolStripMenuItem Choice(string title, int[] values, Func<int, string> label, Func<int> get, Action<int> set)
        {
            var root = new WinForms.ToolStripMenuItem(title);
            foreach (var v in values)
            {
                int val = v;
                var item = new WinForms.ToolStripMenuItem(label(val)) { Checked = get() == val };
                item.Click += (_, _) =>
                {
                    set(val);
                    _s.Save();
                    foreach (WinForms.ToolStripMenuItem o in root.DropDownItems) o.Checked = o == item;
                };
                root.DropDownItems.Add(item);
            }
            return root;
        }

        WinForms.ToolStripMenuItem Toggle(string title, Func<bool> get, Action<bool> set)
        {
            var item = new WinForms.ToolStripMenuItem(title) { CheckOnClick = true, Checked = get() };
            item.CheckedChanged += (_, _) => { set(item.Checked); _s.Save(); };
            return item;
        }

        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        static bool IsStartupEnabled()
        {
            using var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKey);
            return k?.GetValue("ChargeAlert") != null;
        }
        static void SetStartup(bool on)
        {
            using var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKey, true);
            if (k == null) return;
            if (on) k.SetValue("ChargeAlert", "\"" + Environment.ProcessPath + "\"");
            else k.DeleteValue("ChargeAlert", false);
        }

        public void Dispose()
        {
            _timer.Stop();
            _tray.Visible = false;
            _tray.Dispose();
        }
    }
}
