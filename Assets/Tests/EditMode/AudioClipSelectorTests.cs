using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Notation;
using Chess.Core.Rules;
using Chess.Unity;

namespace Chess.Core.Tests
{
    public class AudioClipSelectorTests
    {
        [Test]
        public void SelectMoveKind_CheckmateMoveReturnsCheckmate()
        {
            BoardState board = FenParser.Parse("k7/7R/8/8/8/8/8/4K2R w K - 0 1");
            Move matingMove = new Move(Square.FromAlgebraic("h1"), Square.FromAlgebraic("h8"));
            MoveExecutor.MakeMove(board, matingMove);
            Assert.AreEqual(AudioClipKind.Checkmate, AudioClipSelector.SelectMoveKind(matingMove, board, GameResult.WhiteWinsByCheckmate));
        }

        [Test]
        public void SelectMoveKind_MoveDeliveringCheckReturnsCheck()
        {
            BoardState board = FenParser.Parse("4k3/8/8/8/8/8/8/4R2K w - - 0 1");
            Move checkMove = new Move(Square.FromAlgebraic("e1"), Square.FromAlgebraic("e7"));
            MoveExecutor.MakeMove(board, checkMove);
            Assert.AreEqual(AudioClipKind.Check, AudioClipSelector.SelectMoveKind(checkMove, board, GameResult.Ongoing));
        }

        [Test]
        public void SelectMoveKind_CaptureMoveReturnsCapture()
        {
            BoardState board = FenParser.Parse("k7/8/8/8/4r3/8/8/3R3K w - - 0 1");
            Move captureMove = new Move(Square.FromAlgebraic("d1"), Square.FromAlgebraic("d4"), MoveFlags.Capture);
            MoveExecutor.MakeMove(board, captureMove);
            Assert.AreEqual(AudioClipKind.Capture, AudioClipSelector.SelectMoveKind(captureMove, board, GameResult.Ongoing));
        }

        [Test]
        public void SelectMoveKind_QuietMoveReturnsMove()
        {
            BoardState board = FenParser.Parse("4k3/8/8/8/8/8/8/4K2R w K - 0 1");
            Move quietMove = new Move(Square.FromAlgebraic("h1"), Square.FromAlgebraic("h2"));
            MoveExecutor.MakeMove(board, quietMove);
            Assert.AreEqual(AudioClipKind.Move, AudioClipSelector.SelectMoveKind(quietMove, board, GameResult.Ongoing));
        }

        [Test]
        public void SelectMoveKind_CheckTakesPriorityOverCapture()
        {
            BoardState board = FenParser.Parse("4k3/8/8/8/4r3/8/8/4R2K w - - 0 1");
            Move captureWithCheckMove = new Move(Square.FromAlgebraic("e1"), Square.FromAlgebraic("e4"), MoveFlags.Capture);
            MoveExecutor.MakeMove(board, captureWithCheckMove);
            Assert.AreEqual(AudioClipKind.Check, AudioClipSelector.SelectMoveKind(captureWithCheckMove, board, GameResult.Ongoing));
        }

        [Test]
        public void SelectOutcomeKind_HumanWinsByCheckmateReturnsVictory()
        {
            Assert.AreEqual(AudioClipKind.Victory, AudioClipSelector.SelectOutcomeKind(GameResult.WhiteWinsByCheckmate, PieceColor.White));
            Assert.AreEqual(AudioClipKind.Victory, AudioClipSelector.SelectOutcomeKind(GameResult.BlackWinsByCheckmate, PieceColor.Black));
        }

        [Test]
        public void SelectOutcomeKind_HumanLosesByCheckmateReturnsDefeat()
        {
            Assert.AreEqual(AudioClipKind.Defeat, AudioClipSelector.SelectOutcomeKind(GameResult.BlackWinsByCheckmate, PieceColor.White));
            Assert.AreEqual(AudioClipKind.Defeat, AudioClipSelector.SelectOutcomeKind(GameResult.WhiteWinsByCheckmate, PieceColor.Black));
        }

        [Test]
        public void SelectOutcomeKind_AnyDrawReturnsDraw()
        {
            Assert.AreEqual(AudioClipKind.Draw, AudioClipSelector.SelectOutcomeKind(GameResult.DrawByStalemate, PieceColor.White));
            Assert.AreEqual(AudioClipKind.Draw, AudioClipSelector.SelectOutcomeKind(GameResult.DrawByFiftyMoveRule, PieceColor.Black));
            Assert.AreEqual(AudioClipKind.Draw, AudioClipSelector.SelectOutcomeKind(GameResult.DrawByThreefoldRepetition, PieceColor.White));
            Assert.AreEqual(AudioClipKind.Draw, AudioClipSelector.SelectOutcomeKind(GameResult.DrawByInsufficientMaterial, PieceColor.Black));
        }
    }
}
