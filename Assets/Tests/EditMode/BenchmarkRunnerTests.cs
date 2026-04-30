using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Search;
using Chess.Tools;

namespace Chess.Core.Tests
{
    public class BenchmarkRunnerTests
    {
        private const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const string KiwipeteFen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

        [TearDown]
        public void RestoreDefaultSearchFlags()
        {
            MinimaxSearch.ConfigFlags(useTranspositionTable: true, useMoveOrdering: true, useMagicBitboards: true);
        }

        private static readonly string[] ExpectedHeaderColumns =
        {
            "fen", "depth", "nodes", "timeMs", "nps", "bestMove",
            "useTT", "useOrdering", "useMagic", "peakBytes", "ttFillPct"
        };

        [Test]
        public void EmitsExpectedCsvHeader()
        {
            string csvOutput = BenchmarkRunner.RunCsv(new[] { StartFen }, depth: 1, useTranspositionTable: true, useMoveOrdering: true, useMagicBitboards: true);

            string headerLine = SplitLines(csvOutput)[0];
            Assert.AreEqual(string.Join(",", ExpectedHeaderColumns), headerLine);
        }

        [Test]
        public void EmitsOneRowPerFenPlusHeader()
        {
            string[] inputFens = { StartFen, KiwipeteFen };

            string csvOutput = BenchmarkRunner.RunCsv(inputFens, depth: 1, useTranspositionTable: true, useMoveOrdering: true, useMagicBitboards: true);

            List<string> lines = SplitLines(csvOutput);
            Assert.AreEqual(inputFens.Length + 1, lines.Count);
        }

        [Test]
        public void RowFlagColumnsReflectArguments()
        {
            string csvOutput = BenchmarkRunner.RunCsv(new[] { StartFen }, depth: 1, useTranspositionTable: false, useMoveOrdering: true, useMagicBitboards: false);

            string dataRow = SplitLines(csvOutput)[1];
            string[] columns = ParseCsvRow(dataRow);

            Assert.AreEqual("False", columns[6]);
            Assert.AreEqual("True", columns[7]);
            Assert.AreEqual("False", columns[8]);
        }

        [Test]
        public void BestMoveColumnIsLegalInListedPosition()
        {
            string csvOutput = BenchmarkRunner.RunCsv(new[] { StartFen, KiwipeteFen }, depth: 2, useTranspositionTable: true, useMoveOrdering: true, useMagicBitboards: true);

            List<string> lines = SplitLines(csvOutput);
            for (int rowIndex = 1; rowIndex < lines.Count; rowIndex++)
            {
                string[] columns = ParseCsvRow(lines[rowIndex]);
                string fen = columns[0];
                string bestMoveText = columns[5];

                var board = FenParser.Parse(fen);
                var legalMoves = new List<Move>(64);
                MoveGenerator.GenerateLegal(board, legalMoves);
                Assert.That(legalMoves.Any(legalMove => legalMove.ToString() == bestMoveText), $"bestMove '{bestMoveText}' not legal in '{fen}'");
            }
        }

        [Test]
        public void TogglingMagicBitboardsProducesIdenticalNodeCount()
        {
            string magicCsv = BenchmarkRunner.RunCsv(new[] { KiwipeteFen }, depth: 2, useTranspositionTable: false, useMoveOrdering: false, useMagicBitboards: true);
            string slowCsv = BenchmarkRunner.RunCsv(new[] { KiwipeteFen }, depth: 2, useTranspositionTable: false, useMoveOrdering: false, useMagicBitboards: false);

            string magicNodes = ParseCsvRow(SplitLines(magicCsv)[1])[2];
            string slowNodes = ParseCsvRow(SplitLines(slowCsv)[1])[2];
            Assert.AreEqual(magicNodes, slowNodes);
        }

        private static List<string> SplitLines(string csvOutput)
        {
            return csvOutput.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static string[] ParseCsvRow(string row)
        {
            var columns = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;
            foreach (char character in row)
            {
                if (character == '"') { inQuotes = !inQuotes; continue; }
                if (character == ',' && !inQuotes) { columns.Add(current.ToString()); current.Clear(); continue; }
                current.Append(character);
            }
            columns.Add(current.ToString());
            return columns.ToArray();
        }
    }
}
