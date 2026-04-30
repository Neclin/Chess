using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class SearchTests
    {
        private const string MateInOneFen = "6k1/5ppp/8/8/8/8/8/4Q1K1 w - - 0 1";

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
    }
}
