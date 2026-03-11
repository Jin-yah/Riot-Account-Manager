using System.Runtime.InteropServices;
using RiotAccountManager.Services;

namespace RiotAccountManager.Controls
{
    /// <summary>
    /// A custom title bar panel with window dragging and a close button.
    /// </summary>
    public class TitleBarPanel : Panel
    {
        /// <summary>
        /// The Windows message code for a non-client left mouse button down event.
        /// </summary>
        public const int WM_NCLBUTTONDOWN = 0xA1;

        /// <summary>
        /// The hit-test value for the title bar caption area.
        /// </summary>
        public const int HT_CAPTION = 0x2;

        public event EventHandler? SettingsClicked;
        public event EventHandler? MinimizeClicked;

        private readonly Button closeButton;
        private readonly Button minimizeButton;
        private readonly Button settingsButton;
        private readonly GameToggleSwitch gameToggleSwitch;
        private RiotGameProduct selectedGame;

        public event EventHandler? GameSelectionChanged;

        public RiotGameProduct SelectedGame
        {
            get => selectedGame;
            set
            {
                selectedGame = value;
                if (gameToggleSwitch.SelectedGame != value)
                {
                    gameToggleSwitch.SelectedGame = value;
                }
            }
        }

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        /// <summary>
        /// Releases the mouse capture from a window in the current thread.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        /// <summary>
        /// The label control that displays the window title.
        /// </summary>
        private readonly Label titleLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleBarPanel"/> class.
        /// </summary>
        public TitleBarPanel()
        {
            this.Dock = DockStyle.Top;
            this.Height = 32;
            this.BackColor = AppThemeManager.CurrentTheme.TitleBarBackground;

            titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = AppThemeManager.CurrentTheme.PrimaryText,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0),
            };

            closeButton = new Button
            {
                Text = "✖",
                Dock = DockStyle.Right,
                Width = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = this.BackColor,
                Font = new Font("Segoe UI", 10F),
            };
            closeButton.Click += (s, e) => this.FindForm()?.Close();

            minimizeButton = new Button
            {
                Text = "—",
                Dock = DockStyle.Right,
                Width = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = this.BackColor,
                Font = new Font("Segoe UI", 10F),
            };
            minimizeButton.Click += (s, e) => MinimizeClicked?.Invoke(this, EventArgs.Empty);

            Action<object?, MouseEventArgs> dragForm = (s, e) =>
            {
                if (e.Button == MouseButtons.Left && this.FindForm() is { } form)
                {
                    ReleaseCapture();
                    SendMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
            this.MouseDown += new MouseEventHandler(dragForm);
            titleLabel.MouseDown += new MouseEventHandler(dragForm);

            settingsButton = new Button
            {
                Text = "⚙", // Gear icon
                Dock = DockStyle.Right,
                Width = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = this.BackColor,
                Font = new Font("Segoe UI", 10F),
            };
            settingsButton.Click += (s, e) => SettingsClicked?.Invoke(this, EventArgs.Empty);

            var toggleHost = new Panel
            {
                Dock = DockStyle.Right,
                Width = 76,
                Padding = new Padding(6, 4, 6, 4),
                BackColor = this.BackColor,
            };

            gameToggleSwitch = new GameToggleSwitch
            {
                Dock = DockStyle.Fill,
            };
            gameToggleSwitch.SelectedGameChanged += (s, e) =>
            {
                selectedGame = gameToggleSwitch.SelectedGame;
                GameSelectionChanged?.Invoke(this, EventArgs.Empty);
            };
            toggleHost.Controls.Add(gameToggleSwitch);

            SelectedGame = RiotGameProduct.LeagueOfLegends;
            ApplyTheme(AppThemeManager.CurrentTheme);

            this.Controls.Add(titleLabel);
            this.Controls.Add(toggleHost);
            this.Controls.Add(settingsButton);
            this.Controls.Add(minimizeButton);
            this.Controls.Add(closeButton);
        }

        public void ApplyTheme(AppTheme theme)
        {
            BackColor = theme.TitleBarBackground;
            titleLabel.ForeColor = theme.PrimaryText;
            closeButton.BackColor = theme.TitleBarBackground;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = theme.DangerAccent;
            closeButton.FlatAppearance.MouseDownBackColor = theme.DangerAccentPressed;
            minimizeButton.BackColor = theme.TitleBarBackground;
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.FlatAppearance.MouseOverBackColor = theme.SurfaceBackground;
            minimizeButton.FlatAppearance.MouseDownBackColor = theme.SurfaceHoverBackground;
            settingsButton.BackColor = theme.TitleBarBackground;
            settingsButton.FlatAppearance.BorderSize = 0;
            settingsButton.FlatAppearance.MouseOverBackColor = theme.SurfaceBackground;
            settingsButton.FlatAppearance.MouseDownBackColor = theme.SurfaceHoverBackground;
            gameToggleSwitch.Theme = theme;
            foreach (Control control in Controls)
            {
                if (control is Panel panel)
                {
                    panel.BackColor = theme.TitleBarBackground;
                }
            }
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="Control.ParentChanged"/> event and updates the title label text.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (this.Parent is Form parentForm)
            {
                titleLabel.Text = parentForm.Text;
            }
        }
    }
}
