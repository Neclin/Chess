using NUnit.Framework;
using System.Collections.Generic;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Tests
{
    public class PerftTests
    {
        [TestCase(FenParserTests.StartFen, 1, 20L)]
        [TestCase(FenParserTests.StartFen, 2, 400L)]
        [TestCase(FenParserTests.StartFen, 3, 8902L)]
        [TestCase(FenParserTests.StartFen, 4, 197281L)]
        public void StartPosition_Perft(string fen, int depth, long expectedNodeCount)
        {
            Assert.AreEqual(expectedNodeCount, Perft(FenParser.Parse(fen), depth));
        }

        [Test]
        public void Kiwipete_Perft3()
        {
            const string fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            Assert.AreEqual(97862L, Perft(FenParser.Parse(fen), 3));
        }

        public static long Perft(BoardState board, int depth)
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
