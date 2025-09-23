using LoLAccountLauncher.Services;

namespace LoLAccountLauncher.Controls
{
    /// <summary>
    /// An overlay panel for configuring application settings.
    /// </summary>
    public class SettingsPanel : Panel
    {
        private readonly TextBox riotClientPathBox;
        private readonly Button saveButton;
        private readonly Button cancelButton;
        private readonly Button browseButton;
        private readonly TextBox launchDelayBox;
        private readonly CheckBox checkForUpdatesBox;

        private Form? _parentForm;
        private IButtonControl? _originalAcceptButton;
        private IButtonControl? _originalCancelButton;

        /// <summary>
        /// Gets the configured Riot Client executable path.
        /// </summary>
        public string RiotClientPath => riotClientPathBox.Text;

        /// <summary>
        /// Gets the configured launch delay in milliseconds.
        /// </summary>
        public int LaunchDelayMs
        {
            get
            {
                if (int.TryParse(launchDelayBox.Text, out int delay))
                {
                    return delay;
                }
                return 3000;
            }
        }

        public bool CheckForUpdates => checkForUpdatesBox.Checked;

        /// <summary>
        /// Occurs when the user clicks the Save button.
        /// </summary>
        public event EventHandler? SaveClicked;

        /// <summary>
        /// Occurs when the user clicks the Cancel button.
        /// </summary>
        public event EventHandler? CancelClicked;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPanel"/> class.
        /// </summary>
        public SettingsPanel()
        {
            var settings = SettingsService.LoadSettings();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;

            var container = new Panel
            {
                Size = new Size(400, 210),
                BackColor = Color.FromArgb(62, 62, 66),
                Anchor = AnchorStyles.None,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = true,
            };
            this.Size = container.Size;
            container.Dock = DockStyle.Fill;

            this.ParentChanged += SettingsPanel_ParentChanged;
            this.Disposed += SettingsPanel_Disposed;

            var pathLabel = new Label
            {
                Text = "Riot Client Path:",
                Left = 20,
                Top = 30,
                Width = 120,
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Segoe UI", 9F),
            };

            riotClientPathBox = new TextBox
            {
                Left = 20,
                Top = 55,
                Width = 275,
                Text = settings.RiotClientPath,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.FromArgb(241, 241, 241),
                BorderStyle = BorderStyle.FixedSingle,
            };

            browseButton = new Button
            {
                Text = "Browse...",
                Left = 300,
                Top = 54,
                Width = 80,
                Height = 24,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(82, 82, 82),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            };
            browseButton.FlatAppearance.BorderSize = 0;
            browseButton.Click += BrowseButton_Click;

            var delayLabel = new Label
            {
                Text = "Launch Delay (ms):",
                Left = 20,
                Top = 90,
                Width = 120,
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Segoe UI", 9F),
            };

            launchDelayBox = new TextBox
            {
                Left = 150,
                Top = 90,
                Width = 145,
                Text = settings.LaunchDelayMs.ToString(),
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.FromArgb(241, 241, 241),
                BorderStyle = BorderStyle.FixedSingle,
            };

            checkForUpdatesBox = new CheckBox
            {
                Text = "Automatically check for updates",
                Left = 20,
                Top = 120,
                Width = 250,
                Checked = settings.CheckForUpdates,
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Segoe UI", 9F),
            };

            saveButton = new Button
            {
                Text = "Save",
                Left = 110,
                Top = 150,
                Width = 85,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += (s, e) => SaveClicked?.Invoke(this, EventArgs.Empty);

            cancelButton = new Button
            {
                Text = "Cancel",
                Left = 205,
                Top = 150,
                Width = 85,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(82, 82, 82),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += (s, e) => CancelClicked?.Invoke(this, EventArgs.Empty);

            container.Controls.Add(pathLabel);
            container.Controls.Add(riotClientPathBox);
            container.Controls.Add(browseButton);
            container.Controls.Add(delayLabel);
            container.Controls.Add(launchDelayBox);
            container.Controls.Add(checkForUpdatesBox);
            container.Controls.Add(saveButton);
            container.Controls.Add(cancelButton);

            this.Controls.Add(container);
        }

        /// <summary>
        /// Handles the click event for the browse button, opening a file dialog to select the Riot Client executable.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Executable Files (*.exe)|*.exe|All files (*.*)|*.*";
                ofd.Title = "Select Riot Client Executable";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    riotClientPathBox.Text = ofd.FileName;
                }
            }
        }

        /// <summary>
        /// Handles the ParentChanged event to center the panel and manage form accept/cancel buttons.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void SettingsPanel_ParentChanged(object? sender, EventArgs e)
        {
            if (this.Parent != null && _parentForm == null)
            {
                _parentForm = this.FindForm();
                if (_parentForm != null)
                {
                    _originalAcceptButton = _parentForm.AcceptButton;
                    _originalCancelButton = _parentForm.CancelButton;
                    _parentForm.AcceptButton = saveButton;
                    _parentForm.CancelButton = cancelButton;
                }
                void PositionOverlay()
                {
                    var p = this.Parent;
                    if (p == null)
                        return;
                    this.Location = new Point(
                        Math.Max(0, (p.ClientSize.Width - this.Width) / 2),
                        Math.Max(0, (p.ClientSize.Height - this.Height) / 2)
                    );
                }

                this.Parent.Resize += (ps, pe) => PositionOverlay();
                PositionOverlay();
            }
        }

        /// <summary>
        /// Handles the Disposed event to restore the original accept/cancel buttons on the parent form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void SettingsPanel_Disposed(object? sender, EventArgs e)
        {
            if (_parentForm != null && !_parentForm.IsDisposed)
            {
                _parentForm.AcceptButton = _originalAcceptButton;
                _parentForm.CancelButton = _originalCancelButton;
            }
        }
    }
}
