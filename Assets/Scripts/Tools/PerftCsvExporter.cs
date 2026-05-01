#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Tools
{
    public static class PerftCsvExporter
    {
        private const string OutputCsvPath = "docs/data/perft-validation.csv";

        private struct PerftCase
        {
            public string PositionName;
            public string Fen;
            public int MaximumDepth;
            public long[] CanonicalNodeCounts;
        }

        private static readonly PerftCase[] PerftCases =
        {
            new PerftCase
            {
                PositionName = "start",
                Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
                MaximumDepth = 6,
                CanonicalNodeCounts = new long[] { 20, 400, 8902, 197281, 4865609, 119060324 }
            },
            new PerftCase
            {
                PositionName = "kiwipete",
                Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
                MaximumDepth = 5,
                CanonicalNodeCounts = new long[] { 48, 2039, 97862, 4085603, 193690690 }
            },
            new PerftCase
            {
                PositionName = "position3",
                Fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1",
                MaximumDepth = 6,
                CanonicalNodeCounts = new long[] { 14, 191, 2812, 43238, 674624, 11030083 }
            },
            new PerftCase
            {
                PositionName = "position4",
                Fen = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
                MaximumDepth = 5,
                CanonicalNodeCounts = new long[] { 6, 264, 9467, 422333, 15833292 }
            },
            new PerftCase
            {
                PositionName = "position5",
                Fen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",
                MaximumDepth = 5,
                CanonicalNodeCounts = new long[] { 44, 1486, 62379, 2103487, 89941194 }
            },
            new PerftCase
            {
                PositionName = "edwards",
                Fen = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10",
                MaximumDepth = 5,
                CanonicalNodeCounts = new long[] { 46, 2079, 89890, 3894594, 164075551 }
            }
        };

        [MenuItem("Tools/Chess/Export Perft Validation CSV")]
        public static void ExportPerftValidationCsv()
        {
            var stopwatch = Stopwatch.StartNew();
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("position,depth,nodes,timeMs");

            int totalCases = 0;
            int matchingCases = 0;

            foreach (var perftCase in PerftCases)
            {
                UnityEngine.Debug.Log($"[Perft] {perftCase.PositionName} ({perftCase.Fen}) up to depth {perftCase.MaximumDepth}");
                var board = FenParser.Parse(perftCase.Fen);
                for (int depth = 1; depth <= perftCase.MaximumDepth; depth++)
                {
                    var caseStopwatch = Stopwatch.StartNew();
                    long actualNodeCount = Perft(board, depth);
                    caseStopwatch.Stop();

                    long canonicalNodeCount = perftCase.CanonicalNodeCounts[depth - 1];
                    bool matchesCanonical = actualNodeCount == canonicalNodeCount;
                    totalCases++;
                    if (matchesCanonical) matchingCases++;

                    csvBuilder.Append(perftCase.PositionName).Append(',')
                              .Append(depth).Append(',')
                              .Append(actualNodeCount).Append(',')
                              .Append(caseStopwatch.ElapsedMilliseconds)
                              .AppendLine();

                    string matchMarker = matchesCanonical ? "OK" : $"MISMATCH (canonical={canonicalNodeCount})";
                    UnityEngine.Debug.Log($"[Perft] {perftCase.PositionName} d{depth}: nodes={actualNodeCount} elapsed={caseStopwatch.ElapsedMilliseconds}ms {matchMarker}");
                }
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullOutputPath = Path.Combine(projectRoot, OutputCsvPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath));
            File.WriteAllText(fullOutputPath, csvBuilder.ToString());

            stopwatch.Stop();
            UnityEngine.Debug.Log($"[Perft] Wrote {fullOutputPath} ({matchingCases}/{totalCases} matched canonical) in {stopwatch.Elapsed.TotalSeconds:F1}s");
        }

        [MenuItem("Tools/Chess/Divide Perft Position 4")]
        public static void DividePerftPosition4()
        {
            const string position4Fen = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
            var report = new StringBuilder();
            for (int rootDepth = 2; rootDepth <= 4; rootDepth++)
            {
                var board = FenParser.Parse(position4Fen);
                var rootMoves = new List<Move>(64);
                MoveGenerator.GenerateLegal(board, rootMoves);
                rootMoves.Sort((leftMove, rightMove) => string.CompareOrdinal(leftMove.ToString(), rightMove.ToString()));

                report.AppendLine($"=== Divide Perft Position 4 at depth {rootDepth} ===");
                long total = 0;
                foreach (var move in rootMoves)
                {
                    var undoInfo = MoveExecutor.MakeMove(board, move);
                    long subtreeNodeCount = Perft(board, rootDepth - 1);
                    MoveExecutor.UnmakeMove(board, move, undoInfo);
                    report.AppendLine($"  {move}: {subtreeNodeCount}");
                    total += subtreeNodeCount;
                }
                report.AppendLine($"  total: {total}");
                report.AppendLine();
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputPath = Path.Combine(projectRoot, "docs/data/divide-perft-position4.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllText(outputPath, report.ToString());
            UnityEngine.Debug.Log($"[Divide] Wrote {outputPath}\n{report}");
        }

        private static long Perft(BoardState board, int depth)
        {
            if (depth == 0) return 1;
            var moves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, moves);
            if (depth == 1) return moves.Count;
            long totalNodeCount = 0;
            foreach (var move in moves)
            {
                var undoInfo = MoveExecutor.MakeMove(board, move);
                totalNodeCount += Perft(board, depth - 1);
                MoveExecutor.UnmakeMove(board, move, undoInfo);
            }
            return totalNodeCount;
        }
    }
}
#endif
