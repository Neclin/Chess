using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Tests
{
    public class AttackedSquareTests
    {
        [Test]
        public void StartPosition_E3_NotAttackedByBlack()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            Assert.IsFalse(MoveGenerator.IsSquareAttacked(board, Square.FromAlgebraic("e3"), PieceColor.Black));
        }

        [Test]
        public void BlackKnightOnG4_AttacksF2AndH2()
        {
            var board = FenParser.Parse("4k3/8/8/8/6n1/8/8/4K3 w - - 0 1");
            Assert.IsTrue(MoveGenerator.IsSquareAttacked(board, Square.FromAlgebraic("f2"), PieceColor.Black));
            Assert.IsTrue(MoveGenerator.IsSquareAttacked(board, Square.FromAlgebraic("h2"), PieceColor.Black));
        }

        [Test]
        public void WhiteRookOnA1_AttacksA8_OnEmptyFile()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/R3K3 w - - 0 1");
            Assert.IsTrue(MoveGenerator.IsSquareAttacked(board, Square.FromAlgebraic("a8"), PieceColor.White));
        }
    }
}
