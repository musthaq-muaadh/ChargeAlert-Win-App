using System;
using System.IO;
using System.Text.Json;

namespace ChargeAlert
{
    public sealed class Settings
    {
        /// <summary>Alert when charging reaches this percent (80 / 90 / 100).</summary>
        public int FullAt { get; set; } = 100;
        /// <summary>Low-battery alert percent; 0 = off.</summary>
        public int LowAt { get; set; } = 20;
        /// <summary>Re-remind every N minutes while still plugged in at full; 0 = off.</summary>
        public int RemindMinutes { get; set; } = 10;
        public bool Sound { get; set; } = true;
        public bool PlugToasts { get; set; } = true;
        public bool QuietInFullscreen { get; set; } = true;
        public bool PopupBottomRight { get; set; } = false;
        /// <summary>0 = battery percentage, 1 = app logo.</summary>
        public int TrayIconStyle { get; set; } = 0;
        /// <summary>How long short notices (plug / unplug, status) stay on screen.</summary>
        public int ToastSeconds { get; set; } = 4;

        /// <summary>Set during uninstall so nothing gets written back to disk afterwards.</summary>
        public static bool SavingDisabled { get; set; }

        public void ResetOptions()
        {
            var d = new Settings();
            FullAt = d.FullAt; LowAt = d.LowAt; RemindMinutes = d.RemindMinutes;
            Sound = d.Sound; PlugToasts = d.PlugToasts; QuietInFullscreen = d.QuietInFullscreen;
            PopupBottomRight = d.PopupBottomRight; TrayIconStyle = d.TrayIconStyle; ToastSeconds = d.ToastSeconds;
        }

        static string FilePath => Path.Combine(Folder, "settings.json");

        public static string Folder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChargeAlert");

        public static Settings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
            }
            catch { }
            return new Settings();
        }

        public void Save()
        {
            if (SavingDisabled) return;
            try
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }
    }
}
