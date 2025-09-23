using System.Text.Json;

namespace LoLAccountLauncher.Services
{
    public class UpdateService
    {
        private const string GitHubApiUrl =
            "https://api.github.com/repos/Jin-yah/LoL-Account-Launcher/releases/latest";
        private const string CurrentVersion = "v1.4";

        public async Task CheckForUpdates(Form1 mainForm)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "LoLAccountLauncher");
                    var response = await client.GetStringAsync(GitHubApiUrl);
                    var release = JsonDocument.Parse(response).RootElement;
                    var latestVersion = release.GetProperty("tag_name").GetString();

                    if (latestVersion != null && latestVersion != CurrentVersion)
                    {
                        ShowUpdateNotification(latestVersion, mainForm);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for updates: {ex.Message}");
            }
        }

        private void ShowUpdateNotification(string latestVersion, Form1 mainForm)
        {
            if (string.IsNullOrEmpty(latestVersion))
            {
                return;
            }

            Action onConfirm = () =>
            {
                // Open the GitHub releases page in the default browser
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://github.com/Jin-yah/LoL-Account-Launcher/releases",
                        UseShellExecute = true,
                    }
                );
            };

            var notification = new Controls.NotificationPanel(
                $"An update is available! Version {latestVersion} is here.",
                onConfirm
            );

            mainForm.ShowNotification(notification);
        }
    }
}
