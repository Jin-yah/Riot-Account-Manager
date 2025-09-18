using LoLAccountLauncher.Controls;
using LoLAccountLauncher.Services;
using Microsoft.Data.Sqlite;

namespace LoLAccountLauncher
{
    /// <summary>
    /// The main window of the application.
    /// </summary>
    public partial class Form1 : Form
    {
        private CustomScrollPanel? accountsPanel;
        private CustomScrollPanel? notificationContainer;
        private TitleBarPanel? titleBarPanel;
        private Panel? addAccountButtonContainer;
        private Button? addAccountButton;
        private string dbPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // Prefer the executable's associated icon so the taskbar uses the same
            // icon as the built exe. Fall back to the embedded resource if that
            // fails (e.g., when running under the debugger with a different
            // layout).
            try
            {
                var exeIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (exeIcon != null)
                    this.Icon = exeIcon;
            }
            catch
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream("LoLAccountLauncher.icon.ico");
                if (stream != null)
                {
                    using (stream)
                    {
                        this.Icon = new Icon(stream);
                    }
                }
            }

            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LoLAccountLauncher"
            );
            Directory.CreateDirectory(appDataDir);
            dbPath = Path.Combine(appDataDir, "accounts.db");

            InitDatabase();
            LoadAccounts();
        }

        /// <summary>
        /// Initializes the SQLite database, creating the table and running schema migrations if necessary.
        /// </summary>
        private void InitDatabase()
        {
            using var con = new SqliteConnection($"Data Source={dbPath}");
            con.Open();

            string tableCheckSql =
                "SELECT name FROM sqlite_master WHERE type='table' AND name='accounts';";
            using (var cmd = new SqliteCommand(tableCheckSql, con))
            {
                if (cmd.ExecuteScalar() == null)
                {
                    string createSql =
                        "CREATE TABLE accounts (id INTEGER PRIMARY KEY, username TEXT, credential_key TEXT, display_name TEXT, order_index INTEGER);";
                    using (var createCmd = new SqliteCommand(createSql, con))
                    {
                        createCmd.ExecuteNonQuery();
                    }
                    return;
                }
            }

            Func<string, bool> columnExists = (columnName) =>
            {
                string columnCheckSql = $"PRAGMA table_info(accounts);";
                using (var cmd = new SqliteCommand(columnCheckSql, con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (
                                reader
                                    .GetString(1)
                                    .Equals(columnName, StringComparison.OrdinalIgnoreCase)
                            )
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            };

            Action<string, string> addColumn = (columnName, columnDefinition) =>
            {
                string addColumnSql =
                    $"ALTER TABLE accounts ADD COLUMN {columnName} {columnDefinition};";
                using (var addCmd = new SqliteCommand(addColumnSql, con))
                {
                    addCmd.ExecuteNonQuery();
                }
            };

            if (!columnExists("credential_key"))
                addColumn("credential_key", "TEXT");
            if (!columnExists("order_index"))
                addColumn("order_index", "INTEGER");
            if (!columnExists("display_name"))
                addColumn("display_name", "TEXT");

            string populateDisplayNameSql =
                "UPDATE accounts SET display_name = username WHERE display_name IS NULL;";
            using (var populateCmd = new SqliteCommand(populateDisplayNameSql, con))
            {
                populateCmd.ExecuteNonQuery();
            }

            string populateOrderIndexSql =
                "UPDATE accounts SET order_index = id WHERE order_index IS NULL;";
            using (var populateCmd = new SqliteCommand(populateOrderIndexSql, con))
            {
                populateCmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Loads account information from the database and populates the UI.
        /// </summary>
        private void LoadAccounts()
        {
            accountsPanel?.ContentPanel.Controls.Clear();
            using var con = new SqliteConnection($"Data Source={dbPath}");
            con.Open();
            string sql =
                "SELECT id, username, credential_key, COALESCE(display_name, username) FROM accounts ORDER BY order_index";
            using var cmd = new SqliteCommand(sql, con);
            using SqliteDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                int id = rdr.GetInt32(0);
                string username = rdr.GetString(1);
                string credentialKey = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                string displayName = rdr.GetString(3);

                var item = new AccountListItem(id, username, credentialKey, displayName);

                item.LoginRequested += async (s, e) =>
                {
                    if (s is not AccountListItem accountItem)
                        return;

                    if (accountsPanel != null)
                    {
                        foreach (Control control in accountsPanel.ContentPanel.Controls)
                        {
                            if (control is AccountListItem item)
                            {
                                item.Enabled = false;
                            }
                        }
                    }

                    try
                    {
                        string? password = CredentialManager.GetCredential(
                            accountItem.CredentialKey
                        );
                        if (password == null)
                        {
                            ShowNotification(
                                "Could not retrieve password.",
                                NotificationType.Error
                            );
                            return;
                        }
                        bool ok = await RiotAuth.LoginAsync(
                            accountItem.Username,
                            password,
                            ShowNotification
                        );
                        if (ok)
                            ShowNotification("Login request sent.", NotificationType.Information);
                    }
                    finally
                    {
                        if (accountsPanel != null)
                        {
                            foreach (Control control in accountsPanel.ContentPanel.Controls)
                            {
                                if (control is AccountListItem item && !item.IsDisposed)
                                {
                                    item.Enabled = true;
                                }
                            }
                        }
                    }
                };

                item.EditRequested += (s, e) =>
                {
                    if (s is AccountListItem accountItem)
                        EditAccount(
                            accountItem.AccountId,
                            accountItem.Username,
                            accountItem.CredentialKey
                        );
                };

                item.DeleteRequested += (s, e) =>
                {
                    if (s is AccountListItem accountItem)
                        DeleteAccount(accountItem.AccountId, accountItem.CredentialKey);
                };

                accountsPanel?.ContentPanel.Controls.Add(item);
            }
        }

        /// <summary>
        /// Displays an overlay panel for adding or editing an account.
        /// </summary>
        /// <param name="id">The database ID of the account to edit, or null for a new account.</param>
        /// <param name="username">The current username of the account.</param>
        /// <param name="password">The current password of the account.</param>
        /// <param name="credentialKey">The key for the credential in Windows Credential Manager.</param>
        private void ShowAccountDetailsOverlay(
            int? id = null,
            string username = "",
            string password = "",
            string? credentialKey = null
        )
        {
            SetMainControlsEnabled(false);
            var detailsPanel = new AccountDetailsPanel(username, password);

            Action closeOverlay = () =>
            {
                this.Controls.Remove(detailsPanel);
                detailsPanel.Dispose();
                SetMainControlsEnabled(true);
                EnsureCorrectLayout();
            };

            detailsPanel.SaveClicked += (s, ev) =>
            {
                string newUsername = detailsPanel.Username;
                string newPassword = detailsPanel.Password;

                if (
                    string.IsNullOrWhiteSpace(newUsername) || string.IsNullOrWhiteSpace(newPassword)
                )
                {
                    ShowNotification(
                        "Username and password cannot be empty.",
                        NotificationType.Error
                    );
                    return;
                }

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();

                if (id.HasValue && credentialKey != null)
                {
                    CredentialManager.SaveCredential(credentialKey, newUsername, newPassword);
                    string sql = "UPDATE accounts SET username=@u, display_name=@d WHERE id=@id";
                    using var cmd = new SqliteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@u", newUsername);
                    cmd.Parameters.AddWithValue("@d", newUsername);
                    cmd.Parameters.AddWithValue("@id", id.Value);
                    cmd.ExecuteNonQuery();
                    LoadAccounts();
                    ShowNotification(
                        $"Account '{newUsername}' updated successfully.",
                        NotificationType.Information
                    );
                }
                else
                {
                    int newOrderIndex = 0;
                    using (
                        var orderCmd = new SqliteCommand(
                            "SELECT COALESCE(MAX(order_index), -1) + 1 FROM accounts",
                            con
                        )
                    )
                    {
                        newOrderIndex = Convert.ToInt32(orderCmd.ExecuteScalar());
                    }

                    string newCredentialKey = "LoLAccountLauncher_" + Guid.NewGuid().ToString();
                    CredentialManager.SaveCredential(newCredentialKey, newUsername, newPassword);
                    string sql =
                        "INSERT INTO accounts (username, credential_key, display_name, order_index) VALUES (@u, @c, @d, @o)";
                    using var cmd = new SqliteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@u", newUsername);
                    cmd.Parameters.AddWithValue("@c", newCredentialKey);
                    cmd.Parameters.AddWithValue("@d", newUsername);
                    cmd.Parameters.AddWithValue("@o", newOrderIndex);
                    cmd.ExecuteNonQuery();
                    LoadAccounts();
                    ShowNotification(
                        $"Account '{newUsername}' added successfully.",
                        NotificationType.Information
                    );
                }

                closeOverlay();
            };

            detailsPanel.CancelClicked += (s, ev) =>
            {
                closeOverlay();
            };
            this.Controls.Add(detailsPanel);
            detailsPanel.BringToFront();
        }

        /// <summary>
        /// Displays a notification message at the top of the window.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="type">The type of notification (Information or Error).</param>
        public void ShowNotification(string message, NotificationType type)
        {
            if (notificationContainer == null)
                return;
            var notification = new NotificationPanel(message, type);
            ShowNotification(notification);
        }

        /// <summary>
        /// Displays a pre-configured notification panel at the top of the window.
        /// </summary>
        /// <param name="notification">The notification panel to display.</param>
        public void ShowNotification(NotificationPanel notification)
        {
            if (notificationContainer == null)
                return;
            notificationContainer.ContentPanel.Controls.Add(notification);
            notificationContainer.ContentPanel.ScrollControlIntoView(notification);
        }

        /// <summary>
        /// Handles the click event for the "Add Account" button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object that contains no event data.</param>
        private void AddAccountButton_Click(object? sender, EventArgs e)
        {
            ShowAccountDetailsOverlay();
        }

        /// <summary>
        /// Opens a dialog to edit an existing account's details.
        /// </summary>
        /// <param name="id">The database ID of the account.</param>
        /// <param name="username">The current username of the account.</param>
        /// <param name="credentialKey">The key for the credential in Windows Credential Manager.</param>
        private void EditAccount(int id, string username, string credentialKey)
        {
            string password = CredentialManager.GetCredential(credentialKey) ?? "";
            ShowAccountDetailsOverlay(id, username, password, credentialKey);
        }

        private void TitleBarPanel_SettingsClicked(object? sender, EventArgs e)
        {
            ShowSettingsOverlay();
        }

        private void ShowSettingsOverlay()
        {
            SetMainControlsEnabled(false);
            var settingsPanel = new SettingsPanel();

            Action closeOverlay = () =>
            {
                this.Controls.Remove(settingsPanel);
                settingsPanel.Dispose();
                SetMainControlsEnabled(true);
                EnsureCorrectLayout();
            };

            settingsPanel.SaveClicked += (s, ev) =>
            {
                var newSettings = new AppSettings
                {
                    RiotClientPath = settingsPanel.RiotClientPath,
                    LaunchDelayMs = settingsPanel.LaunchDelayMs,
                    CheckForUpdates = settingsPanel.CheckForUpdates,
                };
                SettingsService.SaveSettings(newSettings);

                ShowNotification("Settings saved successfully.", NotificationType.Information);
                closeOverlay();
            };

            settingsPanel.CancelClicked += (s, ev) =>
            {
                closeOverlay();
            };

            this.Controls.Add(settingsPanel);
            settingsPanel.BringToFront();
        }

        /// <summary>
        /// Deletes an account from the database and Windows Credential Manager after confirmation.
        /// </summary>
        /// <param name="id">The database ID of the account to delete.</param>
        /// <param name="credentialKey">The credential key to delete.</param>
        private void DeleteAccount(int id, string credentialKey)
        {
            Action confirmDelete = () =>
            {
                CredentialManager.DeleteCredential(credentialKey);
                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                string sql = "DELETE FROM accounts WHERE id=@id";
                using var cmd = new SqliteCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                LoadAccounts();
            };

            var notification = new NotificationPanel(
                "Are you sure you want to delete this account?",
                confirmDelete
            );
            ShowNotification(notification);
        }

        /// <summary>
        /// Enables or disables the main interactive controls on the form.
        /// </summary>
        /// <param name="enabled">True to enable controls, false to disable.</param>
        private void SetMainControlsEnabled(bool enabled)
        {
            if (addAccountButton != null)
                addAccountButton.Enabled = enabled;
            if (accountsPanel != null)
                accountsPanel.Enabled = enabled;
        }

        /// <summary>
        /// Ensures the main controls are in the correct Z-order for proper docking and layering.
        /// </summary>
        private void EnsureCorrectLayout()
        {
            accountsPanel?.SendToBack();
            addAccountButtonContainer?.SendToBack();
            titleBarPanel?.SendToBack();
            notificationContainer?.BringToFront();
        }

        /// <summary>
        /// Updates the height of the notification container based on its content.
        /// </summary>
        private void UpdateNotificationContainerHeight()
        {
            if (notificationContainer == null)
                return;
            int totalHeight = notificationContainer.ContentPanel.Padding.Vertical;
            foreach (Control control in notificationContainer.ContentPanel.Controls)
            {
                totalHeight += control.Height + control.Margin.Top + control.Margin.Bottom;
            }
            notificationContainer.Height = Math.Min(
                totalHeight,
                notificationContainer.MaximumSize.Height
            );
            notificationContainer.Location = new Point(0, titleBarPanel?.Height ?? 32);
            notificationContainer.Width = this.ClientSize.Width;
            if (this.Controls.Contains(notificationContainer))
                notificationContainer.BringToFront();
        }

        /// <summary>
        /// Initializes the components of the form.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "LoL Account Launcher";
            this.Size = new Size(420, 600);
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);

            titleBarPanel = new TitleBarPanel();
            titleBarPanel.SettingsClicked += TitleBarPanel_SettingsClicked;
            titleBarPanel.MinimizeClicked += (s, e) => this.WindowState = FormWindowState.Minimized;

            notificationContainer = new CustomScrollPanel
            {
                Height = 0,
                MaximumSize = new Size(420, 198),
                BackColor = Color.Transparent,
            };
            notificationContainer.ContentPanel.ControlAdded += (s, e) =>
            {
                UpdateNotificationContainerHeight();
                if (e.Control != null && notificationContainer != null)
                {
                    e.Control.Width =
                        notificationContainer.ClientSize.Width - e.Control.Margin.Horizontal;
                }
            };
            notificationContainer.ContentPanel.ControlRemoved += (s, e) =>
                UpdateNotificationContainerHeight();

            addAccountButtonContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                Padding = new Padding(10, 10, 10, 0),
                BackColor = this.BackColor,
            };

            addAccountButton = new Button
            {
                Text = "Add Account",
                Dock = DockStyle.Fill,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            };

            addAccountButton.FlatAppearance.BorderSize = 0;
            addAccountButton.Click += AddAccountButton_Click;
            addAccountButtonContainer.Controls.Add(addAccountButton);
            accountsPanel = new CustomScrollPanel { Dock = DockStyle.Fill };
            accountsPanel.ContentPanel.Padding = new Padding(5);
            accountsPanel.MaximumSize = new Size(420, 505);

            accountsPanel.ContentPanel.AllowDrop = true;
            accountsPanel.ContentPanel.DragEnter += AccountsPanel_DragEnter;
            accountsPanel.ContentPanel.DragOver += AccountsPanel_DragOver;
            accountsPanel.ContentPanel.DragDrop += AccountsPanel_DragDrop;

            Controls.AddRange(
                new Control[]
                {
                    titleBarPanel,
                    addAccountButtonContainer,
                    accountsPanel,
                    notificationContainer,
                }
            );

            this.Resize += (s, e) => UpdateNotificationContainerHeight();
            this.Load += async (s, e) =>
            {
                EnsureCorrectLayout();
                var settings = SettingsService.LoadSettings();
                if (settings.CheckForUpdates)
                {
                    var updateService = new UpdateService();
                    await updateService.CheckForUpdates(this);
                }
            };
        }

        /// <summary>
        /// Handles the DragEnter event for the accounts panel to set the visual effect.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
        private void AccountsPanel_DragEnter(object? sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        /// <summary>
        /// Handles the DragOver event to reorder account items as they are dragged.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
        private void AccountsPanel_DragOver(object? sender, DragEventArgs e)
        {
            if (accountsPanel == null)
                return;
            var contentPanel = accountsPanel.ContentPanel;
            Point clientPoint = contentPanel.PointToClient(new Point(e.X, e.Y));

            var dragPanel = e.Data?.GetData(typeof(AccountListItem)) as AccountListItem;
            if (dragPanel == null)
                return;

            var childControl = contentPanel.GetChildAtPoint(clientPoint);
            if (childControl == null)
                return;

            AccountListItem? targetItem = childControl as AccountListItem;
            if (targetItem == null && childControl.Parent is AccountListItem)
            {
                targetItem = (AccountListItem)childControl.Parent;
            }

            if (targetItem == null || targetItem == dragPanel)
                return;

            int dragIndex = contentPanel.Controls.GetChildIndex(dragPanel);
            int targetIndex = contentPanel.Controls.GetChildIndex(targetItem);

            if (dragIndex != targetIndex)
            {
                contentPanel.Controls.SetChildIndex(dragPanel, targetIndex);
            }
        }

        /// <summary>
        /// Handles the DragDrop event to finalize the new account order in the database.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
        private void AccountsPanel_DragDrop(object? sender, DragEventArgs e)
        {
            UpdateAccountOrderInDb();
        }

        /// <summary>
        /// Updates the order_index for all accounts in the database based on their current UI order.
        /// </summary>
        private void UpdateAccountOrderInDb()
        {
            if (accountsPanel == null)
                return;
            using var con = new SqliteConnection($"Data Source={dbPath}");
            con.Open();
            for (int i = 0; i < accountsPanel.ContentPanel.Controls.Count; i++)
            {
                if (accountsPanel.ContentPanel.Controls[i] is AccountListItem item)
                {
                    int accountId = item.AccountId;
                    string sql = "UPDATE accounts SET order_index = @order WHERE id = @id";
                    using var cmd = new SqliteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@order", i);
                    cmd.Parameters.AddWithValue("@id", accountId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
