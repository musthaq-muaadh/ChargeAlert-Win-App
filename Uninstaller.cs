using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace ChargeAlert
{
    /// <summary>
    /// Removes ChargeAlert cleanly: startup entry, saved settings and the app's own files.
    /// Only files named "ChargeAlert.*" next to the program are deleted, never the whole folder.
    /// </summary>
    static class Uninstaller
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static string InstallFolder =>
            Path.GetDirectoryName(Environment.ProcessPath ?? "") ?? "";

        public static void Run(bool removeSettings)
        {
            Settings.SavingDisabled = true;
            var notes = new StringBuilder();

            // 1) no more starting with Windows
            try
            {
                using var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKey, true);
                k?.DeleteValue("ChargeAlert", false);
                notes.AppendLine("•  Removed from Windows startup");
            }
            catch { }

            // 2) saved settings
            if (removeSettings)
            {
                try
                {
                    if (Directory.Exists(Settings.Folder)) Directory.Delete(Settings.Folder, true);
                    notes.AppendLine("•  Deleted your settings and saved data");
                }
                catch { notes.AppendLine("•  Could not delete the settings folder (it may be in use)"); }
            }

            // 3) the program files, after this process has exited
            var exe = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exe))
            {
                var dir = Path.GetDirectoryName(exe)!;
                var setupUninstaller = Path.Combine(dir, "unins000.exe");

                if (File.Exists(setupUninstaller))
                {
                    // installed with Setup.exe: let its uninstaller remove files and shortcuts
                    RunLater($"start \"\" \"{setupUninstaller}\"");
                    notes.AppendLine("•  Starting the uninstaller to remove the program files");
                }
                else if (!string.Equals(Path.GetFileNameWithoutExtension(exe), "ChargeAlert", StringComparison.OrdinalIgnoreCase))
                {
                    notes.AppendLine("•  Program files left in place (started through the .NET host)");
                }
                else if (dir.IndexOfAny(new[] { '&', '%', '^', '"' }) >= 0)
                {
                    notes.AppendLine("•  Program files left in place (the folder name has special characters). You can delete the folder yourself.");
                }
                else
                {
                    var cmd = $"del /f /q \"{dir}\\ChargeAlert.*\" & rmdir \"{dir}\" 2>nul";
                    var extracted = Path.Combine(Path.GetTempPath(), ".net", "ChargeAlert");
                    if (Directory.Exists(extracted) && extracted.IndexOfAny(new[] { '&', '%', '^' }) < 0)
                        cmd += $" & rmdir /s /q \"{extracted}\"";
                    RunLater(cmd);
                    notes.AppendLine("•  Deleting the program files");
                }
            }

            MessageBox.Show("ChargeAlert has been removed.\n\n" + notes.ToString().TrimEnd(),
                            "Uninstall ChargeAlert", MessageBoxButton.OK, MessageBoxImage.Information);

            Application.Current.Shutdown();
        }

        /// <summary>Runs a cmd line a few seconds from now, once this program has closed.</summary>
        static void RunLater(string commands)
        {
            try
            {
                Process.Start(new ProcessStartInfo("cmd.exe", "/c ping 127.0.0.1 -n 4 >nul & " + commands)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch { }
        }
    }
}
