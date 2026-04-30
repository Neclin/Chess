using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Rules;

namespace Chess.Core.Tests
{
    public class GameStateCheckerTests
    {
        [Test]
        public void FoolsMate_IsBlackWinsByCheckmate()
        {
            var board = FenParser.Parse("rnb1kbnr/pppp1ppp/8/4p3/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 1 3");
            Assert.AreEqual(GameResult.BlackWinsByCheckmate, GameStateChecker.Evaluate(board));
        }

        [Test]
        public void StalematedBlackKing_IsDrawByStalemate()
        {
            var board = FenParser.Parse("k7/8/1Q6/8/8/8/8/K7 b - - 0 1");
            Assert.AreEqual(GameResult.DrawByStalemate, GameStateChecker.Evaluate(board));
        }

        [Test]
        public void KingVersusKing_IsDrawByInsufficientMaterial()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/4K3 w - - 0 1");
            Assert.AreEqual(GameResult.DrawByInsufficientMaterial, GameStateChecker.Evaluate(board));
        }

        [Test]
        public void KingAndBishopVersusKing_IsDrawByInsufficientMaterial()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/4KB2 w - - 0 1");
            Assert.AreEqual(GameResult.DrawByInsufficientMaterial, GameStateChecker.Evaluate(board));
        }

        [Test]
        public void KingAndKnightVersusKing_IsDrawByInsufficientMaterial()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/4KN2 w - - 0 1");
            Assert.AreEqual(GameResult.DrawByInsufficientMaterial, GameStateChecker.Evaluate(board));
        }

        [Test]
        public void KingAndPawnVersusKing_IsNotInsufficientMaterial()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/4P3/4K3 w - - 0 1");
            Assert.AreEqual(GameResult.Ongoing, GameStateChecker.Evaluate(board));
        }

        [Test]
        public void HalfmoveClockAtOneHundred_IsDrawByFiftyMoveRule()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/4K3 w - - 100 60");
            Assert.AreEqual(GameResult.DrawByFiftyMoveRule, GameStateChecker.Evaluate(board));
        }

        [Test]
        public void StartPosition_IsOngoing()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            Assert.AreEqual(GameResult.Ongoing, GameStateChecker.Evaluate(board));
        }

        [Test]
        public void StartPosition_NotInCheck()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            Assert.IsFalse(GameStateChecker.InCheck(board));
        }

        [Test]
        public void FoolsMatePosition_WhiteIsInCheck()
        {
            var board = FenParser.Parse("rnb1kbnr/pppp1ppp/8/4p3/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 1 3");
            Assert.IsTrue(GameStateChecker.InCheck(board));
        }
    }
}
