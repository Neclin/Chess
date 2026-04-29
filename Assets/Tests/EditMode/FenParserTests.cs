using NUnit.Framework;
using Chess.Core.Board;

namespace Chess.Core.Tests
{
    public class FenParserTests
    {
        public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        [Test]
        public void StartPosition_HasSixteenPawns()
        {
            var board = FenParser.Parse(StartFen);
            int pawnCount = Bitboard.CountSetBits(board.BitboardFor(PieceType.Pawn, PieceColor.White))
                          + Bitboard.CountSetBits(board.BitboardFor(PieceType.Pawn, PieceColor.Black));
            Assert.AreEqual(16, pawnCount);
        }

        [Test]
        public void StartPosition_WhiteToMove()
        {
            Assert.AreEqual(PieceColor.White, FenParser.Parse(StartFen).SideToMove);
        }

        [Test]
        public void StartPosition_AllCastlingRights()
        {
            Assert.AreEqual(CastlingRights.All, FenParser.Parse(StartFen).Castling);
        }

        [Test]
        public void Kiwipete_RoundTrips()
        {
            const string fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            Assert.AreEqual(fen, FenParser.ToFen(FenParser.Parse(fen)));
        }

        [Test]
        public void EnPassantSquare_Parses()
        {
            var board = FenParser.Parse("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3");
            Assert.AreEqual(Square.FromAlgebraic("d6"), board.EnPassantSquare);
        }
    }
}
