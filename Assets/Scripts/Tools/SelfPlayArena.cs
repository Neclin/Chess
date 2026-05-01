using System;
using System.Collections.Generic;
using System.IO;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Notation;
using Chess.Core.Rules;
using Chess.Core.Search;

namespace Chess.Tools
{
    public sealed class EngineConfig
    {
        public string Name = "config";
        public bool UseTranspositionTable = true;
        public bool UseMoveOrdering = true;
        public bool UseMagicBitboards = true;
        public int Depth = 4;
        public int TimeMs = 0;
    }

    public static class SelfPlayArena
    {
        public const string StartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const int MaximumPliesBeforeAdjudication = 400;

        public sealed class MatchResult
        {
            public string White;
            public string Black;
            public string OpeningFen = StartPositionFen;
            public GameResult Outcome;
            public int Plies;
            public List<Move> Moves = new List<Move>();
        }

        public static List<MatchResult> RunMatch(EngineConfig configurationA, EngineConfig configurationB, int games,
                                                 string[] openingFens = null, int randomSeed = 1)
        {
            var matchResults = new List<MatchResult>(games);
            var openingPicker = new Random(randomSeed);

            for (int gameIndex = 0; gameIndex < games; gameIndex++)
            {
                string openingFen = openingFens != null && openingFens.Length > 0
                    ? openingFens[openingPicker.Next(openingFens.Length)]
                    : StartPositionFen;

                bool configurationAPlaysWhite = gameIndex % 2 == 0;
                EngineConfig whiteConfig = configurationAPlaysWhite ? configurationA : configurationB;
                EngineConfig blackConfig = configurationAPlaysWhite ? configurationB : configurationA;

                matchResults.Add(PlayGame(whiteConfig, blackConfig, openingFen,
                                          configurationAPlaysWhite ? configurationA.Name : configurationB.Name,
                                          configurationAPlaysWhite ? configurationB.Name : configurationA.Name));
            }
            return matchResults;
        }

        public static MatchResult PlayGame(EngineConfig whiteConfig, EngineConfig blackConfig,
                                            string startFen, string whiteName, string blackName)
        {
            var board = FenParser.Parse(startFen);
            var matchResult = new MatchResult
            {
                White = whiteName,
                Black = blackName,
                OpeningFen = startFen
            };

            MinimaxSearch.ResetTTStats();

            for (int plyIndex = 0; plyIndex < MaximumPliesBeforeAdjudication; plyIndex++)
            {
                GameResult positionResult = GameStateChecker.Evaluate(board);
                if (positionResult != GameResult.Ongoing)
                {
                    matchResult.Outcome = positionResult;
                    matchResult.Plies = plyIndex;
                    return matchResult;
                }

                EngineConfig sideToMoveConfig = board.SideToMove == PieceColor.White ? whiteConfig : blackConfig;
                MinimaxSearch.ConfigFlags(
                    sideToMoveConfig.UseTranspositionTable,
                    sideToMoveConfig.UseMoveOrdering,
                    sideToMoveConfig.UseMagicBitboards);

                Move chosenMove = sideToMoveConfig.TimeMs > 0
                    ? IterativeDeepening.Search(board, maxDepth: 64, timeMs: sideToMoveConfig.TimeMs).BestMove
                    : MinimaxSearch.FindBestMove(board, sideToMoveConfig.Depth).BestMove;

                MoveExecutor.MakeMove(board, chosenMove);
                matchResult.Moves.Add(chosenMove);
            }

            matchResult.Outcome = GameResult.DrawByFiftyMoveRule;
            matchResult.Plies = MaximumPliesBeforeAdjudication;
            return matchResult;
        }

        public static string Summarise(List<MatchResult> matchResults, string aName)
        {
            int aWins = 0, aLosses = 0, draws = 0;
            foreach (var result in matchResults)
            {
                switch (result.Outcome)
                {
                    case GameResult.WhiteWinsByCheckmate:
                        if (result.White == aName) aWins++; else aLosses++;
                        break;
                    case GameResult.BlackWinsByCheckmate:
                        if (result.Black == aName) aWins++; else aLosses++;
                        break;
                    default:
                        draws++;
                        break;
                }
            }
            return $"{aName}: {aWins}W / {aLosses}L / {draws}D over {matchResults.Count} games";
        }

        public static string WriteMatchPgn(MatchResult matchResult, string eventName, int round)
        {
            var headers = new PgnHeaders
            {
                Event = eventName,
                Site = "self-play",
                Date = DateTime.Today.ToString("yyyy.MM.dd"),
                Round = round.ToString(),
                White = matchResult.White,
                Black = matchResult.Black
            };
            var startBoard = FenParser.Parse(matchResult.OpeningFen);
            return PgnWriter.Write(startBoard, matchResult.Moves, matchResult.Outcome, headers);
        }

        public static string DumpMatchPgnsToDirectory(List<MatchResult> matchResults, string directoryPath, string eventName)
        {
            Directory.CreateDirectory(directoryPath);
            for (int gameIndex = 0; gameIndex < matchResults.Count; gameIndex++)
            {
                string fileName = $"game-{gameIndex + 1:D4}-{matchResults[gameIndex].White}-vs-{matchResults[gameIndex].Black}.pgn";
                string filePath = Path.Combine(directoryPath, SanitiseFileName(fileName));
                File.WriteAllText(filePath, WriteMatchPgn(matchResults[gameIndex], eventName, gameIndex + 1));
            }
            return directoryPath;
        }

        private static string SanitiseFileName(string fileName)
        {
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidCharacter, '_');
            return fileName;
        }
    }
}
