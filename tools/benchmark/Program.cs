using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Chess.Tools;

namespace Chess.Benchmark
{
    public static class Program
    {
        private static readonly string[] BenchmarkFens =
        {
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
            "r1bqkb1r/pppp1ppp/2n2n2/4p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4",
            "r2q1rk1/ppp2ppp/2nbbn2/3p4/3P4/2NBBN2/PPP2PPP/R2Q1RK1 w - - 0 9",
            "8/2k5/3p4/p2P1p2/P2P1P2/8/2K5/8 w - - 0 1",
            "8/8/8/4k3/8/8/4P3/4K3 w - - 0 1",
            "rnbqkbnr/pppp1ppp/8/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2",
            "r1bqk2r/pppp1ppp/2n2n2/2b1p3/2B1P3/2N2N2/PPPP1PPP/R1BQK2R w KQkq - 4 4",
            "rnbqkbnr/pp2pppp/8/2pp4/3P4/4P3/PPP2PPP/RNBQKBNR w KQkq - 0 3",
            "4k3/p7/1p6/2p5/3p4/4P3/5P2/4K3 w - - 0 1"
        };

        private static readonly int[] DefaultBenchmarkDepths = { 5, 6 };

        public static int Main(string[] commandLineArguments)
        {
            int[] depths = ParseDepthsFromArgs(commandLineArguments);

            int totalCells = 8 * depths.Length * BenchmarkFens.Length;
            int completedCells = 0;
            var totalStopwatch = Stopwatch.StartNew();

            var fullCsvBuilder = new StringBuilder();
            fullCsvBuilder.AppendLine(BenchmarkRunner.CsvHeader);

            Console.WriteLine($"Chess.Benchmark — {totalCells} cells (8 combos x {depths.Length} depths x {BenchmarkFens.Length} fens), depths={string.Join(",", depths)}");

            for (int flagCombination = 7; flagCombination >= 0; flagCombination--)
            {
                bool useTranspositionTable = (flagCombination & 1) != 0;
                bool useMoveOrdering = (flagCombination & 2) != 0;
                bool useMagicBitboards = (flagCombination & 4) != 0;
                string flagLabel = $"TT={BinaryDigit(useTranspositionTable)} ord={BinaryDigit(useMoveOrdering)} mag={BinaryDigit(useMagicBitboards)}";

                foreach (int depth in depths)
                {
                    for (int fenIndex = 0; fenIndex < BenchmarkFens.Length; fenIndex++)
                    {
                        var cellStopwatch = Stopwatch.StartNew();
                        string singleFenCsv = BenchmarkRunner.RunCsv(
                            new[] { BenchmarkFens[fenIndex] },
                            depth,
                            useTranspositionTable,
                            useMoveOrdering,
                            useMagicBitboards);
                        cellStopwatch.Stop();

                        AppendDataRowsOnly(fullCsvBuilder, singleFenCsv);
                        completedCells++;
                        Console.WriteLine($"[{completedCells}/{totalCells}] {flagLabel} depth={depth} fen={fenIndex + 1}/{BenchmarkFens.Length} took={FormatDuration(cellStopwatch.Elapsed.TotalSeconds)}");
                    }
                }
            }

            string outputDirectoryPath = Path.GetFullPath(Path.Combine("docs", "data", "benchmark"));
            Directory.CreateDirectory(outputDirectoryPath);
            string outputFileName = $"benchmark-{DateTime.Now:yyyyMMdd-HHmm}.csv";
            string outputFilePath = Path.Combine(outputDirectoryPath, outputFileName);
            File.WriteAllText(outputFilePath, fullCsvBuilder.ToString());

            Console.WriteLine($"Wrote {outputFilePath} (total elapsed {FormatDuration(totalStopwatch.Elapsed.TotalSeconds)})");
            return 0;
        }

        private static int[] ParseDepthsFromArgs(string[] commandLineArguments)
        {
            if (commandLineArguments == null || commandLineArguments.Length == 0)
                return DefaultBenchmarkDepths;

            var parsedDepths = new System.Collections.Generic.List<int>();
            foreach (var argument in commandLineArguments)
            {
                foreach (var depthToken in argument.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (int.TryParse(depthToken, out int parsedDepth) && parsedDepth >= 1)
                        parsedDepths.Add(parsedDepth);
                }
            }
            return parsedDepths.Count > 0 ? parsedDepths.ToArray() : DefaultBenchmarkDepths;
        }

        private static string BinaryDigit(bool flag) => flag ? "1" : "0";

        private static void AppendDataRowsOnly(StringBuilder destination, string csvWithHeader)
        {
            string[] lines = csvWithHeader.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                destination.AppendLine(lines[lineIndex]);
            }
        }

        private static string FormatDuration(double totalSeconds)
        {
            if (totalSeconds < 1) return "<1s";
            int minutes = (int)(totalSeconds / 60);
            int seconds = (int)(totalSeconds % 60);
            return minutes > 0 ? $"{minutes}m{seconds:D2}s" : $"{seconds}s";
        }
    }
}
