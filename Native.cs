using System;
using System.Runtime.InteropServices;

namespace ChargeAlert
{
    static class Native
    {
        [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr handle);
        [DllImport("shell32.dll")] static extern int SHQueryUserNotificationState(out int state);

        /// <summary>True when a fullscreen app / game / presentation is in front.</summary>
        public static bool FullscreenAppActive() =>
            SHQueryUserNotificationState(out var s) == 0 && (s == 2 || s == 3 || s == 4);
    }
}
