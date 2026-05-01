#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Tools
{
    public static class StockfishBridgeWindow
    {
        private const string StartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const string OutputCsvPath = "docs/data/perft-cross-check.csv";

        private struct StockfishPerftCase
        {
            public string PositionName;
            public string Fen;
            public int MaximumDepth;
        }

        private static readonly StockfishPerftCase[] StockfishPerftCases =
        {
            new StockfishPerftCase
            {
                PositionName = "start",
                Fen = StartPositionFen,
                MaximumDepth = 6
            },
            new StockfishPerftCase
            {
                PositionName = "kiwipete",
                Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
                MaximumDepth = 5
            },
            new StockfishPerftCase
            {
                PositionName = "position3",
                Fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1",
                MaximumDepth = 6
            },
            new StockfishPerftCase
            {
                PositionName = "position4",
                Fen = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
                MaximumDepth = 5
            },
            new StockfishPerftCase
            {
                PositionName = "position5",
                Fen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",
                MaximumDepth = 5
            },
            new StockfishPerftCase
            {
                PositionName = "edwards",
                Fen = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10",
                MaximumDepth = 5
            }
        };

        [MenuItem("Tools/Chess/Validate Perft against Stockfish")]
        public static void ValidatePerftAgainstStockfish()
        {
            using var stockfishBridge = new StockfishBridge();
            WarmUpEngines(stockfishBridge);
            for (int depth = 1; depth <= 4; depth++)
            {
                var ourEngineStopwatch = Stopwatch.StartNew();
                long ourEngineNodeCount = RunOurPerft(StartPositionFen, depth);
                ourEngineStopwatch.Stop();

                var stockfishStopwatch = Stopwatch.StartNew();
                long stockfishNodeCount = stockfishBridge.Perft(StartPositionFen, depth);
                stockfishStopwatch.Stop();

                string matchMarker = ourEngineNodeCount == stockfishNodeCount ? "OK" : "MISMATCH";
                UnityEngine.Debug.Log(
                    $"[StockfishPerft] start d{depth}: " +
                    $"ours={ourEngineNodeCount} ({FormatMilliseconds(TicksToMilliseconds(ourEngineStopwatch.ElapsedTicks))}ms) " +
                    $"stockfish={stockfishNodeCount} ({FormatMilliseconds(TicksToMilliseconds(stockfishStopwatch.ElapsedTicks))}ms) " +
                    matchMarker);
            }
        }

        [MenuItem("Tools/Chess/Export Perft Cross-Check CSV")]
        public static void ExportPerftCrossCheckCsv()
        {
            var totalStopwatch = Stopwatch.StartNew();
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("position,depth,ourNodes,stockfishNodes,match,ourTimeMs,stockfishTimeMs");

            int totalCases = 0;
            int matchingCases = 0;

            using var stockfishBridge = new StockfishBridge();
            WarmUpEngines(stockfishBridge);
            foreach (var perftCase in StockfishPerftCases)
            {
                UnityEngine.Debug.Log(
                    $"[CrossCheck] {perftCase.PositionName} ({perftCase.Fen}) up to depth {perftCase.MaximumDepth}");
                for (int depth = 1; depth <= perftCase.MaximumDepth; depth++)
                {
                    var ourEngineStopwatch = Stopwatch.StartNew();
                    long ourEngineNodeCount = RunOurPerft(perftCase.Fen, depth);
                    ourEngineStopwatch.Stop();

                    var stockfishStopwatch = Stopwatch.StartNew();
                    long stockfishNodeCount = stockfishBridge.Perft(perftCase.Fen, depth);
                    stockfishStopwatch.Stop();

                    bool matches = ourEngineNodeCount == stockfishNodeCount;
                    totalCases++;
                    if (matches) matchingCases++;

                    double ourEngineMilliseconds = TicksToMilliseconds(ourEngineStopwatch.ElapsedTicks);
                    double stockfishMilliseconds = TicksToMilliseconds(stockfishStopwatch.ElapsedTicks);

                    csvBuilder.Append(perftCase.PositionName).Append(',')
                              .Append(depth).Append(',')
                              .Append(ourEngineNodeCount).Append(',')
                              .Append(stockfishNodeCount).Append(',')
                              .Append(matches ? "TRUE" : "FALSE").Append(',')
                              .Append(FormatMilliseconds(ourEngineMilliseconds)).Append(',')
                              .Append(FormatMilliseconds(stockfishMilliseconds))
                              .AppendLine();

                    string matchMarker = matches ? "OK" : "MISMATCH";
                    UnityEngine.Debug.Log(
                        $"[CrossCheck] {perftCase.PositionName} d{depth}: " +
                        $"ours={ourEngineNodeCount} ({FormatMilliseconds(ourEngineMilliseconds)}ms) " +
                        $"stockfish={stockfishNodeCount} ({FormatMilliseconds(stockfishMilliseconds)}ms) " +
                        matchMarker);
                }
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullOutputPath = Path.Combine(projectRoot, OutputCsvPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath));
            File.WriteAllText(fullOutputPath, csvBuilder.ToString());

            totalStopwatch.Stop();
            UnityEngine.Debug.Log(
                $"[CrossCheck] Wrote {fullOutputPath} ({matchingCases}/{totalCases} matched) " +
                $"in {totalStopwatch.Elapsed.TotalSeconds:F1}s");
        }

        private static void WarmUpEngines(StockfishBridge stockfishBridge)
        {
            RunOurPerft(StartPositionFen, 2);
            stockfishBridge.Perft(StartPositionFen, 2);
        }

        private static double TicksToMilliseconds(long ticks)
        {
            return ticks * 1000.0 / Stopwatch.Frequency;
        }

        private static string FormatMilliseconds(double milliseconds)
        {
            return milliseconds.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static long RunOurPerft(string fen, int depth)
        {
            var board = FenParser.Parse(fen);
            return PerftRecursive(board, depth);
        }

        private static long PerftRecursive(BoardState board, int depth)
        {
            if (depth == 0) return 1;
            var moves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, moves);
            if (depth == 1) return moves.Count;
            long totalNodeCount = 0;
            foreach (var move in moves)
            {
                var undoInfo = MoveExecutor.MakeMove(board, move);
                totalNodeCount += PerftRecursive(board, depth - 1);
                MoveExecutor.UnmakeMove(board, move, undoInfo);
            }
            return totalNodeCount;
        }
    }
}
#endif
