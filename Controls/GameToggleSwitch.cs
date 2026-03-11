using System.Drawing.Drawing2D;
using RiotAccountManager.Services;

namespace RiotAccountManager.Controls
{
    public class GameToggleSwitch : Control
    {
        private RiotGameProduct selectedGame = RiotGameProduct.LeagueOfLegends;
        private AppTheme theme = AppThemeManager.CurrentTheme;

        public event EventHandler? SelectedGameChanged;

        public RiotGameProduct SelectedGame
        {
            get => selectedGame;
            set
            {
                if (selectedGame == value)
                    return;

                selectedGame = value;
                Invalidate();
                SelectedGameChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public AppTheme Theme
        {
            get => theme;
            set
            {
                theme = value;
                Invalidate();
            }
        }

        public GameToggleSwitch()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.UserPaint,
                true
            );
            Size = new Size(64, 24);
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            SelectedGame = selectedGame == RiotGameProduct.LeagueOfLegends
                ? RiotGameProduct.Valorant
                : RiotGameProduct.LeagueOfLegends;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle trackBounds = new(1, 1, Width - 2, Height - 2);

            using (var trackPath = CreateRoundedRectangle(trackBounds, Height / 2f))
            using (var trackBrush = new SolidBrush(theme.ToggleTrackBackground))
            {
                e.Graphics.FillPath(trackBrush, trackPath);
            }

            int knobPadding = 3;
            int knobSize = Height - (knobPadding * 2);
            int knobX = selectedGame == RiotGameProduct.LeagueOfLegends
                ? knobPadding
                : Width - knobSize - knobPadding;
            Rectangle knobBounds = new(knobX, knobPadding, knobSize, knobSize);

            using (var knobBrush = new SolidBrush(theme.ToggleKnobBackground))
            using (var knobAccentBrush = new SolidBrush(theme.ToggleKnobAccent))
            using (var knobPath = CreateRoundedRectangle(knobBounds, knobSize / 2f))
            {
                e.Graphics.FillPath(knobBrush, knobPath);

                Rectangle innerAccent = new(
                    knobBounds.X + (knobBounds.Width / 4),
                    knobBounds.Y + (knobBounds.Height / 4),
                    Math.Max(4, knobBounds.Width / 2),
                    Math.Max(4, knobBounds.Height / 2)
                );
                using var accentPath = CreateRoundedRectangle(innerAccent, innerAccent.Height / 2f);
                e.Graphics.FillPath(knobAccentBrush, accentPath);
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Space || keyData == Keys.Enter || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
        }

        private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, float radius)
        {
            float diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(
                bounds.Right - diameter,
                bounds.Bottom - diameter,
                diameter,
                diameter,
                0,
                90
            );
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}