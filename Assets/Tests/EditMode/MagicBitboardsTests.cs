using NUnit.Framework;
using System;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Tests
{
    public class MagicBitboardsTests
    {
        [Test]
        public void RookRelevantBlockerMask_OnA1_ExcludesEdgesOnItsRays()
        {
            ulong mask = MagicBitboards.RookRelevantBlockerMasks[Square.FromAlgebraic("a1")];
            Assert.AreEqual(12, Bitboard.CountSetBits(mask));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("b1")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("g1")));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("h1")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("a2")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("a7")));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("a8")));
        }

        [Test]
        public void RookRelevantBlockerMask_OnD4_ExcludesEdgesOnItsRays()
        {
            ulong mask = MagicBitboards.RookRelevantBlockerMasks[Square.FromAlgebraic("d4")];
            Assert.AreEqual(10, Bitboard.CountSetBits(mask));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("a4")));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("h4")));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("d1")));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("d8")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("b4")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("g4")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("d2")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("d7")));
        }

        [Test]
        public void BishopRelevantBlockerMask_OnD4_ExcludesAllEdgeSquares()
        {
            ulong mask = MagicBitboards.BishopRelevantBlockerMasks[Square.FromAlgebraic("d4")];
            Assert.AreEqual(9, Bitboard.CountSetBits(mask));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("a1")));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("h8")));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("a7")));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("g1")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("e5")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("b2")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("e3")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("b6")));
        }

        [Test]
        public void BishopRelevantBlockerMask_OnA1_HasSixRelevantBits()
        {
            ulong mask = MagicBitboards.BishopRelevantBlockerMasks[Square.FromAlgebraic("a1")];
            Assert.AreEqual(6, Bitboard.CountSetBits(mask));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("b2")));
            Assert.IsTrue(Bitboard.IsBitSet(mask, Square.FromAlgebraic("g7")));
            Assert.IsFalse(Bitboard.IsBitSet(mask, Square.FromAlgebraic("h8")));
        }

        [Test]
        public void RookMagicLookup_MatchesSlowRays_ForAllSquaresOnEmptyBoard()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                Assert.AreEqual(SlidingAttacks.RookSlow(squareIndex, 0UL), SlidingAttacks.Rook(squareIndex, 0UL),
                    $"Mismatch on empty board at square {squareIndex}");
            }
        }

        [Test]
        public void BishopMagicLookup_MatchesSlowRays_ForAllSquaresOnEmptyBoard()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                Assert.AreEqual(SlidingAttacks.BishopSlow(squareIndex, 0UL), SlidingAttacks.Bishop(squareIndex, 0UL),
                    $"Mismatch on empty board at square {squareIndex}");
            }
        }

        [Test]
        public void RookMagicLookup_MatchesSlowRays_ForOneThousandRandomOccupanciesPerSquare()
        {
            var random = new Random(1);
            byte[] randomBuffer = new byte[8];
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                for (int trialIndex = 0; trialIndex < 1000; trialIndex++)
                {
                    random.NextBytes(randomBuffer);
                    ulong occupancyBitboard = BitConverter.ToUInt64(randomBuffer, 0);
                    Assert.AreEqual(SlidingAttacks.RookSlow(squareIndex, occupancyBitboard),
                        SlidingAttacks.Rook(squareIndex, occupancyBitboard),
                        $"Mismatch at square {squareIndex}, trial {trialIndex}, occ=0x{occupancyBitboard:X16}");
                }
            }
        }

        [Test]
        public void BishopMagicLookup_MatchesSlowRays_ForOneThousandRandomOccupanciesPerSquare()
        {
            var random = new Random(1);
            byte[] randomBuffer = new byte[8];
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                for (int trialIndex = 0; trialIndex < 1000; trialIndex++)
                {
                    random.NextBytes(randomBuffer);
                    ulong occupancyBitboard = BitConverter.ToUInt64(randomBuffer, 0);
                    Assert.AreEqual(SlidingAttacks.BishopSlow(squareIndex, occupancyBitboard),
                        SlidingAttacks.Bishop(squareIndex, occupancyBitboard),
                        $"Mismatch at square {squareIndex}, trial {trialIndex}, occ=0x{occupancyBitboard:X16}");
                }
            }
        }

        [Test]
        public void RookSlowAndBishopSlow_StillExist_AndMatchHistoricalBehavior()
        {
            Assert.AreEqual(14, Bitboard.CountSetBits(SlidingAttacks.RookSlow(0, 0UL)));
            Assert.AreEqual(13, Bitboard.CountSetBits(SlidingAttacks.BishopSlow(Square.FromAlgebraic("d4"), 0UL)));
        }
    }
}
