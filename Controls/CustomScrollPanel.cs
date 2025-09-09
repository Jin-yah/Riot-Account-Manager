namespace LoLAccountLauncher.Controls
{
    /// <summary>
    /// A panel that provides a custom-styled, minimalistic scrollbar for its content.
    /// </summary>
    public class CustomScrollPanel : Panel
    {
        /// <summary>
        /// The inner panel that holds the user controls and enables scrolling.
        /// </summary>
        private readonly TransparentFlowLayoutPanel contentPanel;

        /// <summary>
        /// The panel used to hide the default scrollbar of the content panel.
        /// </summary>
        private readonly TransparentPanel clippingPanel;

        /// <summary>
        /// Gets the inner FlowLayoutPanel that holds the content.
        /// </summary>
        public TransparentFlowLayoutPanel ContentPanel => contentPanel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomScrollPanel"/> class.
        /// </summary>
        public CustomScrollPanel()
        {
            this.BackColor = Color.FromArgb(45, 45, 48);

            // This panel will contain the FlowLayoutPanel and clip its default scrollbar
            clippingPanel = new TransparentPanel
            {
                Dock = DockStyle.Fill,
                BackColor = this.BackColor,
            };

            contentPanel = new TransparentFlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
            };

            // When the clipping panel resizes, adjust the content panel. We keep the
            // content panel slightly wider to push the native vertical scrollbar
            // out of view, but its height must match the visible clipping area so
            // AutoScroll can correctly detect vertical overflow.
            clippingPanel.Layout += (s, e) =>
            {
                contentPanel.Location = Point.Empty;
                contentPanel.Width =
                    clippingPanel.ClientSize.Width + SystemInformation.VerticalScrollBarWidth;
                contentPanel.Height = clippingPanel.ClientSize.Height;
            };
            clippingPanel.Controls.Add(contentPanel);

            this.Controls.Add(clippingPanel);

            // Wire up events
            this.MouseWheel += OnMouseWheel;
        }

        /// <summary>
        /// Handles the MouseWheel event to manually scroll the content panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        private void OnMouseWheel(object? sender, MouseEventArgs e)
        {
            if (contentPanel.VerticalScroll.Visible)
            {
                int scrollAmount = e.Delta > 0 ? -1 : 1;
                int newScrollValue =
                    contentPanel.VerticalScroll.Value
                    + (scrollAmount * contentPanel.VerticalScroll.SmallChange);

                int maxScroll =
                    contentPanel.VerticalScroll.Maximum
                    - contentPanel.VerticalScroll.LargeChange
                    + 1;
                newScrollValue = Math.Max(
                    contentPanel.VerticalScroll.Minimum,
                    Math.Min(maxScroll, newScrollValue)
                );

                contentPanel.VerticalScroll.Value = newScrollValue;
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.BackColorChanged"/> event and updates child control colors.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (clippingPanel != null)
                clippingPanel.BackColor = this.BackColor;
            if (contentPanel != null)
                contentPanel.BackColor = this.BackColor;
        }

        /// <summary>
        /// A Panel that is configured to support a transparent background color.
        /// </summary>
        private class TransparentPanel : Panel
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TransparentPanel"/> class.
            /// </summary>
            public TransparentPanel()
            {
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            }
        }

        /// <summary>
        /// A FlowLayoutPanel that is configured to support a transparent background color.
        /// </summary>
        public class TransparentFlowLayoutPanel : FlowLayoutPanel
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TransparentFlowLayoutPanel"/> class.
            /// </summary>
            public TransparentFlowLayoutPanel()
            {
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            }
        }
    }
}
