using System;
using System.Collections.Generic;
using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Notation;

namespace Chess.Core.Tests
{
    public class SanTests
    {
        [Test]
        public void PawnPushFromStart_IsBareDestinationSquare()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            var move = FindLegalMove(board, "e2", "e4");
            Assert.AreEqual("e4", San.ToSan(board, move));
        }

        [Test]
        public void KnightMoveWithoutDisambiguation_UsesPieceLetterPlusDestination()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            var move = FindLegalMove(board, "g1", "f3");
            Assert.AreEqual("Nf3", San.ToSan(board, move));
        }

        [Test]
        public void CastleKingside_IsOhDashOh()
        {
            var board = FenParser.Parse("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            var move = FindLegalMove(board, "e1", "g1");
            Assert.AreEqual("O-O", San.ToSan(board, move));
        }

        [Test]
        public void CastleQueenside_IsOhDashOhDashOh()
        {
            var board = FenParser.Parse("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
            var move = FindLegalMove(board, "e1", "c1");
            Assert.AreEqual("O-O-O", San.ToSan(board, move));
        }

        [Test]
        public void QueenCapture_IsPieceLetterXDestination()
        {
            var board = FenParser.Parse("k7/8/8/4p3/8/8/4Q3/4K3 w - - 0 1");
            var move = FindLegalMove(board, "e2", "e5");
            Assert.AreEqual("Qxe5", San.ToSan(board, move));
        }

        [Test]
        public void PawnPromotionToQueen_IsDestinationEqualsQ()
        {
            var board = FenParser.Parse("8/4P1k1/8/8/8/8/8/4K3 w - - 0 1");
            var move = FindLegalMove(board, "e7", "e8", PieceType.Queen);
            Assert.AreEqual("e8=Q", San.ToSan(board, move));
        }

        [Test]
        public void TwoKnightsBothReachDestination_FileDisambiguates()
        {
            var board = FenParser.Parse("4k3/8/8/8/8/5N2/8/1N2K3 w - - 0 1");
            var move = FindLegalMove(board, "b1", "d2");
            Assert.AreEqual("Nbd2", San.ToSan(board, move));
        }

        [Test]
        public void TwoRooksOnSameFileBothReachDestination_RankDisambiguates()
        {
            var board = FenParser.Parse("k7/8/8/8/8/4R3/4p3/4R2K w - - 0 1");
            var move = FindLegalMove(board, "e1", "e2");
            Assert.AreEqual("R1xe2", San.ToSan(board, move));
        }

        [Test]
        public void MoveGivingCheck_AppendsPlus()
        {
            var board = FenParser.Parse("8/8/8/4k3/8/8/8/4K1N1 w - - 0 1");
            var move = FindLegalMove(board, "g1", "f3");
            Assert.AreEqual("Nf3+", San.ToSan(board, move));
        }

        [Test]
        public void MoveGivingCheckmate_AppendsHash()
        {
            var board = FenParser.Parse("rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq g3 0 2");
            var move = FindLegalMove(board, "d8", "h4");
            Assert.AreEqual("Qh4#", San.ToSan(board, move));
        }

        [Test]
        public void PawnCapture_IncludesFromFileAndX()
        {
            var board = FenParser.Parse("4k3/8/8/3p4/4P3/8/8/4K3 w - - 0 1");
            var move = FindLegalMove(board, "e4", "d5");
            Assert.AreEqual("exd5", San.ToSan(board, move));
        }

        [Test]
        public void EnPassantCapture_IsFormattedAsPawnCapture()
        {
            var board = FenParser.Parse("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1");
            var move = FindLegalMove(board, "e5", "d6");
            Assert.AreEqual("exd6", San.ToSan(board, move));
        }

        private static Move FindLegalMove(BoardState board, string fromAlgebraic, string toAlgebraic, PieceType promotionPiece = PieceType.None)
        {
            int fromSquareIndex = Square.FromAlgebraic(fromAlgebraic);
            int toSquareIndex = Square.FromAlgebraic(toAlgebraic);
            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            foreach (var legalMove in legalMoves)
            {
                if (legalMove.FromSquare == fromSquareIndex
                    && legalMove.ToSquare == toSquareIndex
                    && legalMove.PromotionPiece == promotionPiece)
                    return legalMove;
            }
            throw new ArgumentException($"No legal move from {fromAlgebraic} to {toAlgebraic} with promotion {promotionPiece}");
        }
    }
}
