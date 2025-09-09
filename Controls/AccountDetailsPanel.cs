using System.Windows.Forms;

namespace LoLAccountLauncher.Controls
{
    /// <summary>
    /// An overlay panel for entering or editing account credentials.
    /// </summary>
    public class AccountDetailsPanel : Panel
    {
        private readonly TextBox usernameBox;
        private readonly TextBox passwordBox;
        private readonly Button saveButton;
        private readonly Button cancelButton;

        private Form? _parentForm;
        private IButtonControl? _originalAcceptButton;
        private IButtonControl? _originalCancelButton;

        /// <summary>
        /// Gets the username entered by the user.
        /// </summary>
        public string Username => usernameBox.Text;

        /// <summary>
        /// Gets the password entered by the user.
        /// </summary>
        public string Password => passwordBox.Text;

        /// <summary>
        /// Occurs when the user clicks the Save button.
        /// </summary>
        public event EventHandler? SaveClicked;

        /// <summary>
        /// Occurs when the user clicks the Cancel button.
        /// </summary>
        public event EventHandler? CancelClicked;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDetailsPanel"/> class.
        /// </summary>
        /// <param name="username">The initial username to display.</param>
        /// <param name="password">The initial password to display.</param>
        public AccountDetailsPanel(string username = "", string password = "")
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;

            var container = new Panel
            {
                Size = new Size(340, 220),
                BackColor = Color.FromArgb(62, 62, 66),
                Anchor = AnchorStyles.None,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = true,
            };
            this.Size = container.Size;
            container.Dock = DockStyle.Fill;

            this.ParentChanged += AccountDetailsPanel_ParentChanged;
            this.Disposed += AccountDetailsPanel_Disposed;

            var userLabel = new Label
            {
                Text = "Username:",
                Left = 20,
                Top = 20,
                Width = 80,
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Segoe UI", 9F),
            };
            usernameBox = new TextBox
            {
                Left = 110,
                Top = 20,
                Width = 180,
                Text = username,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.FromArgb(241, 241, 241),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var passLabel = new Label
            {
                Text = "Password:",
                Left = 20,
                Top = 60,
                Width = 80,
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Segoe UI", 9F),
            };
            passwordBox = new TextBox
            {
                Left = 110,
                Top = 60,
                Width = 180,
                Text = password,
                UseSystemPasswordChar = true,
                BackColor = Color.FromArgb(62, 62, 66),
                ForeColor = Color.FromArgb(241, 241, 241),
                BorderStyle = BorderStyle.FixedSingle,
            };

            saveButton = new Button
            {
                Text = "Save",
                Left = 110,
                Top = 120,
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
                Top = 120,
                Width = 85,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(82, 82, 82),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += (s, e) => CancelClicked?.Invoke(this, EventArgs.Empty);

            container.Controls.Add(userLabel);
            container.Controls.Add(usernameBox);
            container.Controls.Add(passLabel);
            container.Controls.Add(passwordBox);
            container.Controls.Add(saveButton);
            container.Controls.Add(cancelButton);

            this.Controls.Add(container);
        }

        /// <summary>
        /// Handles the ParentChanged event to center the panel and manage form accept/cancel buttons.
        /// </summary>
        private void AccountDetailsPanel_ParentChanged(object? sender, EventArgs e)
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
        private void AccountDetailsPanel_Disposed(object? sender, EventArgs e)
        {
            if (_parentForm != null && !_parentForm.IsDisposed)
            {
                _parentForm.AcceptButton = _originalAcceptButton;
                _parentForm.CancelButton = _originalCancelButton;
            }
        }
    }
}
