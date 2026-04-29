using NUnit.Framework;
using Chess.Core.Board;

namespace Chess.Core.Tests
{
    public class BoardStateTests
    {
        [Test]
        public void EmptyHasNoOccupancy()
        {
            var board = new BoardState();
            Assert.AreEqual(0UL, board.AllOccupancy);
        }

        [Test]
        public void PlacingWhitePawnSetsBitInWhitePawnAndAllOccupancy()
        {
            var board = new BoardState();
            board.PlacePiece(PieceType.Pawn, PieceColor.White, Square.FromAlgebraic("e2"));
            Assert.AreEqual(1UL << 12, board.BitboardFor(PieceType.Pawn, PieceColor.White));
            Assert.AreEqual(1UL << 12, board.WhiteOccupancy);
            Assert.AreEqual(0UL, board.BlackOccupancy);
            Assert.AreEqual(1UL << 12, board.AllOccupancy);
        }

        [Test]
        public void RemovePieceClearsBit()
        {
            var board = new BoardState();
            int squareIndex = Square.FromAlgebraic("e2");
            board.PlacePiece(PieceType.Pawn, PieceColor.White, squareIndex);
            board.RemovePiece(PieceType.Pawn, PieceColor.White, squareIndex);
            Assert.AreEqual(0UL, board.AllOccupancy);
        }
    }
}
