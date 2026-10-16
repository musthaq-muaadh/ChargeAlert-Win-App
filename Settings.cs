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

        static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChargeAlert", "settings.json");

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
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }
    }
}
