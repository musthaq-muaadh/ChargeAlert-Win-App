using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChargeAlert
{
    public partial class SettingsWindow : Window
    {
        static readonly Brush Dim = new SolidColorBrush(Color.FromRgb(0x8A, 0x94, 0x8E));
        static readonly Brush Danger = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x60));

        readonly Settings _s;
        readonly BatteryMonitor _monitor;
        readonly Dictionary<string, StackPanel> _pages = new();
        int _groupCounter;
        string _current = "General";

        internal SettingsWindow(Settings settings, BatteryMonitor monitor)
        {
            InitializeComponent();
            _s = settings;
            _monitor = monitor;
            VersionText.Text = "Version " + (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.1.0");
            BuildPages();
        }

        public void ShowPage(string name)
        {
            foreach (var child in Nav.Children)
                if (child is RadioButton rb && (rb.Content as string) == name) { rb.IsChecked = true; return; }
        }

        // ---------- pages ----------

        void BuildPages()
        {
            Nav.Children.Clear();
            PagesHost.Children.Clear();
            _pages.Clear();

            BuildGeneral();
            BuildAlerts();
            BuildAppearance();
            BuildAbout();

            if (!_pages.ContainsKey(_current)) _current = "General";
            Select(_current);
        }

        StackPanel NewPage(string name)
        {
            var page = new StackPanel { Visibility = Visibility.Collapsed };
            _pages[name] = page;
            PagesHost.Children.Add(page);

            var rb = new RadioButton
            {
                Content = name,
                GroupName = "nav",
                Style = (Style)FindResource("NavBtn"),
                IsChecked = name == _current
            };
            rb.Checked += (_, _) => Select(name);
            Nav.Children.Add(rb);
            return page;
        }

        void Select(string name)
        {
            _current = name;
            foreach (var kv in _pages)
                kv.Value.Visibility = kv.Key == name ? Visibility.Visible : Visibility.Collapsed;
            PageTitle.Text = name;
            Scroller.ScrollToTop();
        }

        void BuildGeneral()
        {
            var p = NewPage("General");

            Toggle(p, "Launch at startup", "Start ChargeAlert automatically when you sign in to Windows.",
                BatteryMonitor.IsStartupEnabled, BatteryMonitor.SetStartup, saveSettings: false);

            Toggle(p, "Play a sound", "A short sound with the full-charge and low-battery popups.",
                () => _s.Sound, v => _s.Sound = v);

            Toggle(p, "Charger plugged / unplugged notices", "A small notice each time the charger is connected or removed.",
                () => _s.PlugToasts, v => _s.PlugToasts = v);

            Toggle(p, "Stay quiet during fullscreen apps", "Hold alerts while a game or fullscreen app is in front.",
                () => _s.QuietInFullscreen, v => _s.QuietInFullscreen = v);

            var status = new Button { Content = "Show", Style = (Style)FindResource("ActionBtn") };
            status.Click += (_, _) => _monitor.ShowStatus();
            Row(p, "Battery status", "Show the current charge in a popup.", status, false);

            var test = new Button { Content = "Test popup", Style = (Style)FindResource("ActionBtn") };
            test.Click += (_, _) => _monitor.TestFullPopup();
            Row(p, "Preview the full-charge popup", "Closes when you press Dismiss, or when the charger is unplugged.", test, false);
        }

        void BuildAlerts()
        {
            var p = NewPage("Alerts");

            Choice(p, "Full-charge alert at", "80% or 90% is gentler on the battery than always charging to 100%.",
                new[] { ("80%", 80), ("90%", 90), ("100%", 100) },
                () => _s.FullAt, v => { _s.FullAt = v; _monitor.ResetAlerts(); });

            Choice(p, "Low-battery alert at", "Warns you to plug in. Closes when the charger is connected.",
                new[] { ("Off", 0), ("10%", 10), ("15%", 15), ("20%", 20), ("30%", 30) },
                () => _s.LowAt, v => { _s.LowAt = v; _monitor.ResetAlerts(); });

            Choice(p, "Remind me while still plugged in", "Shows the popup again if you leave the charger in after dismissing it.",
                new[] { ("Off", 0), ("5 min", 5), ("10 min", 10), ("15 min", 15), ("30 min", 30) },
                () => _s.RemindMinutes, v => _s.RemindMinutes = v);
        }

        void BuildAppearance()
        {
            var p = NewPage("Appearance");

            Choice(p, "Popup position", "Where the popups slide in.",
                new[] { ("Top center", 0), ("Bottom right", 1) },
                () => _s.PopupBottomRight ? 1 : 0, v => _s.PopupBottomRight = v == 1);

            Choice(p, "Short notices stay for", "Plug / unplug and status notices.",
                new[] { ("3 sec", 3), ("4 sec", 4), ("6 sec", 6), ("10 sec", 10) },
                () => _s.ToastSeconds, v => _s.ToastSeconds = v);

            Choice(p, "Tray icon shows", "The icon next to the clock.",
                new[] { ("Battery %", 0), ("App logo", 1) },
                () => _s.TrayIconStyle, v => _s.TrayIconStyle = v);
        }

        void BuildAbout()
        {
            var p = NewPage("About");

            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 22) };
            header.Children.Add(new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Assets/logo.png")),
                Width = 72, Height = 72
            });
            var t = new StackPanel { Margin = new Thickness(16, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            t.Children.Add(new TextBlock { Text = "ChargeAlert", Foreground = Brushes.White, FontSize = 20, FontWeight = FontWeights.SemiBold });
            t.Children.Add(new TextBlock { Text = VersionText.Text, Foreground = Dim, FontSize = 12 });
            t.Children.Add(new TextBlock { Text = "Tells you when your laptop is charged, then gets out of the way.", Foreground = Dim, FontSize = 12, Margin = new Thickness(0, 4, 0, 0) });
            header.Children.Add(t);
            p.Children.Add(header);

            var open = new Button { Content = "Open folder", Style = (Style)FindResource("ActionBtn") };
            open.Click += (_, _) =>
            {
                try { Process.Start(new ProcessStartInfo("explorer.exe", "\"" + Settings.Folder + "\"") { UseShellExecute = true }); } catch { }
            };
            Row(p, "Settings folder", "Where ChargeAlert keeps your settings.", open, false);

            var reset = new Button { Content = "Reset", Style = (Style)FindResource("ActionBtn") };
            reset.Click += (_, _) =>
            {
                var r = MessageBox.Show(this, "Reset every option to its default?", "ChargeAlert",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (r != MessageBoxResult.OK) return;
                _s.ResetOptions();
                _s.Save();
                _monitor.ApplySettings();
                BuildPages();
            };
            Row(p, "Reset options", "Put every option back to its default.", reset, false);

            // ----- uninstall -----
            p.Children.Add(new Border { Height = 1, Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x31, 0x2D)), Margin = new Thickness(0, 6, 0, 22) });
            p.Children.Add(new TextBlock { Text = "Uninstall", Foreground = Danger, FontSize = 15, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
            p.Children.Add(new TextBlock
            {
                Text = "Removes ChargeAlert completely: it stops starting with Windows, your settings and saved data are deleted, "
                     + "and the program files are removed. Nothing else on your PC is touched.",
                Foreground = Dim, FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 14)
            });

            var keep = new CheckBox { Style = (Style)FindResource("Switch"), IsChecked = false };
            Row(p, "Keep my settings", "Leave your settings in place in case you install ChargeAlert again.", keep, false);

            var folder = Uninstaller.InstallFolder;
            if (folder.Length > 0)
                p.Children.Add(new TextBlock { Text = "Program files: " + folder, Foreground = Dim, FontSize = 11, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 14) });

            var uninstall = new Button { Content = "Uninstall ChargeAlert…", Style = (Style)FindResource("DangerBtn"), HorizontalAlignment = HorizontalAlignment.Left };
            uninstall.Click += (_, _) => ConfirmUninstall(keep.IsChecked == true);
            p.Children.Add(uninstall);
        }

        void ConfirmUninstall(bool keepSettings)
        {
            var msg = "This will completely remove ChargeAlert from this PC:\n\n"
                    + "   •  stop it starting with Windows\n"
                    + (keepSettings ? "" : "   •  delete your settings and saved data\n")
                    + "   •  delete the program files\n\n"
                    + "This can't be undone. Continue?";

            var r = MessageBox.Show(this, msg, "Uninstall ChargeAlert", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (r != MessageBoxResult.Yes) return;

            Uninstaller.Run(removeSettings: !keepSettings);
        }

        // ---------- row builders ----------

        void Row(StackPanel page, string title, string? desc, FrameworkElement control, bool below)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
            var text = new StackPanel();
            text.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontSize = 13.5, FontWeight = FontWeights.SemiBold });
            if (!string.IsNullOrEmpty(desc))
                text.Children.Add(new TextBlock { Text = desc, Foreground = Dim, FontSize = 11.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) });

            if (below)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                control.Margin = new Thickness(0, 10, 0, 0);
                control.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetRow(control, 1);
            }
            else
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                text.Margin = new Thickness(0, 0, 18, 0);
                control.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(control, 1);
            }

            grid.Children.Add(text);
            grid.Children.Add(control);
            page.Children.Add(grid);
        }

        void Changed(Action set, bool save = true)
        {
            set();
            if (save) _s.Save();
            _monitor.ApplySettings();
        }

        void Toggle(StackPanel page, string title, string? desc, Func<bool> get, Action<bool> set, bool saveSettings = true)
        {
            var cb = new CheckBox { Style = (Style)FindResource("Switch"), IsChecked = get() };
            cb.Checked += (_, _) => Changed(() => set(true), saveSettings);
            cb.Unchecked += (_, _) => Changed(() => set(false), saveSettings);
            Row(page, title, desc, cb, false);
        }

        void Choice<T>(StackPanel page, string title, string? desc, (string label, T value)[] options,
                       Func<T> get, Action<T> set, bool below = false)
        {
            var group = "g" + _groupCounter++;
            var panel = new WrapPanel();
            var current = get();
            foreach (var (label, value) in options)
            {
                var v = value;
                var rb = new RadioButton
                {
                    Content = label,
                    GroupName = group,
                    Style = (Style)FindResource("SegBtn"),
                    IsChecked = EqualityComparer<T>.Default.Equals(current, v)
                };
                rb.Checked += (_, _) => Changed(() => set(v));
                panel.Children.Add(rb);
            }
            Row(page, title, desc, panel, below || options.Length >= 4);
        }
    }
}
