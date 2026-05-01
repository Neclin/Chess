using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Chess.Core.Rules;
using Chess.Core.Search;
using Chess.Tools;

namespace Chess.SelfPlay
{
    public static class Program
    {
        private static readonly string[] DefaultRoundRobinOrder = { "baseline", "tt", "ordering", "magic", "full" };

        public static int Main(string[] commandLineArguments)
        {
            string mode = commandLineArguments.Length > 0 ? commandLineArguments[0].ToLowerInvariant() : "roundrobin";
            int gamesPerPairing = ParseIntFlag(commandLineArguments, "--games", defaultValue: 50);
            int timeBudgetMs = ParseIntFlag(commandLineArguments, "--time-ms", defaultValue: 0);
            int searchDepth = ParseIntFlag(commandLineArguments, "--depth", defaultValue: timeBudgetMs > 0 ? 64 : 4);
            string openingsPath = ParseStringFlag(commandLineArguments, "--openings", defaultValue: null);
            string runTimestamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
            string outputDirectory = ParseStringFlag(commandLineArguments, "--out",
                defaultValue: Path.GetFullPath(Path.Combine("docs", "data", "selfplay", runTimestamp)));

            string[] openingFens = LoadOpeningFensFromFile(openingsPath);
            string openingBookPath = ParseStringFlag(commandLineArguments, "--book", defaultValue: null);
            LoadOpeningBookIfRequested(openingBookPath);

            string searchDescriptor = timeBudgetMs > 0
                ? $"ID time={timeBudgetMs}ms maxDepth={searchDepth}"
                : $"fixed depth={searchDepth}";

            if (mode == "pair")
            {
                if (commandLineArguments.Length < 3)
                {
                    Console.Error.WriteLine("usage: pair <configA> <configB> [--games N] [--depth D] [--time-ms N] [--openings path] [--book path] [--out path]");
                    return 2;
                }
                EngineConfig configurationA = ResolveConfig(commandLineArguments[1], searchDepth, timeBudgetMs);
                EngineConfig configurationB = ResolveConfig(commandLineArguments[2], searchDepth, timeBudgetMs);
                if (configurationA == null || configurationB == null) return 2;

                Directory.CreateDirectory(outputDirectory);
                Console.WriteLine($"Chess.SelfPlay pair: {configurationA.Name} vs {configurationB.Name}, games={gamesPerPairing}, {searchDescriptor}");
                var allMatchResults = new List<SelfPlayArena.MatchResult>(gamesPerPairing);
                RunPairingWithProgress(configurationA, configurationB, gamesPerPairing, openingFens, outputDirectory, allMatchResults);

                string summary = SelfPlayArena.Summarise(allMatchResults, configurationA.Name);
                Console.WriteLine(summary);
                WriteSummaryCsv(outputDirectory, new List<PairingSummary> { ComputePairingSummary(configurationA, configurationB, allMatchResults) });
                return 0;
            }

            Directory.CreateDirectory(outputDirectory);
            string[] roundRobinConfigNames = ParseRoundRobinNames(commandLineArguments) ?? DefaultRoundRobinOrder;
            Console.WriteLine($"Chess.SelfPlay round-robin: configs=[{string.Join(",", roundRobinConfigNames)}], gamesPerPairing={gamesPerPairing}, {searchDescriptor}, output={outputDirectory}");

            var pairingSummaries = new List<PairingSummary>();
            int totalPairings = roundRobinConfigNames.Length * (roundRobinConfigNames.Length - 1) / 2;
            int completedPairings = 0;
            var totalStopwatch = Stopwatch.StartNew();

            for (int firstConfigIndex = 0; firstConfigIndex < roundRobinConfigNames.Length; firstConfigIndex++)
            {
                for (int secondConfigIndex = firstConfigIndex + 1; secondConfigIndex < roundRobinConfigNames.Length; secondConfigIndex++)
                {
                    EngineConfig configurationA = ResolveConfig(roundRobinConfigNames[firstConfigIndex], searchDepth, timeBudgetMs);
                    EngineConfig configurationB = ResolveConfig(roundRobinConfigNames[secondConfigIndex], searchDepth, timeBudgetMs);
                    completedPairings++;
                    Console.WriteLine($"[pairing {completedPairings}/{totalPairings}] {configurationA.Name} vs {configurationB.Name}");

                    var pairingResults = new List<SelfPlayArena.MatchResult>(gamesPerPairing);
                    RunPairingWithProgress(configurationA, configurationB, gamesPerPairing, openingFens, outputDirectory, pairingResults);

                    pairingSummaries.Add(ComputePairingSummary(configurationA, configurationB, pairingResults));
                    Console.WriteLine($"  {SelfPlayArena.Summarise(pairingResults, configurationA.Name)}");
                }
            }

            WriteSummaryCsv(outputDirectory, pairingSummaries);
            totalStopwatch.Stop();
            Console.WriteLine($"Round-robin complete in {FormatDuration(totalStopwatch.Elapsed.TotalSeconds)}. Summary → {Path.Combine(outputDirectory, "selfplay-summary.csv")}");
            return 0;
        }

        private static void RunPairingWithProgress(EngineConfig configurationA, EngineConfig configurationB, int games,
                                                    string[] openingFens, string parentOutputDirectory,
                                                    List<SelfPlayArena.MatchResult> accumulatedResults)
        {
            string pairingDirectory = Path.Combine(parentOutputDirectory, $"{configurationA.Name}-vs-{configurationB.Name}");
            Directory.CreateDirectory(pairingDirectory);

            var openingPicker = new Random(1);
            for (int gameIndex = 0; gameIndex < games; gameIndex++)
            {
                string openingFen = openingFens != null && openingFens.Length > 0
                    ? openingFens[openingPicker.Next(openingFens.Length)]
                    : SelfPlayArena.StartPositionFen;
                OpeningBook.SharedRandom = new Random(gameIndex + 1);
                bool aPlaysWhite = gameIndex % 2 == 0;
                EngineConfig whiteConfig = aPlaysWhite ? configurationA : configurationB;
                EngineConfig blackConfig = aPlaysWhite ? configurationB : configurationA;
                string whiteName = aPlaysWhite ? configurationA.Name : configurationB.Name;
                string blackName = aPlaysWhite ? configurationB.Name : configurationA.Name;

                var perGameStopwatch = Stopwatch.StartNew();
                var matchResult = SelfPlayArena.PlayGame(whiteConfig, blackConfig, openingFen, whiteName, blackName);
                perGameStopwatch.Stop();
                accumulatedResults.Add(matchResult);

                string pgnFileName = SanitiseFileName($"game-{gameIndex + 1:D4}-{whiteName}-vs-{blackName}.pgn");
                File.WriteAllText(Path.Combine(pairingDirectory, pgnFileName),
                    SelfPlayArena.WriteMatchPgn(matchResult, eventName: $"{configurationA.Name}-vs-{configurationB.Name}", round: gameIndex + 1));

                Console.WriteLine($"  game {gameIndex + 1}/{games}: {whiteName}(W) vs {blackName}(B) → {matchResult.Outcome} after {matchResult.Plies} plies ({FormatDuration(perGameStopwatch.Elapsed.TotalSeconds)})");
            }
        }

        private sealed class PairingSummary
        {
            public string AName;
            public string BName;
            public int AWins;
            public int ALosses;
            public int Draws;
            public double EloDifference;
            public double EloMargin;
            public int Games;
        }

        private static PairingSummary ComputePairingSummary(EngineConfig configurationA, EngineConfig configurationB, List<SelfPlayArena.MatchResult> matchResults)
        {
            int aWins = 0, aLosses = 0, draws = 0;
            foreach (var result in matchResults)
            {
                switch (result.Outcome)
                {
                    case GameResult.WhiteWinsByCheckmate:
                        if (result.White == configurationA.Name) aWins++; else aLosses++;
                        break;
                    case GameResult.BlackWinsByCheckmate:
                        if (result.Black == configurationA.Name) aWins++; else aLosses++;
                        break;
                    default:
                        draws++;
                        break;
                }
            }
            int totalGames = matchResults.Count;
            double aScore = aWins + 0.5 * draws;
            double bScore = aLosses + 0.5 * draws;
            double eloDifference;
            if (aScore <= 0) eloDifference = -800;
            else if (bScore <= 0) eloDifference = +800;
            else eloDifference = 400.0 * Math.Log10(aScore / bScore);
            double eloMargin = totalGames > 0 ? 400.0 / Math.Sqrt(totalGames) : 0;

            return new PairingSummary
            {
                AName = configurationA.Name,
                BName = configurationB.Name,
                AWins = aWins,
                ALosses = aLosses,
                Draws = draws,
                EloDifference = eloDifference,
                EloMargin = eloMargin,
                Games = totalGames
            };
        }

        private static void WriteSummaryCsv(string outputDirectory, List<PairingSummary> pairingSummaries)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("whiteCfg,blackCfg,wins,losses,draws,eloDiff,eloMargin,games");
            foreach (var summary in pairingSummaries)
            {
                csvBuilder.Append(summary.AName).Append(',')
                          .Append(summary.BName).Append(',')
                          .Append(summary.AWins).Append(',')
                          .Append(summary.ALosses).Append(',')
                          .Append(summary.Draws).Append(',')
                          .Append(summary.EloDifference.ToString("F1", CultureInfo.InvariantCulture)).Append(',')
                          .Append(summary.EloMargin.ToString("F1", CultureInfo.InvariantCulture)).Append(',')
                          .Append(summary.Games)
                          .AppendLine();
            }
            File.WriteAllText(Path.Combine(outputDirectory, "selfplay-summary.csv"), csvBuilder.ToString());
        }

        private static EngineConfig ResolveConfig(string configurationName, int searchDepth, int timeBudgetMs)
        {
            if (!SelfPlayArena.NamedConfigs.ContainsKey(configurationName))
            {
                Console.Error.WriteLine($"Unknown config '{configurationName}'. Known: {string.Join(", ", SelfPlayArena.NamedConfigs.Keys)}");
                return null;
            }
            return SelfPlayArena.CloneNamedConfig(configurationName, searchDepth, timeBudgetMs);
        }

        private static string[] ParseRoundRobinNames(string[] commandLineArguments)
        {
            string raw = ParseStringFlag(commandLineArguments, "--configs", defaultValue: null);
            if (string.IsNullOrEmpty(raw)) return null;
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        private static int ParseIntFlag(string[] commandLineArguments, string flagName, int defaultValue)
        {
            for (int argumentIndex = 0; argumentIndex < commandLineArguments.Length - 1; argumentIndex++)
            {
                if (string.Equals(commandLineArguments[argumentIndex], flagName, StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(commandLineArguments[argumentIndex + 1], out int parsedValue))
                    return parsedValue;
            }
            return defaultValue;
        }

        private static string ParseStringFlag(string[] commandLineArguments, string flagName, string defaultValue)
        {
            for (int argumentIndex = 0; argumentIndex < commandLineArguments.Length - 1; argumentIndex++)
            {
                if (string.Equals(commandLineArguments[argumentIndex], flagName, StringComparison.OrdinalIgnoreCase))
                    return commandLineArguments[argumentIndex + 1];
            }
            return defaultValue;
        }

        private static void LoadOpeningBookIfRequested(string openingBookPath)
        {
            if (string.IsNullOrEmpty(openingBookPath))
            {
                OpeningBook.Default = null;
                return;
            }
            if (!File.Exists(openingBookPath))
            {
                Console.Error.WriteLine($"Opening book file not found: {openingBookPath}");
                OpeningBook.Default = null;
                return;
            }
            byte[] polyglotBytes = File.ReadAllBytes(openingBookPath);
            OpeningBook.Default = OpeningBook.LoadFromBytes(polyglotBytes);
            Console.WriteLine($"Loaded opening book ({OpeningBook.Default.PositionCount} positions) from {openingBookPath}");
        }

        private static string[] LoadOpeningFensFromFile(string openingsPath)
        {
            if (string.IsNullOrEmpty(openingsPath)) return null;
            if (!File.Exists(openingsPath))
            {
                Console.Error.WriteLine($"Openings file not found: {openingsPath}");
                return null;
            }
            var collectedFens = new List<string>();
            foreach (string rawLine in File.ReadAllLines(openingsPath))
            {
                string trimmed = rawLine.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#")) continue;
                collectedFens.Add(trimmed);
            }
            return collectedFens.ToArray();
        }

        private static string SanitiseFileName(string fileName)
        {
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidCharacter, '_');
            return fileName;
        }

        private static string FormatDuration(double totalSeconds)
        {
            if (totalSeconds < 1) return $"{(int)(totalSeconds * 1000)}ms";
            int minutes = (int)(totalSeconds / 60);
            int seconds = (int)(totalSeconds % 60);
            return minutes > 0 ? $"{minutes}m{seconds:D2}s" : $"{seconds}s";
        }
    }
}
