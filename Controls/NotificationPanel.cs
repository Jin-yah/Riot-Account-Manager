using RiotAccountManager.Services;

namespace RiotAccountManager.Controls
{
    /// <summary>
    /// Specifies the type of notification to display.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// An informational message.
        /// </summary>
        Information,

        /// <summary>
        /// An error message.
        /// </summary>
        Error,
    }

    /// <summary>
    /// A closable panel for displaying in-app notifications.
    /// </summary>
    public class NotificationPanel : Panel
    {
        private readonly NotificationType? notificationType;
        private readonly Button? closeButton;
        private readonly Button? declineButton;
        private readonly Button? confirmButton;
        private readonly Label messageLabel;

        /// <summary>
        /// The timer to automatically close the notification after a set duration.
        /// </summary>
        private System.Windows.Forms.Timer? autoCloseTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationPanel"/> class.
        /// </summary>
        /// <param name="message">The message to display in the notification.</param>
        /// <param name="type">The type of notification (Information or Error).</param>
        public NotificationPanel(string message, NotificationType type)
        {
            notificationType = type;
            this.Height = 60;
            this.Padding = new Padding(5);
            this.Margin = new Padding(3);

            closeButton = new Button
            {
                Text = "✖",
                Dock = DockStyle.Right,
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Dispose();

            messageLabel = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.Transparent,
            };

            this.Controls.Add(messageLabel);
            this.Controls.Add(closeButton);

            ApplyTheme(AppThemeManager.CurrentTheme);

            InitializeAutoCloseTimer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationPanel"/> class for confirmation prompts.
        /// </summary>
        /// <param name="message">The confirmation message to display.</param>
        /// <param name="onConfirm">The action to execute when the user confirms.</param>
        public NotificationPanel(string message, Action onConfirm)
        {
            this.Height = 60;
            this.Padding = new Padding(5);
            this.Margin = new Padding(3);

            declineButton = new Button
            {
                Text = "✖",
                Dock = DockStyle.Right,
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
            };
            declineButton.FlatAppearance.BorderSize = 0;
            declineButton.Click += (s, e) => this.Dispose();

            confirmButton = new Button
            {
                Text = "✔",
                Dock = DockStyle.Right,
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
            };
            confirmButton.FlatAppearance.BorderSize = 0;
            confirmButton.Click += (s, e) =>
            {
                onConfirm();
                this.Dispose();
            };

            messageLabel = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.Transparent,
            };

            this.Controls.Add(messageLabel);
            this.Controls.Add(declineButton);
            this.Controls.Add(confirmButton);

            ApplyTheme(AppThemeManager.CurrentTheme);
        }

        public void ApplyTheme(AppTheme theme)
        {
            BackColor = notificationType == NotificationType.Error
                ? theme.NotificationErrorBackground
                : notificationType == NotificationType.Information
                    ? theme.NotificationInfoBackground
                    : theme.ConfirmationBackground;
            messageLabel.ForeColor = theme.PrimaryText;

            if (closeButton != null)
            {
                closeButton.ForeColor = theme.PrimaryText;
                closeButton.BackColor = Color.Transparent;
                closeButton.FlatAppearance.MouseOverBackColor = ThemeStyler.Lighten(
                    theme.SurfaceHoverBackground,
                    0.08
                );
                closeButton.FlatAppearance.MouseDownBackColor = theme.SurfaceHoverBackground;
            }

            if (declineButton != null)
            {
                declineButton.ForeColor = theme.ConfirmationDecline;
                declineButton.BackColor = Color.Transparent;
                declineButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, theme.ConfirmationDecline);
                declineButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(38, theme.ConfirmationDecline);
            }

            if (confirmButton != null)
            {
                confirmButton.ForeColor = theme.ConfirmationApprove;
                confirmButton.BackColor = Color.Transparent;
                confirmButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, theme.ConfirmationApprove);
                confirmButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(38, theme.ConfirmationApprove);
            }
        }

        /// <summary>
        /// Sets up and starts a timer that will automatically dispose of the notification.
        /// </summary>
        private void InitializeAutoCloseTimer()
        {
            autoCloseTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            autoCloseTimer.Tick += (sender, e) => this.Dispose();
            autoCloseTimer.Start();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                autoCloseTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
