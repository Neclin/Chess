using System.Diagnostics;
using System.Globalization;
using System.Text;
using Chess.Core.Board;
using Chess.Core.Search;

namespace Chess.Tools
{
    public static class BenchmarkRunner
    {
        public const string CsvHeader = "fen,depth,nodes,timeMs,nps,bestMove,useTT,useOrdering,useMagic,peakBytes,ttFillPct";

        public static string RunCsv(string[] fens, int depth, bool useTranspositionTable, bool useMoveOrdering, bool useMagicBitboards)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine(CsvHeader);

            foreach (var fen in fens)
            {
                var board = FenParser.Parse(fen);
                MinimaxSearch.ConfigFlags(useTranspositionTable, useMoveOrdering, useMagicBitboards);
                MinimaxSearch.ResetTTStats();

                long managedHeapBeforeSearchBytes = System.GC.GetTotalMemory(forceFullCollection: false);
                var stopwatch = Stopwatch.StartNew();
                SearchResult searchResult = MinimaxSearch.FindBestMove(board, depth);
                stopwatch.Stop();
                long managedHeapAfterSearchBytes = System.GC.GetTotalMemory(forceFullCollection: false);

                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                long nodesPerSecond = elapsedMilliseconds > 0 ? searchResult.NodesVisited * 1000L / elapsedMilliseconds : 0;
                long peakWorkingSetBytes = managedHeapBeforeSearchBytes > managedHeapAfterSearchBytes ? managedHeapBeforeSearchBytes : managedHeapAfterSearchBytes;
                double transpositionTableFillPercent = useTranspositionTable ? MinimaxSearch.GetTTFillPercent() : 0.0;

                csvBuilder.Append('"').Append(fen).Append('"').Append(',')
                          .Append(depth).Append(',')
                          .Append(searchResult.NodesVisited).Append(',')
                          .Append(elapsedMilliseconds).Append(',')
                          .Append(nodesPerSecond).Append(',')
                          .Append(searchResult.BestMove).Append(',')
                          .Append(useTranspositionTable).Append(',')
                          .Append(useMoveOrdering).Append(',')
                          .Append(useMagicBitboards).Append(',')
                          .Append(peakWorkingSetBytes).Append(',')
                          .Append(transpositionTableFillPercent.ToString("F2", CultureInfo.InvariantCulture))
                          .AppendLine();
            }

            return csvBuilder.ToString();
        }
    }
}
