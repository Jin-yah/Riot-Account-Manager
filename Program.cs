namespace LoLAccountLauncher
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
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
