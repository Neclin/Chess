using NUnit.Framework;
using System.Collections.Generic;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Tests
{
    public class CastlingTests
    {
        [Test]
        public void WhiteCanCastleKingside_WhenPathClear()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/4K2R w K - 0 1");
            var moves = new List<Move>();
            MoveGenerator.GeneratePseudoLegal(board, moves);
            Assert.IsTrue(moves.Exists(move =>
                move.FromSquare == Square.FromAlgebraic("e1") && move.ToSquare == Square.FromAlgebraic("g1") &&
                (move.Flags & MoveFlags.CastleKingside) != 0));
        }

        [Test]
        public void WhiteCannotCastleKingside_IfPathThroughCheck()
        {
            var board = FenParser.Parse("4k1r1/8/8/8/8/8/8/4K2R w K - 0 1");
            var moves = new List<Move>();
            MoveGenerator.GeneratePseudoLegal(board, moves);
            Assert.IsFalse(moves.Exists(move =>
                move.FromSquare == Square.FromAlgebraic("e1") && move.ToSquare == Square.FromAlgebraic("g1")));
        }

        [Test]
        public void WhiteCannotCastleQueenside_IfB1Occupied()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/RN2K3 w Q - 0 1");
            var moves = new List<Move>();
            MoveGenerator.GeneratePseudoLegal(board, moves);
            Assert.IsFalse(moves.Exists(move =>
                move.FromSquare == Square.FromAlgebraic("e1") && move.ToSquare == Square.FromAlgebraic("c1")));
        }
    }
}
