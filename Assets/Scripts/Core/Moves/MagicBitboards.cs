using System;
using Chess.Core.Board;

namespace Chess.Core.Moves
{
    public static class MagicBitboards
    {
        public static readonly ulong[] RookRelevantBlockerMasks = new ulong[64];
        public static readonly ulong[] BishopRelevantBlockerMasks = new ulong[64];
        public static readonly ulong[] RookMagicMultipliers = new ulong[64];
        public static readonly ulong[] BishopMagicMultipliers = new ulong[64];
        public static readonly int[] RookIndexShifts = new int[64];
        public static readonly int[] BishopIndexShifts = new int[64];
        public static readonly ulong[][] RookAttackTable = new ulong[64][];
        public static readonly ulong[][] BishopAttackTable = new ulong[64][];

        private const int MagicSearchSeed = unchecked((int)0xC0FFEEu);

        static MagicBitboards()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                RookRelevantBlockerMasks[squareIndex] = ComputeRookRelevantBlockerMask(squareIndex);
                BishopRelevantBlockerMasks[squareIndex] = ComputeBishopRelevantBlockerMask(squareIndex);
                RookIndexShifts[squareIndex] = 64 - Bitboard.CountSetBits(RookRelevantBlockerMasks[squareIndex]);
                BishopIndexShifts[squareIndex] = 64 - Bitboard.CountSetBits(BishopRelevantBlockerMasks[squareIndex]);
            }

            var magicSearchRandom = new Random(MagicSearchSeed);
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                BuildAttackTableForSquare(
                    squareIndex,
                    RookRelevantBlockerMasks[squareIndex],
                    RookIndexShifts[squareIndex],
                    SlidingAttacks.RookSlow,
                    magicSearchRandom,
                    out RookMagicMultipliers[squareIndex],
                    out RookAttackTable[squareIndex]);

                BuildAttackTableForSquare(
                    squareIndex,
                    BishopRelevantBlockerMasks[squareIndex],
                    BishopIndexShifts[squareIndex],
                    SlidingAttacks.BishopSlow,
                    magicSearchRandom,
                    out BishopMagicMultipliers[squareIndex],
                    out BishopAttackTable[squareIndex]);
            }
        }

        public static ulong RookAttacks(int squareIndex, ulong occupancyBitboard)
        {
            ulong relevantBlockers = occupancyBitboard & RookRelevantBlockerMasks[squareIndex];
            ulong attackTableIndex = (relevantBlockers * RookMagicMultipliers[squareIndex]) >> RookIndexShifts[squareIndex];
            return RookAttackTable[squareIndex][attackTableIndex];
        }

        public static ulong BishopAttacks(int squareIndex, ulong occupancyBitboard)
        {
            ulong relevantBlockers = occupancyBitboard & BishopRelevantBlockerMasks[squareIndex];
            ulong attackTableIndex = (relevantBlockers * BishopMagicMultipliers[squareIndex]) >> BishopIndexShifts[squareIndex];
            return BishopAttackTable[squareIndex][attackTableIndex];
        }

        private static ulong ComputeRookRelevantBlockerMask(int squareIndex)
        {
            ulong mask = 0UL;
            int fromFile = Square.FileIndex(squareIndex);
            int fromRank = Square.RankIndex(squareIndex);
            for (int file = fromFile + 1; file <= 6; file++) mask |= 1UL << Square.FromFileAndRank(file, fromRank);
            for (int file = fromFile - 1; file >= 1; file--) mask |= 1UL << Square.FromFileAndRank(file, fromRank);
            for (int rank = fromRank + 1; rank <= 6; rank++) mask |= 1UL << Square.FromFileAndRank(fromFile, rank);
            for (int rank = fromRank - 1; rank >= 1; rank--) mask |= 1UL << Square.FromFileAndRank(fromFile, rank);
            return mask;
        }

        private static ulong ComputeBishopRelevantBlockerMask(int squareIndex)
        {
            ulong mask = 0UL;
            int fromFile = Square.FileIndex(squareIndex);
            int fromRank = Square.RankIndex(squareIndex);
            for (int file = fromFile + 1, rank = fromRank + 1; file <= 6 && rank <= 6; file++, rank++) mask |= 1UL << Square.FromFileAndRank(file, rank);
            for (int file = fromFile + 1, rank = fromRank - 1; file <= 6 && rank >= 1; file++, rank--) mask |= 1UL << Square.FromFileAndRank(file, rank);
            for (int file = fromFile - 1, rank = fromRank + 1; file >= 1 && rank <= 6; file--, rank++) mask |= 1UL << Square.FromFileAndRank(file, rank);
            for (int file = fromFile - 1, rank = fromRank - 1; file >= 1 && rank >= 1; file--, rank--) mask |= 1UL << Square.FromFileAndRank(file, rank);
            return mask;
        }

        private static void BuildAttackTableForSquare(
            int squareIndex,
            ulong relevantBlockerMask,
            int indexShift,
            Func<int, ulong, ulong> referenceAttackFunction,
            Random magicSearchRandom,
            out ulong magicMultiplier,
            out ulong[] attackTable)
        {
            int relevantBitCount = Bitboard.CountSetBits(relevantBlockerMask);
            int subsetCount = 1 << relevantBitCount;
            ulong[] enumeratedSubsets = new ulong[subsetCount];
            ulong[] referenceAttacksBySubset = new ulong[subsetCount];
            ulong currentSubset = 0UL;
            for (int subsetIndex = 0; subsetIndex < subsetCount; subsetIndex++)
            {
                enumeratedSubsets[subsetIndex] = currentSubset;
                referenceAttacksBySubset[subsetIndex] = referenceAttackFunction(squareIndex, currentSubset);
                currentSubset = (currentSubset - relevantBlockerMask) & relevantBlockerMask;
            }

            ulong[] candidateTable = new ulong[subsetCount];
            bool[] entryWasWritten = new bool[subsetCount];
            while (true)
            {
                ulong magicCandidate = NextSparseRandomUlong(magicSearchRandom);
                Array.Clear(entryWasWritten, 0, subsetCount);
                bool collisionFound = false;
                for (int subsetIndex = 0; subsetIndex < subsetCount; subsetIndex++)
                {
                    ulong tableIndex = (enumeratedSubsets[subsetIndex] * magicCandidate) >> indexShift;
                    if (!entryWasWritten[tableIndex])
                    {
                        candidateTable[tableIndex] = referenceAttacksBySubset[subsetIndex];
                        entryWasWritten[tableIndex] = true;
                    }
                    else if (candidateTable[tableIndex] != referenceAttacksBySubset[subsetIndex])
                    {
                        collisionFound = true;
                        break;
                    }
                }
                if (!collisionFound)
                {
                    magicMultiplier = magicCandidate;
                    attackTable = candidateTable;
                    return;
                }
            }
        }

        private static ulong NextSparseRandomUlong(Random magicSearchRandom)
        {
            return NextRandomUlong(magicSearchRandom)
                 & NextRandomUlong(magicSearchRandom)
                 & NextRandomUlong(magicSearchRandom);
        }

        private static ulong NextRandomUlong(Random magicSearchRandom)
        {
            byte[] randomBytes = new byte[8];
            magicSearchRandom.NextBytes(randomBytes);
            return BitConverter.ToUInt64(randomBytes, 0);
        }
    }
}
