using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class IterativeDeepeningTests
    {
        private const string KiwipeteFen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
        private const string MateInOneFen = "6k1/5ppp/8/8/8/8/8/4Q1K1 w - - 0 1";

        [TearDown]
        public void RestoreDefaultSearchFlags()
        {
            MinimaxSearch.ConfigFlags(useTranspositionTable: true, useMoveOrdering: true, useMagicBitboards: true);
            MinimaxSearch.ResetTTStats();
        }

        [Test]
        public void RespectsTimeBudget()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var deepeningResult = IterativeDeepening.Search(board, maxDepth: 64, timeMs: 200);
            stopwatch.Stop();

            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500));
            Assert.That(deepeningResult.DepthReached, Is.GreaterThan(0));
            Assert.AreNotEqual(deepeningResult.BestMove.FromSquare, deepeningResult.BestMove.ToSquare);
        }

        [Test]
        public void RespectsMaxDepthCap()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            var deepeningResult = IterativeDeepening.Search(board, maxDepth: 2, timeMs: 5000);
            Assert.That(deepeningResult.DepthReached, Is.LessThanOrEqualTo(2));
        }

        [Test]
        public void FindsMateInOne()
        {
            var board = FenParser.Parse(MateInOneFen);
            var deepeningResult = IterativeDeepening.Search(board, maxDepth: 64, timeMs: 200);

            Assert.That(deepeningResult.Score, Is.GreaterThan(MinimaxSearch.MateScore - 1000));
            Assert.AreEqual(Square.FromAlgebraic("e8"), deepeningResult.BestMove.ToSquare);
        }

        [Test]
        public void HigherBudgetReachesAtLeastAsDeep()
        {
            var shortBudgetBoard = FenParser.Parse(KiwipeteFen);
            var shortBudgetResult = IterativeDeepening.Search(shortBudgetBoard, maxDepth: 64, timeMs: 200);

            var longBudgetBoard = FenParser.Parse(KiwipeteFen);
            var longBudgetResult = IterativeDeepening.Search(longBudgetBoard, maxDepth: 64, timeMs: 1000);

            Assert.That(longBudgetResult.DepthReached, Is.GreaterThanOrEqualTo(shortBudgetResult.DepthReached));
        }

        [Test]
        public void StrictTimeBudget_ReturnsWithinSlack()
        {
            var board = FenParser.Parse(KiwipeteFen);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            IterativeDeepening.Search(board, maxDepth: 64, timeMs: 200);
            stopwatch.Stop();

            Assert.Less(stopwatch.ElapsedMilliseconds, 500, "ID should return within budget + ~300ms slack");
        }

        [Test]
        public void TightBudget_ReturnsLegalMoveAndDepthAtLeastOne()
        {
            var board = FenParser.Parse(KiwipeteFen);
            var deepeningResult = IterativeDeepening.Search(board, maxDepth: 64, timeMs: 80);

            Assert.GreaterOrEqual(deepeningResult.DepthReached, 1,
                "even at 80ms ID should report at least depth 1 — partial-PV must keep depth-N when available");

            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            Assert.That(legalMoves.Any(legalMove => legalMove.ToString() == deepeningResult.BestMove.ToString()),
                $"returned best move '{deepeningResult.BestMove}' must be legal in the position");
        }

        [Test]
        public void LeavesBoardStateUnchangedOnTightBudget()
        {
            var board = FenParser.Parse(KiwipeteFen);
            ulong zobristBefore = board.ZobristKey;
            int fullmoveBefore = board.FullmoveNumber;

            IterativeDeepening.Search(board, maxDepth: 64, timeMs: 50);

            Assert.AreEqual(zobristBefore, board.ZobristKey);
            Assert.AreEqual(fullmoveBefore, board.FullmoveNumber);
        }
    }
}
