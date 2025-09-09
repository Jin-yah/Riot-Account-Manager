using System.Diagnostics;
using System.Runtime.InteropServices;
using LoLAccountLauncher.Controls;

namespace LoLAccountLauncher.Services
{
    /// <summary>
    /// Handles authentication with the local Riot Client API.
    /// </summary>
    public static class RiotAuth
    {
        /// <summary>
        /// Brings the thread that created the specified window into the foreground and activates the window.
        /// </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window.
        /// </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// Moves the cursor to the specified screen coordinates.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        /// <summary>
        /// Synthesizes mouse motion and button clicks.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern void mouse_event(
            uint dwFlags,
            uint dx,
            uint dy,
            uint cButtons,
            UIntPtr dwExtraInfo
        );

        /// <summary>
        /// Retrieves the position of the mouse cursor, in screen coordinates.
        /// </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// Defines the x- and y-coordinates of a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// Defines the coordinates of the upper-left and lower-right corners of a rectangle.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left,
                Top,
                Right,
                Bottom;
        }

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;

        /// <summary>
        /// Ensures the Riot Client process is running, starting it if necessary.
        /// </summary>
        /// <param name="showNotification">A callback to display notifications to the user.</param>
        /// <returns>True if the client is running or was started successfully, otherwise false.</returns>
        private static async Task<bool> EnsureRiotClientRunningAsync(
            Action<string, NotificationType> showNotification
        )
        {
            if (Process.GetProcessesByName("Riot Client").Any())
                return true;

            string[] candidatePaths = new[]
            {
                "C:/Riot Games/Riot Client/RiotClientServices.exe",
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Riot Games/Riot Client/RiotClientServices.exe"
                ),
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Riot Games/Riot Client/RiotClientServices.exe"
                ),
            };

            string? exePath = candidatePaths.FirstOrDefault(File.Exists);

            if (exePath == null)
            {
                showNotification(
                    "Could not find Riot Client executable. Please start it manually.",
                    NotificationType.Error
                );
                return false;
            }

            try
            {
                Process.Start(
                    exePath,
                    "--launch-product=league_of_legends --launch-patchline=live"
                );
            }
            catch (Exception ex)
            {
                showNotification(
                    "Failed to start Riot Client: " + ex.Message,
                    NotificationType.Error
                );
                return false;
            }

            for (int i = 0; i < 50; i++)
            {
                if (Process.GetProcessesByName("Riot Client").Any())
                    return true;
                await Task.Delay(500);
            }

            showNotification("Riot Client did not start in time.", NotificationType.Error);
            return false;
        }

        /// <summary>
        /// Attempts to log into the Riot Client using UI automation.
        /// </summary>
        /// <param name="username">The account username.</param>
        /// <param name="password">The account password.</param>
        /// <param name="showNotification">A callback to display notifications to the user.</param>
        /// <returns>True if the login process was initiated successfully, otherwise false.</returns>
        public static async Task<bool> LoginAsync(
            string username,
            string password,
            Action<string, NotificationType> showNotification
        )
        {
            bool clientWasAlreadyRunning = Process.GetProcessesByName("Riot Client").Any();
            if (!await EnsureRiotClientRunningAsync(showNotification))
                return false;

            if (!clientWasAlreadyRunning)
            {
                await Task.Delay(3000);
            }

            try
            {
                var process = Process.GetProcessesByName("Riot Client").FirstOrDefault();
                if (process == null)
                {
                    showNotification(
                        "Riot Client process (Riot Client) not found.",
                        NotificationType.Error
                    );
                    return false;
                }

                IntPtr windowHandle = process.MainWindowHandle;
                if (windowHandle == IntPtr.Zero)
                {
                    await Task.Delay(1000);
                    process.Refresh();
                    windowHandle = process.MainWindowHandle;
                }

                if (windowHandle == IntPtr.Zero)
                {
                    showNotification(
                        "Could not find the main window of the Riot Client.",
                        NotificationType.Error
                    );
                    return false;
                }

                if (!GetWindowRect(windowHandle, out RECT windowRect))
                {
                    showNotification(
                        "Could not get the Riot Client window dimensions.",
                        NotificationType.Error
                    );
                    return false;
                }

                GetCursorPos(out POINT originalCursorPos);

                SetForegroundWindow(windowHandle);
                await Task.Delay(500);

                try
                {
                    int focusX = windowRect.Left + 100;
                    int focusY = windowRect.Top + 100;
                    SetCursorPos(focusX, focusY);
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                }
                finally
                {
                    SetCursorPos(originalCursorPos.X, originalCursorPos.Y);
                }
                await Task.Delay(200);

                for (int i = 0; i < 3; i++)
                {
                    SendKeys.SendWait("{TAB}");
                    await Task.Delay(50);
                }

                SendKeys.SendWait("^{a}{DEL}");
                await Task.Delay(100);
                SendKeys.SendWait(username);

                await Task.Delay(100);
                SendKeys.SendWait("{TAB}");
                await Task.Delay(100);

                SendKeys.SendWait("^{a}{DEL}");
                await Task.Delay(100);
                SendKeys.SendWait(password);

                await Task.Delay(200);
                SendKeys.SendWait("{ENTER}");

                return true;
            }
            catch (Exception ex)
            {
                showNotification(
                    $"An error occurred during UI automation: {ex.Message}",
                    NotificationType.Error
                );
                return false;
            }
        }
    }
}
