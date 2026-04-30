using System.Collections.Generic;
using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Notation;

namespace Chess.Core.Tests
{
    public class MoveHistoryFormatterTests
    {
        [Test]
        public void EmptyHistoryReturnsEmptyString()
        {
            var formatted = MoveHistoryFormatter.Format(new List<string>(), PieceColor.White, 1);
            Assert.AreEqual(string.Empty, formatted);
        }

        [Test]
        public void WhiteToMoveFromStart_FourPlies_RendersTwoFullmovesEachOnOwnLine()
        {
            var sanByPly = new List<string> { "e4", "e5", "Nf3", "Nc6" };
            var formatted = MoveHistoryFormatter.Format(sanByPly, PieceColor.White, 1);
            Assert.AreEqual("1. e4 e5\n2. Nf3 Nc6", formatted);
        }

        [Test]
        public void WhiteToMoveFromStart_OddNumberOfPlies_LastFullmoveHasOnlyWhiteMove()
        {
            var sanByPly = new List<string> { "e4", "e5", "Nf3" };
            var formatted = MoveHistoryFormatter.Format(sanByPly, PieceColor.White, 1);
            Assert.AreEqual("1. e4 e5\n2. Nf3", formatted);
        }

        [Test]
        public void WhiteToMoveFromStart_SinglePly_OnlyMoveNumberAndWhiteMove()
        {
            var sanByPly = new List<string> { "e4" };
            var formatted = MoveHistoryFormatter.Format(sanByPly, PieceColor.White, 1);
            Assert.AreEqual("1. e4", formatted);
        }

        [Test]
        public void BlackToMoveFromCustomFen_FirstPlyShownWithEllipsis()
        {
            var sanByPly = new List<string> { "Nc6" };
            var formatted = MoveHistoryFormatter.Format(sanByPly, PieceColor.Black, 5);
            Assert.AreEqual("5... Nc6", formatted);
        }

        [Test]
        public void BlackToMoveFromCustomFen_ThreePliesAdvancesFullmoveCounter()
        {
            var sanByPly = new List<string> { "Nc6", "Bb5", "a6" };
            var formatted = MoveHistoryFormatter.Format(sanByPly, PieceColor.Black, 5);
            Assert.AreEqual("5... Nc6\n6. Bb5 a6", formatted);
        }

        [Test]
        public void WhiteToMoveFromMidGame_StartingFullmoveNumberHonoured()
        {
            var sanByPly = new List<string> { "Nf3", "Nc6" };
            var formatted = MoveHistoryFormatter.Format(sanByPly, PieceColor.White, 12);
            Assert.AreEqual("12. Nf3 Nc6", formatted);
        }
    }
}
