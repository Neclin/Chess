using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Tests
{
    public class PrecomputedAttacksTests
    {
        [Test]
        public void KnightOnD4_HasEightTargets()
        {
            ulong attackBitboard = PrecomputedAttacks.KnightAttacks[Square.FromAlgebraic("d4")];
            Assert.AreEqual(8, Bitboard.CountSetBits(attackBitboard));
        }

        [Test]
        public void KnightOnA1_HasTwoTargets()
            => Assert.AreEqual(2, Bitboard.CountSetBits(PrecomputedAttacks.KnightAttacks[0]));

        [Test]
        public void KingOnE1_HasFiveTargets()
            => Assert.AreEqual(5, Bitboard.CountSetBits(PrecomputedAttacks.KingAttacks[Square.FromAlgebraic("e1")]));

        [Test]
        public void KingInCorner_HasThreeTargets()
            => Assert.AreEqual(3, Bitboard.CountSetBits(PrecomputedAttacks.KingAttacks[0]));

        [Test]
        public void WhitePawnOnE2_AttacksD3AndF3()
        {
            ulong attackBitboard = PrecomputedAttacks.PawnAttacks[(int)PieceColor.White, Square.FromAlgebraic("e2")];
            ulong expected = (1UL << Square.FromAlgebraic("d3")) | (1UL << Square.FromAlgebraic("f3"));
            Assert.AreEqual(expected, attackBitboard);
        }

        [Test]
        public void BlackPawnOnE7_AttacksD6AndF6()
        {
            ulong attackBitboard = PrecomputedAttacks.PawnAttacks[(int)PieceColor.Black, Square.FromAlgebraic("e7")];
            ulong expected = (1UL << Square.FromAlgebraic("d6")) | (1UL << Square.FromAlgebraic("f6"));
            Assert.AreEqual(expected, attackBitboard);
        }
    }
}
