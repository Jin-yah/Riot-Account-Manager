namespace LoLAccountLauncher.Controls
{
    /// <summary>
    /// A custom control representing a single account in the list.
    /// It handles its own UI, hover animations, and exposes events for user actions.
    /// </summary>
    public class AccountListItem : Panel
    {
        /// <summary>
        /// Gets the database ID of the account.
        /// </summary>
        public int AccountId { get; }

        /// <summary>
        /// Gets the account username.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the key for the credential in Windows Credential Manager.
        /// </summary>
        public string CredentialKey { get; }

        /// <summary>
        /// Gets the display name for the account.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Occurs when the user clicks the item to log in.
        /// </summary>
        public event EventHandler? LoginRequested;

        /// <summary>
        /// Occurs when the user clicks the Edit button.
        /// </summary>
        public event EventHandler? EditRequested;

        /// <summary>
        /// Occurs when the user clicks the Delete button.
        /// </summary>
        public event EventHandler? DeleteRequested;

        private readonly Label usernameLabel;
        private readonly Button editBtn;
        private readonly Button deleteBtn;
        private readonly Label dragHandle;

        private readonly System.Windows.Forms.Timer animationTimer;
        private DateTime animationStart;
        private bool isHovering = false;
        private readonly Color startColor = Color.FromArgb(62, 62, 66);
        private readonly Color hoverColor = Color.FromArgb(82, 82, 86);

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountListItem"/> class.
        /// </summary>
        /// <param name="accountId">The database ID of the account.</param>
        /// <param name="username">The account username.</param>
        /// <param name="credentialKey">The key for the credential in Windows Credential Manager.</param>
        /// <param name="displayName">The display name for the account.</param>
        public AccountListItem(
            int accountId,
            string username,
            string credentialKey,
            string displayName
        )
        {
            this.AccountId = accountId;
            this.Username = username;
            this.CredentialKey = credentialKey;
            this.DisplayName = displayName;

            this.Width = 400;
            this.Height = 50;
            this.Margin = new Padding(5);
            this.BackColor = startColor;
            this.Cursor = Cursors.Hand;

            dragHandle = new Label
            {
                Text = "⋮",
                Dock = DockStyle.Left,
                Width = 20,
                ForeColor = Color.FromArgb(150, 150, 150),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.SizeAll,
            };

            usernameLabel = new Label
            {
                Text = displayName,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand,
            };

            editBtn = new Button
            {
                Text = "Edit",
                Dock = DockStyle.Right,
                Width = 65,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 90, 158),
                Font = new Font("Segoe UI", 9F),
            };
            editBtn.FlatAppearance.BorderSize = 0;
            editBtn.Visible = false;

            deleteBtn = new Button
            {
                Text = "Delete",
                Dock = DockStyle.Right,
                Width = 65,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(179, 57, 57),
                Font = new Font("Segoe UI", 9F),
            };
            deleteBtn.FlatAppearance.BorderSize = 0;
            deleteBtn.Visible = false;

            dragHandle.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.DoDragDrop(this, DragDropEffects.Move);
                }
            };

            animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
            animationTimer.Tick += AnimationTimer_Tick;

            Action<object?, EventArgs> enterAction = (s, e) =>
            {
                if (!isHovering)
                {
                    isHovering = true;
                    animationStart = DateTime.Now;
                    editBtn.Visible = true;
                    deleteBtn.Visible = true;
                    animationTimer.Start();
                }
            };

            Action<object?, EventArgs> leaveAction = (s, e) =>
            {
                if (
                    isHovering
                    && !this.ClientRectangle.Contains(this.PointToClient(Cursor.Position))
                )
                {
                    isHovering = false;
                    animationStart = DateTime.Now;
                    animationTimer.Start();
                }
            };

            this.MouseEnter += new EventHandler(enterAction);
            usernameLabel.MouseEnter += new EventHandler(enterAction);
            editBtn.MouseEnter += new EventHandler(enterAction);
            deleteBtn.MouseEnter += new EventHandler(enterAction);

            this.MouseLeave += new EventHandler(leaveAction);
            usernameLabel.MouseLeave += new EventHandler(leaveAction);
            editBtn.MouseLeave += new EventHandler(leaveAction);
            deleteBtn.MouseLeave += new EventHandler(leaveAction);

            this.Click += (s, e) => LoginRequested?.Invoke(this, EventArgs.Empty);
            usernameLabel.Click += (s, e) => LoginRequested?.Invoke(this, EventArgs.Empty);
            editBtn.Click += (s, e) => EditRequested?.Invoke(this, EventArgs.Empty);
            deleteBtn.Click += (s, e) => DeleteRequested?.Invoke(this, EventArgs.Empty);

            this.Controls.Add(dragHandle);
            this.Controls.Add(usernameLabel);
            this.Controls.Add(editBtn);
            this.Controls.Add(deleteBtn);
        }

        /// <summary>
        /// Handles the animation timer tick to create a smooth hover effect.
        /// </summary>
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            var elapsed = (DateTime.Now - animationStart).TotalMilliseconds;
            var progress = elapsed / 200.0; // 0.2-second animation

            if (progress >= 1.0)
            {
                animationTimer.Stop();
                progress = 1.0;
                if (!isHovering)
                {
                    editBtn.Visible = false;
                    deleteBtn.Visible = false;
                }
            }

            this.BackColor = isHovering
                ? LerpColor(startColor, hoverColor, progress)
                : LerpColor(hoverColor, startColor, progress);
        }

        /// <summary>
        /// Linearly interpolates between two colors.
        /// </summary>
        private static Color LerpColor(Color a, Color b, double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            int red = (int)(a.R + (b.R - a.R) * t);
            int green = (int)(a.G + (b.G - a.G) * t);
            int blue = (int)(a.B + (b.B - a.B) * t);
            return Color.FromArgb(a.A, red, green, blue);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
