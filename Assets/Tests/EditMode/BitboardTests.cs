using NUnit.Framework;
using Chess.Core.Board;

namespace Chess.Core.Tests
{
    public class BitboardTests
    {
        [Test] public void EmptyHasZeroPopcount() => Assert.AreEqual(0, Bitboard.CountSetBits(0UL));
        [Test] public void FullBoardPopcountIs64() => Assert.AreEqual(64, Bitboard.CountSetBits(~0UL));
        [Test] public void SetBitWorks() => Assert.AreEqual(1UL << 12, Bitboard.SetBit(0UL, 12));
        [Test] public void ClearBitWorks() => Assert.AreEqual(0UL, Bitboard.ClearBit(1UL << 12, 12));
        [Test] public void GetBitTrue() => Assert.IsTrue(Bitboard.IsBitSet(1UL << 12, 12));
        [Test] public void GetBitFalse() => Assert.IsFalse(Bitboard.IsBitSet(1UL << 12, 13));
        [Test] public void LowestSetBitIndex_OfSingleBit_ReturnsThatBitIndex() => Assert.AreEqual(7, Bitboard.LowestSetBitIndex(1UL << 7));

        [Test]
        public void PopLowestSetBitIndex_ReturnsLowestBitAndClearsIt()
        {
            ulong bitboard = (1UL << 3) | (1UL << 17);
            int squareIndex = Bitboard.PopLowestSetBitIndex(ref bitboard);
            Assert.AreEqual(3, squareIndex);
            Assert.AreEqual(1UL << 17, bitboard);
        }
    }
}
