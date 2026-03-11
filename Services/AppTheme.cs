namespace RiotAccountManager.Services
{
    public sealed class AppTheme
    {
        public required Color WindowBackground { get; init; }
        public required Color TitleBarBackground { get; init; }
        public required Color SurfaceBackground { get; init; }
        public required Color SurfaceHoverBackground { get; init; }
        public required Color InputBackground { get; init; }
        public required Color BorderColor { get; init; }
        public required Color PrimaryText { get; init; }
        public required Color MutedText { get; init; }
        public required Color PrimaryAccent { get; init; }
        public required Color PrimaryAccentHover { get; init; }
        public required Color PrimaryAccentPressed { get; init; }
        public required Color SecondaryAccent { get; init; }
        public required Color SecondaryAccentHover { get; init; }
        public required Color SecondaryAccentPressed { get; init; }
        public required Color DangerAccent { get; init; }
        public required Color DangerAccentHover { get; init; }
        public required Color DangerAccentPressed { get; init; }
        public required Color ToggleTrackBackground { get; init; }
        public required Color ToggleTrackBorder { get; init; }
        public required Color ToggleKnobBackground { get; init; }
        public required Color ToggleKnobAccent { get; init; }
        public required Color NotificationInfoBackground { get; init; }
        public required Color NotificationErrorBackground { get; init; }
        public required Color ConfirmationBackground { get; init; }
        public required Color ConfirmationApprove { get; init; }
        public required Color ConfirmationDecline { get; init; }
    }

    public static class AppThemeManager
    {
        private static readonly AppTheme LeagueTheme = new()
        {
            WindowBackground = Color.FromArgb(26, 31, 40),
            TitleBarBackground = Color.FromArgb(19, 24, 33),
            SurfaceBackground = Color.FromArgb(43, 52, 67),
            SurfaceHoverBackground = Color.FromArgb(58, 70, 89),
            InputBackground = Color.FromArgb(34, 41, 54),
            BorderColor = Color.FromArgb(185, 146, 73),
            PrimaryText = Color.FromArgb(241, 241, 241),
            MutedText = Color.FromArgb(170, 177, 189),
            PrimaryAccent = Color.FromArgb(33, 102, 173),
            PrimaryAccentHover = Color.FromArgb(54, 124, 196),
            PrimaryAccentPressed = Color.FromArgb(20, 76, 133),
            SecondaryAccent = Color.FromArgb(197, 165, 86),
            SecondaryAccentHover = Color.FromArgb(216, 184, 107),
            SecondaryAccentPressed = Color.FromArgb(157, 129, 61),
            DangerAccent = Color.FromArgb(150, 56, 56),
            DangerAccentHover = Color.FromArgb(176, 68, 68),
            DangerAccentPressed = Color.FromArgb(120, 44, 44),
            ToggleTrackBackground = Color.FromArgb(18, 22, 29),
            ToggleTrackBorder = Color.FromArgb(111, 127, 158),
            ToggleKnobBackground = Color.FromArgb(238, 214, 146),
            ToggleKnobAccent = Color.FromArgb(25, 83, 145),
            NotificationInfoBackground = Color.FromArgb(168, 30, 84, 140),
            NotificationErrorBackground = Color.FromArgb(168, 127, 51, 51),
            ConfirmationBackground = Color.FromArgb(168, 63, 81, 120),
            ConfirmationApprove = Color.FromArgb(222, 214, 146),
            ConfirmationDecline = Color.FromArgb(255, 204, 194),
        };

        private static readonly AppTheme ValorantTheme = new()
        {
            WindowBackground = Color.FromArgb(23, 20, 23),
            TitleBarBackground = Color.FromArgb(16, 14, 17),
            SurfaceBackground = Color.FromArgb(47, 34, 40),
            SurfaceHoverBackground = Color.FromArgb(64, 43, 51),
            InputBackground = Color.FromArgb(35, 27, 31),
            BorderColor = Color.FromArgb(198, 72, 84),
            PrimaryText = Color.FromArgb(245, 240, 241),
            MutedText = Color.FromArgb(187, 170, 174),
            PrimaryAccent = Color.FromArgb(212, 68, 78),
            PrimaryAccentHover = Color.FromArgb(232, 84, 95),
            PrimaryAccentPressed = Color.FromArgb(164, 50, 59),
            SecondaryAccent = Color.FromArgb(127, 42, 51),
            SecondaryAccentHover = Color.FromArgb(147, 53, 63),
            SecondaryAccentPressed = Color.FromArgb(95, 31, 38),
            DangerAccent = Color.FromArgb(124, 40, 49),
            DangerAccentHover = Color.FromArgb(151, 48, 58),
            DangerAccentPressed = Color.FromArgb(96, 31, 38),
            ToggleTrackBackground = Color.FromArgb(18, 16, 18),
            ToggleTrackBorder = Color.FromArgb(111, 79, 85),
            ToggleKnobBackground = Color.FromArgb(234, 86, 97),
            ToggleKnobAccent = Color.FromArgb(255, 227, 229),
            NotificationInfoBackground = Color.FromArgb(168, 150, 54, 62),
            NotificationErrorBackground = Color.FromArgb(168, 170, 54, 64),
            ConfirmationBackground = Color.FromArgb(168, 102, 41, 51),
            ConfirmationApprove = Color.FromArgb(255, 225, 228),
            ConfirmationDecline = Color.FromArgb(255, 202, 202),
        };

        public static AppTheme CurrentTheme { get; private set; } = LeagueTheme;

        public static void SetGame(RiotGameProduct game)
        {
            CurrentTheme = game == RiotGameProduct.Valorant ? ValorantTheme : LeagueTheme;
        }
    }

    public static class ThemeStyler
    {
        public static void ApplyPrimaryButton(Button button, AppTheme theme)
        {
            button.ForeColor = Color.White;
            button.BackColor = theme.PrimaryAccent;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = theme.PrimaryAccentHover;
            button.FlatAppearance.MouseDownBackColor = theme.PrimaryAccentPressed;
        }

        public static void ApplySecondaryButton(Button button, AppTheme theme)
        {
            button.ForeColor = theme.PrimaryText;
            button.BackColor = theme.SecondaryAccent;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = theme.SecondaryAccentHover;
            button.FlatAppearance.MouseDownBackColor = theme.SecondaryAccentPressed;
        }

        public static void ApplyDangerButton(Button button, AppTheme theme)
        {
            button.ForeColor = Color.White;
            button.BackColor = theme.DangerAccent;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = theme.DangerAccentHover;
            button.FlatAppearance.MouseDownBackColor = theme.DangerAccentPressed;
        }

        public static void ApplyNeutralButton(Button button, AppTheme theme)
        {
            button.ForeColor = theme.PrimaryText;
            button.BackColor = theme.SurfaceHoverBackground;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Lighten(theme.SurfaceHoverBackground, 0.08);
            button.FlatAppearance.MouseDownBackColor = Darken(theme.SurfaceHoverBackground, 0.1);
        }

        public static void ApplyInput(TextBox textBox, AppTheme theme)
        {
            textBox.BackColor = theme.InputBackground;
            textBox.ForeColor = theme.PrimaryText;
        }

        public static Color Lighten(Color color, double amount)
        {
            return Blend(color, Color.White, amount);
        }

        public static Color Darken(Color color, double amount)
        {
            return Blend(color, Color.Black, amount);
        }

        private static Color Blend(Color from, Color to, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            int red = (int)(from.R + ((to.R - from.R) * amount));
            int green = (int)(from.G + ((to.G - from.G) * amount));
            int blue = (int)(from.B + ((to.B - from.B) * amount));
            return Color.FromArgb(from.A, red, green, blue);
        }
    }
}