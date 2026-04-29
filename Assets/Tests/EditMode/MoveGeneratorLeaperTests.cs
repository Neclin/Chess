using NUnit.Framework;
using System.Collections.Generic;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Tests
{
    public class MoveGeneratorLeaperTests
    {
        [Test]
        public void StartPosition_GeneratesTwentyPseudoLegalMoves()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            var moves = new List<Move>();
            MoveGenerator.GeneratePseudoLegal(board, moves);
            Assert.AreEqual(20, moves.Count);
        }

        [Test]
        public void KnightOnly_GeneratesKnightMoves()
        {
            var board = FenParser.Parse("7k/8/8/8/8/8/8/1N5K w - - 0 1");
            var moves = new List<Move>();
            MoveGenerator.GeneratePseudoLegal(board, moves);
            Assert.IsTrue(moves.Exists(move => move.FromSquare == Square.FromAlgebraic("b1") && move.ToSquare == Square.FromAlgebraic("a3")));
            Assert.IsTrue(moves.Exists(move => move.FromSquare == Square.FromAlgebraic("b1") && move.ToSquare == Square.FromAlgebraic("c3")));
            Assert.IsTrue(moves.Exists(move => move.FromSquare == Square.FromAlgebraic("b1") && move.ToSquare == Square.FromAlgebraic("d2")));
        }
    }
}
