#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Search;

namespace Chess.Tools
{
    public static class IterativeDeepeningCsvExporter
    {
        private const string OutputCsvPath = "docs/data/iterative-deepening.csv";

        private struct BenchmarkPosition
        {
            public string PositionName;
            public string Fen;
        }

        private static readonly BenchmarkPosition[] BenchmarkPositions =
        {
            new BenchmarkPosition
            {
                PositionName = "start",
                Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            },
            new BenchmarkPosition
            {
                PositionName = "kiwipete",
                Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"
            },
            new BenchmarkPosition
            {
                PositionName = "endgame",
                Fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1"
            },
            new BenchmarkPosition
            {
                PositionName = "tactical",
                Fen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"
            }
        };

        private static readonly int[] TimeBudgetsMs = { 100, 500, 1000, 2000, 5000, 10000 };
        private static readonly int[] FixedDepths = { 3, 4, 5, 6 };

        [MenuItem("Tools/Chess/Export Iterative Deepening CSV")]
        public static void ExportIterativeDeepeningCsv()
        {
            var totalStopwatch = Stopwatch.StartNew();
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("position,mode,timeBudgetMs,depthReached,nodes,timeMs,bestMove");

            foreach (var benchmarkPosition in BenchmarkPositions)
            {
                UnityEngine.Debug.Log($"[ID] === position={benchmarkPosition.PositionName} ({benchmarkPosition.Fen}) ===");

                foreach (int timeBudgetMs in TimeBudgetsMs)
                {
                    var board = FenParser.Parse(benchmarkPosition.Fen);
                    var iterationStopwatch = Stopwatch.StartNew();
                    IterativeDeepeningResult deepeningResult = IterativeDeepening.Search(board, maxDepth: 64, timeMs: timeBudgetMs);
                    iterationStopwatch.Stop();

                    csvBuilder.Append(benchmarkPosition.PositionName).Append(',')
                              .Append("id,")
                              .Append(timeBudgetMs).Append(',')
                              .Append(deepeningResult.DepthReached).Append(',')
                              .Append(deepeningResult.TotalNodes).Append(',')
                              .Append(iterationStopwatch.ElapsedMilliseconds).Append(',')
                              .Append(deepeningResult.BestMove)
                              .AppendLine();

                    UnityEngine.Debug.Log($"[ID] {benchmarkPosition.PositionName} budget={timeBudgetMs}ms depth={deepeningResult.DepthReached} nodes={deepeningResult.TotalNodes} elapsed={iterationStopwatch.ElapsedMilliseconds}ms move={deepeningResult.BestMove}");
                }

                foreach (int fixedDepth in FixedDepths)
                {
                    var board = FenParser.Parse(benchmarkPosition.Fen);
                    var iterationStopwatch = Stopwatch.StartNew();
                    SearchResult searchResult = MinimaxSearch.FindBestMove(board, fixedDepth);
                    iterationStopwatch.Stop();

                    csvBuilder.Append(benchmarkPosition.PositionName).Append(',')
                              .Append("fixed,,")
                              .Append(fixedDepth).Append(',')
                              .Append(searchResult.NodesVisited).Append(',')
                              .Append(iterationStopwatch.ElapsedMilliseconds).Append(',')
                              .Append(searchResult.BestMove)
                              .AppendLine();

                    UnityEngine.Debug.Log($"[Fixed] {benchmarkPosition.PositionName} depth={fixedDepth} nodes={searchResult.NodesVisited} elapsed={iterationStopwatch.ElapsedMilliseconds}ms move={searchResult.BestMove}");
                }
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullOutputPath = Path.Combine(projectRoot, OutputCsvPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath));
            File.WriteAllText(fullOutputPath, csvBuilder.ToString());

            totalStopwatch.Stop();
            UnityEngine.Debug.Log($"[ID] Wrote {fullOutputPath} in {totalStopwatch.Elapsed.TotalSeconds:F1}s");
        }
    }
}
#endif
