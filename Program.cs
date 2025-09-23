using System.Runtime.InteropServices;

namespace LoLAccountLauncher
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    internal static class Program
    {
        // Windows API declarations for finding and activating windows
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_RESTORE = 9; // Restores a minimized window

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Unique name for the mutex
            string mutexName = "LoLAccountLauncherSingleInstanceMutex";
            using (Mutex mutex = new Mutex(true, mutexName, out bool createdNew))
            {
                if (!createdNew)
                {
                    // Another instance is already running
                    IntPtr hWnd = FindWindow(string.Empty, "LoL Account Launcher");
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_RESTORE);
                        SetForegroundWindow(hWnd);
                    }
                    return;
                }

                try
                {
                    ApplicationConfiguration.Initialize();
                    Application.Run(new Form1());
                }
                catch (Exception ex)
                {
                    string logPath = Path.Combine(AppContext.BaseDirectory, "error.log");
                    File.WriteAllText(logPath, ex.ToString());
                    MessageBox.Show(
                        $"A critical error occurred and has been logged to:\n{logPath}\n\nError: {ex.Message}",
                        "LoL Account Launcher - Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }
    }
}
