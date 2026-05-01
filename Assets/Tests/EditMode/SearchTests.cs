using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class SearchTests
    {
        private const string MateInOneFen = "6k1/5ppp/8/8/8/8/8/4Q1K1 w - - 0 1";
        private const string KiwipeteFen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

        [TearDown]
        public void RestoreDefaultSearchFlags()
        {
            MinimaxSearch.ConfigFlags(useTranspositionTable: true, useMoveOrdering: true, useMagicBitboards: true);
            MinimaxSearch.ResetTTStats();
        }

        [Test]
        public void MateInOne_FindsCorrectMove()
        {
            var board = FenParser.Parse(MateInOneFen);
            var searchResult = MinimaxSearch.FindBestMove(board, depth: 3);
            Assert.AreEqual(Square.FromAlgebraic("e8"), searchResult.BestMove.ToSquare);
        }

        [Test]
        public void MateInOne_ScoreIsMateScore()
        {
            var board = FenParser.Parse(MateInOneFen);
            var searchResult = MinimaxSearch.FindBestMove(board, depth: 3);
            Assert.That(searchResult.Score, Is.GreaterThan(MinimaxSearch.MateScore - 1000));
        }

        [Test]
        public void RootStalemate_ReturnsZeroScore()
        {
            var board = FenParser.Parse("k7/2Q5/1K6/8/8/8/8/8 b - - 0 1");
            var searchResult = MinimaxSearch.FindBestMove(board, depth: 1);
            Assert.AreEqual(0, searchResult.Score);
        }

        [Test]
        public void RootCheckmate_ReturnsNegativeMateScore()
        {
            var board = FenParser.Parse("rnb1kbnr/pppp1ppp/8/4p3/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 1 3");
            var searchResult = MinimaxSearch.FindBestMove(board, depth: 1);
            Assert.AreEqual(-MinimaxSearch.MateScore, searchResult.Score);
        }

        [Test]
        public void CapturesUndefendedQueen()
        {
            var board = FenParser.Parse("4k3/8/8/8/3q4/8/8/3QK3 w - - 0 1");
            var searchResult = MinimaxSearch.FindBestMove(board, depth: 2);
            Assert.AreEqual(Square.FromAlgebraic("d4"), searchResult.BestMove.ToSquare);
        }

        [Test]
        public void PrefersHigherValueCaptureWhenBothHang()
        {
            var board = FenParser.Parse("k7/8/8/8/4q3/8/8/n3Q2K w - - 0 1");
            var searchResult = MinimaxSearch.FindBestMove(board, depth: 2);
            Assert.AreEqual(Square.FromAlgebraic("e4"), searchResult.BestMove.ToSquare);
        }

        [Test]
        public void FindBestMove_WithoutDeadline_DoesNotReportAborted()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            var resultWithoutDeadline = MinimaxSearch.FindBestMove(board, depth: 3);

            Assert.IsFalse(resultWithoutDeadline.Aborted);
            Assert.IsFalse(resultWithoutDeadline.HasUsablePartialResult);
        }

        [Test]
        public void FindBestMove_WithGenerousDeadline_BehavesAsDeadlineLessSearch()
        {
            var boardWithoutDeadline = FenParser.Parse(FenParserTests.StartFen);
            var resultWithoutDeadline = MinimaxSearch.FindBestMove(boardWithoutDeadline, depth: 3);

            var boardWithGenerousDeadline = FenParser.Parse(FenParserTests.StartFen);
            var generousDeadline = new SearchDeadline(Stopwatch.StartNew(), deadlineMilliseconds: 60_000);
            var resultWithGenerousDeadline = MinimaxSearch.FindBestMove(boardWithGenerousDeadline, depth: 3, generousDeadline);

            Assert.AreEqual(resultWithoutDeadline.BestMove.ToString(), resultWithGenerousDeadline.BestMove.ToString());
            Assert.IsFalse(resultWithGenerousDeadline.Aborted);
        }

        [Test]
        public void FindBestMove_WithExpiredDeadline_ReturnsAborted()
        {
            var board = FenParser.Parse(KiwipeteFen);
            var elapsedTimer = Stopwatch.StartNew();
            var oneMillisecondDeadline = new SearchDeadline(elapsedTimer, deadlineMilliseconds: 1);

            var abortedResult = MinimaxSearch.FindBestMove(board, depth: 6, oneMillisecondDeadline);

            Assert.IsTrue(abortedResult.Aborted);
            Assert.Less(elapsedTimer.ElapsedMilliseconds, 500, "abort should land within 500ms even on a 1ms budget");
        }

        [Test]
        public void FindBestMove_WithExpiredDeadline_LeavesBoardStateUnchanged()
        {
            var board = FenParser.Parse(KiwipeteFen);
            ulong zobristBefore = board.ZobristKey;
            int fullmoveBefore = board.FullmoveNumber;
            PieceColor sideToMoveBefore = board.SideToMove;

            var oneMillisecondDeadline = new SearchDeadline(Stopwatch.StartNew(), deadlineMilliseconds: 1);
            MinimaxSearch.FindBestMove(board, depth: 6, oneMillisecondDeadline);

            Assert.AreEqual(zobristBefore, board.ZobristKey, "abort must leave zobrist key unchanged (try/finally restores moves)");
            Assert.AreEqual(fullmoveBefore, board.FullmoveNumber);
            Assert.AreEqual(sideToMoveBefore, board.SideToMove);
        }

        [Test]
        public void FindBestMove_PartialPvCarriesLegalMoveWhenRegimeHit()
        {
            var board = FenParser.Parse(KiwipeteFen);
            var elapsedTimer = Stopwatch.StartNew();
            var moderateDeadline = new SearchDeadline(elapsedTimer, deadlineMilliseconds: 500);

            var partialResult = MinimaxSearch.FindBestMove(board, depth: 6, moderateDeadline);

            Assume.That(partialResult.Aborted, "test regime: search should abort within 500ms at depth 6 (skip if hardware completes the search)");
            Assume.That(partialResult.HasUsablePartialResult, "test regime: at least one root move should complete in 500ms (skip if hardware is too slow for any single root move)");

            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            Assert.That(legalMoves.Any(legalMove => legalMove.ToString() == partialResult.BestMove.ToString()),
                $"partial best move '{partialResult.BestMove}' must be legal in Kiwipete");
        }

        [Test]
        public void FindBestMove_NeverExpiringDeadline_NeverFlagsPartial()
        {
            var board = FenParser.Parse(MateInOneFen);
            var generousDeadline = new SearchDeadline(Stopwatch.StartNew(), deadlineMilliseconds: 60_000);

            var result = MinimaxSearch.FindBestMove(board, depth: 3, generousDeadline);

            Assert.IsFalse(result.Aborted);
            Assert.IsFalse(result.HasUsablePartialResult);
        }
    }
}
