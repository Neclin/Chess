using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class EvaluatorTests
    {
        [Test]
        public void StartPosition_IsApproximatelyZero()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            Assert.That(System.Math.Abs(Evaluator.Evaluate(board)), Is.LessThan(50));
        }

        [Test]
        public void ExtraQueen_FavoursWhiteWhenWhiteToMove()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/3QK3 w - - 0 1");
            Assert.That(Evaluator.Evaluate(board), Is.GreaterThan(800));
        }

        [Test]
        public void ExtraQueen_FavoursBlackWhenWhiteToMove()
        {
            var board = FenParser.Parse("3qk3/8/8/8/8/8/8/4K3 w - - 0 1");
            Assert.That(Evaluator.Evaluate(board), Is.LessThan(-800));
        }

        [Test]
        public void Evaluate_FlipsSign_WhenSideToMoveFlips()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/3QK3 w - - 0 1");
            int scoreWithWhiteToMove = Evaluator.Evaluate(board);
            board.SideToMove = PieceColor.Black;
            int scoreWithBlackToMove = Evaluator.Evaluate(board);
            Assert.AreEqual(scoreWithWhiteToMove, -scoreWithBlackToMove);
        }

        [Test]
        public void KnightInCenter_ScoresHigherThanKnightOnRim()
        {
            var withCenterKnight = FenParser.Parse("4k3/8/8/8/4N3/8/8/4K3 w - - 0 1");
            var withRimKnight = FenParser.Parse("4k3/8/8/8/8/8/8/N3K3 w - - 0 1");
            Assert.That(Evaluator.Evaluate(withCenterKnight), Is.GreaterThan(Evaluator.Evaluate(withRimKnight)));
        }
    }
}
