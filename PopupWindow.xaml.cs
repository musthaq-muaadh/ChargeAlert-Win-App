using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace ChargeAlert
{
    public partial class PopupWindow : Window
    {
        readonly bool _bottomRight;
        bool _closing;

        /// <summary>User pressed Dismiss.</summary>
        public event Action? Dismissed;
        /// <summary>User pressed Snooze.</summary>
        public event Action? Snoozed;

        public PopupWindow(string icon, string title, string message, int percent, Color accent,
                           bool persistent, bool showSnooze, bool bottomRight, double toastSeconds = 4)
        {
            InitializeComponent();
            _bottomRight = bottomRight;

            IconText.Text = icon;
            TitleText.Text = title;
            MessageText.Text = message;
            LevelBar.Value = percent;
            LevelBar.Foreground = new SolidColorBrush(accent);
            IconCircle.Background = new SolidColorBrush(Color.FromArgb(0x33, accent.R, accent.G, accent.B));

            SnoozeBtn.Visibility = showSnooze ? Visibility.Visible : Visibility.Collapsed;
            if (!persistent) Buttons.Visibility = Visibility.Collapsed;

            Loaded += (_, _) => PlaceAndShow();

            if (!persistent)
            {
                var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Math.Max(1, toastSeconds)) };
                t.Tick += (_, _) => { t.Stop(); CloseAnimated(); };
                t.Start();
            }
        }

        void PlaceAndShow()
        {
            var screen = WinForms.Screen.PrimaryScreen ?? WinForms.Screen.AllScreens[0];
            var wa = screen.WorkingArea;
            var dpi = VisualTreeHelper.GetDpi(this);

            double wl = wa.Left / dpi.DpiScaleX, wt = wa.Top / dpi.DpiScaleY;
            double ww = wa.Width / dpi.DpiScaleX, wh = wa.Height / dpi.DpiScaleY;

            if (_bottomRight)
            {
                Left = wl + ww - Width;
                Top = wt + wh - ActualHeight;
            }
            else
            {
                Left = wl + (ww - Width) / 2;
                Top = wt;
            }

            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            var dur = TimeSpan.FromMilliseconds(380);
            Slide.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(_bottomRight ? 36 : -36, 0, dur) { EasingFunction = ease });
            BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250)));
        }

        public void CloseAnimated()
        {
            if (_closing) return;
            _closing = true;

            var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            fade.Completed += (_, _) => Close();
            Slide.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(_bottomRight ? 24 : -24, TimeSpan.FromMilliseconds(200)));
            BeginAnimation(OpacityProperty, fade);
        }

        void Dismiss_Click(object sender, RoutedEventArgs e) { Dismissed?.Invoke(); CloseAnimated(); }
        void Snooze_Click(object sender, RoutedEventArgs e) { Snoozed?.Invoke(); CloseAnimated(); }
    }
}
