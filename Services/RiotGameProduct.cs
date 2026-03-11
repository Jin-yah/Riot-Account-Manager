using System.Diagnostics;

namespace RiotAccountManager.Services
{
    public enum RiotGameProduct
    {
        LeagueOfLegends,
        Valorant,
    }

    public static class RiotGameProductExtensions
    {
        public static string GetLaunchArguments(this RiotGameProduct game) =>
            game switch
            {
                RiotGameProduct.LeagueOfLegends =>
                    "--launch-product=league_of_legends --launch-patchline=live",
                RiotGameProduct.Valorant => "--launch-product=valorant --launch-patchline=live",
                _ => throw new ArgumentOutOfRangeException(nameof(game), game, null),
            };

        public static string GetAlreadyRunningMessage(this RiotGameProduct game) =>
            game switch
            {
                RiotGameProduct.LeagueOfLegends => "League of Legends is already running.",
                RiotGameProduct.Valorant => "VALORANT is already running.",
                _ => throw new ArgumentOutOfRangeException(nameof(game), game, null),
            };

        public static bool IsGameRunning(this RiotGameProduct game) =>
            game.GetProcessNames()
                .Any(processName => Process.GetProcessesByName(processName).Any());

        private static IReadOnlyList<string> GetProcessNames(this RiotGameProduct game) =>
            game switch
            {
                RiotGameProduct.LeagueOfLegends => ["LeagueClient"],
                RiotGameProduct.Valorant => ["VALORANT", "VALORANT-Win64-Shipping"],
                _ => throw new ArgumentOutOfRangeException(nameof(game), game, null),
            };
    }
}
