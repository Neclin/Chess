using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Tests
{
    public class SlidingAttacksTests
    {
        [Test]
        public void RookOnA1_EmptyBoard_HasFourteenTargets()
            => Assert.AreEqual(14, Bitboard.CountSetBits(SlidingAttacks.Rook(0, 0UL)));

        [Test]
        public void BishopOnD4_EmptyBoard_HasThirteenTargets()
            => Assert.AreEqual(13, Bitboard.CountSetBits(SlidingAttacks.Bishop(Square.FromAlgebraic("d4"), 0UL)));

        [Test]
        public void RookOnA1_BlockerOnA4_StopsAtA4()
        {
            ulong occupancyBitboard = 1UL << Square.FromAlgebraic("a4");
            ulong attackBitboard = SlidingAttacks.Rook(0, occupancyBitboard);
            Assert.IsTrue(Bitboard.IsBitSet(attackBitboard, Square.FromAlgebraic("a4")));
            Assert.IsFalse(Bitboard.IsBitSet(attackBitboard, Square.FromAlgebraic("a5")));
        }

        [Test]
        public void Queen_IsRookOrBishop()
        {
            int squareIndex = Square.FromAlgebraic("d4");
            ulong queenAttacks = SlidingAttacks.Queen(squareIndex, 0UL);
            Assert.AreEqual(SlidingAttacks.Rook(squareIndex, 0UL) | SlidingAttacks.Bishop(squareIndex, 0UL), queenAttacks);
        }
    }
}
