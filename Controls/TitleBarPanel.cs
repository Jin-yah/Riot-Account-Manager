using System.Runtime.InteropServices;

namespace LoLAccountLauncher.Controls
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
            this.BackColor = Color.FromArgb(37, 37, 38);

            titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(220, 220, 220),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0),
            };

            var closeButton = new Button
            {
                Text = "✖",
                Dock = DockStyle.Right,
                Width = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(37, 37, 38),
                Font = new Font("Segoe UI", 10F),
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 57, 43);
            closeButton.Click += (s, e) => this.FindForm()?.Close();

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

            this.Controls.Add(titleLabel);
            this.Controls.Add(closeButton);
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
