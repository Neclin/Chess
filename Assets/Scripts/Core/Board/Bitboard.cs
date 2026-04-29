namespace Chess.Core.Board
{
    public static class Bitboard
    {
        public static ulong SetBit(ulong bitboard, int squareIndex) => bitboard | (1UL << squareIndex);

        public static ulong ClearBit(ulong bitboard, int squareIndex) => bitboard & ~(1UL << squareIndex);

        public static bool IsBitSet(ulong bitboard, int squareIndex) => ((bitboard >> squareIndex) & 1UL) != 0;

        public static int CountSetBits(ulong bitboard)
        {
            int bitCount = 0;
            while (bitboard != 0)
            {
                bitboard &= bitboard - 1;
                bitCount++;
            }
            return bitCount;
        }

        public static int LowestSetBitIndex(ulong bitboard)
        {
            int bitIndex = 0;
            while ((bitboard & 1UL) == 0)
            {
                bitboard >>= 1;
                bitIndex++;
            }
            return bitIndex;
        }

        public static int PopLowestSetBitIndex(ref ulong bitboard)
        {
            int squareIndex = LowestSetBitIndex(bitboard);
            bitboard &= bitboard - 1;
            return squareIndex;
        }

        public const ulong FileAMask = 0x0101010101010101UL;
        public const ulong FileHMask = 0x8080808080808080UL;
        public const ulong Rank1Mask = 0x00000000000000FFUL;
        public const ulong Rank8Mask = 0xFF00000000000000UL;
    }
}
