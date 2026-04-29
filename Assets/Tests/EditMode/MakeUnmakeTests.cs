using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Tests
{
    public class MakeUnmakeTests
    {
        [Test]
        public void E2E4_ChangesAndRevertsCleanly()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            string fenBefore = FenParser.ToFen(board);
            var move = new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4"), MoveFlags.DoublePawnPush);
            var undoInfo = MoveExecutor.MakeMove(board, move);
            Assert.AreEqual(PieceColor.Black, board.SideToMove);
            Assert.AreEqual(Square.FromAlgebraic("e3"), board.EnPassantSquare);
            MoveExecutor.UnmakeMove(board, move, undoInfo);
            Assert.AreEqual(fenBefore, FenParser.ToFen(board));
        }

        [Test]
        public void Capture_RevertsCapturedPiece()
        {
            var board = FenParser.Parse("4k3/8/8/3p4/4P3/8/8/4K3 w - - 0 1");
            string fenBefore = FenParser.ToFen(board);
            var move = new Move(Square.FromAlgebraic("e4"), Square.FromAlgebraic("d5"), MoveFlags.Capture);
            var undoInfo = MoveExecutor.MakeMove(board, move);
            MoveExecutor.UnmakeMove(board, move, undoInfo);
            Assert.AreEqual(fenBefore, FenParser.ToFen(board));
        }

        [Test]
        public void CastleKingside_RevertsRookAndKing()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/8/8/4K2R w K - 0 1");
            string fenBefore = FenParser.ToFen(board);
            var move = new Move(4, 6, MoveFlags.CastleKingside);
            var undoInfo = MoveExecutor.MakeMove(board, move);
            MoveExecutor.UnmakeMove(board, move, undoInfo);
            Assert.AreEqual(fenBefore, FenParser.ToFen(board));
        }

        [Test]
        public void Promotion_RevertsPawnAndRemovesPromotedPiece()
        {
            var board = FenParser.Parse("4k3/P7/8/8/8/8/8/4K3 w - - 0 1");
            string fenBefore = FenParser.ToFen(board);
            var move = new Move(
                Square.FromAlgebraic("a7"),
                Square.FromAlgebraic("a8"),
                MoveFlags.Promotion,
                PieceType.Queen);
            var undoInfo = MoveExecutor.MakeMove(board, move);
            MoveExecutor.UnmakeMove(board, move, undoInfo);
            Assert.AreEqual(fenBefore, FenParser.ToFen(board));
        }
    }
}
