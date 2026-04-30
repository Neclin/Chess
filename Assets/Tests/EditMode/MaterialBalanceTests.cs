using System.Collections.Generic;
using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Rules;

namespace Chess.Core.Tests
{
    public class MaterialBalanceTests
    {
        [Test]
        public void StandardPieceValues()
        {
            Assert.AreEqual(1, MaterialBalance.Value(PieceType.Pawn));
            Assert.AreEqual(3, MaterialBalance.Value(PieceType.Knight));
            Assert.AreEqual(3, MaterialBalance.Value(PieceType.Bishop));
            Assert.AreEqual(5, MaterialBalance.Value(PieceType.Rook));
            Assert.AreEqual(9, MaterialBalance.Value(PieceType.Queen));
            Assert.AreEqual(0, MaterialBalance.Value(PieceType.King));
            Assert.AreEqual(0, MaterialBalance.Value(PieceType.None));
        }

        [Test]
        public void EmptyCaptureListGivesZeroScore()
        {
            int score = MaterialBalance.WhiteMinusBlackScore(new List<CapturedPiece>());
            Assert.AreEqual(0, score);
        }

        [Test]
        public void WhiteCapturesPawn_PositiveScore()
        {
            var captures = new List<CapturedPiece>
            {
                new CapturedPiece(PieceType.Pawn, PieceColor.White)
            };
            int score = MaterialBalance.WhiteMinusBlackScore(captures);
            Assert.AreEqual(1, score);
        }

        [Test]
        public void BlackCapturesQueen_NegativeScore()
        {
            var captures = new List<CapturedPiece>
            {
                new CapturedPiece(PieceType.Queen, PieceColor.Black)
            };
            int score = MaterialBalance.WhiteMinusBlackScore(captures);
            Assert.AreEqual(-9, score);
        }

        [Test]
        public void MixedCaptures_NetDifference()
        {
            var captures = new List<CapturedPiece>
            {
                new CapturedPiece(PieceType.Pawn, PieceColor.White),
                new CapturedPiece(PieceType.Pawn, PieceColor.White),
                new CapturedPiece(PieceType.Knight, PieceColor.White),
                new CapturedPiece(PieceType.Rook, PieceColor.Black)
            };
            int score = MaterialBalance.WhiteMinusBlackScore(captures);
            Assert.AreEqual(0, score);
        }
    }
}
