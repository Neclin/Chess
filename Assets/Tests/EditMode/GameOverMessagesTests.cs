using NUnit.Framework;
using Chess.Core.Rules;

namespace Chess.Core.Tests
{
    public class GameOverMessagesTests
    {
        [Test]
        public void White_wins_by_checkmate_message()
        {
            Assert.AreEqual("Checkmate — White wins", GameOverMessages.Describe(GameResult.WhiteWinsByCheckmate));
        }

        [Test]
        public void Black_wins_by_checkmate_message()
        {
            Assert.AreEqual("Checkmate — Black wins", GameOverMessages.Describe(GameResult.BlackWinsByCheckmate));
        }

        [Test]
        public void Stalemate_message()
        {
            Assert.AreEqual("Stalemate — Draw", GameOverMessages.Describe(GameResult.DrawByStalemate));
        }

        [Test]
        public void Insufficient_material_message()
        {
            Assert.AreEqual("Draw by insufficient material", GameOverMessages.Describe(GameResult.DrawByInsufficientMaterial));
        }

        [Test]
        public void Fifty_move_rule_message()
        {
            Assert.AreEqual("Draw by fifty-move rule", GameOverMessages.Describe(GameResult.DrawByFiftyMoveRule));
        }

        [Test]
        public void Threefold_repetition_message()
        {
            Assert.AreEqual("Draw by threefold repetition", GameOverMessages.Describe(GameResult.DrawByThreefoldRepetition));
        }

        [Test]
        public void Ongoing_message_is_empty()
        {
            Assert.AreEqual(string.Empty, GameOverMessages.Describe(GameResult.Ongoing));
        }
    }
}
