using System;
using System.Drawing;
using System.Drawing.Text;

namespace ChargeAlert
{
    /// <summary>Draws the battery percentage as the tray icon.</summary>
    static class TrayIcons
    {
        /// <summary>The app logo as a tray icon.</summary>
        public static Icon Logo()
        {
            try
            {
                var info = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/logo.ico"));
                using var stream = info.Stream;
                return new Icon(stream);
            }
            catch { return (Icon)SystemIcons.Application.Clone(); }
        }

        public static Icon Make(int pct, bool charging, bool low)
        {
            using var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                var color = charging ? Color.FromArgb(52, 199, 89)
                          : low ? Color.FromArgb(255, 69, 58)
                          : Color.White;

                float size = pct >= 100 ? 17f : 24f;
                using var font = new Font("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Pixel);
                using var brush = new SolidBrush(color);
                using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(pct.ToString(), font, brush, new RectangleF(-2, 0, 36, 32), fmt);
            }

            var h = bmp.GetHicon();
            var icon = (Icon)Icon.FromHandle(h).Clone();
            Native.DestroyIcon(h);
            return icon;
        }
    }
}
