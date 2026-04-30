using Chess.Core.Board;

namespace Chess.Core.Moves
{
    public static class PrecomputedAttacks
    {
        public static readonly ulong[] KnightAttacks = new ulong[64];
        public static readonly ulong[] KingAttacks = new ulong[64];
        public static readonly ulong[,] PawnAttacks = new ulong[2, 64];

        static PrecomputedAttacks()
        {
            int[,] knightFileRankOffsets =
            {
                { 1, 2 }, { 2, 1 }, { 2, -1 }, { 1, -2 },
                { -1, -2 }, { -2, -1 }, { -2, 1 }, { -1, 2 }
            };

            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                int fileIndex = Square.FileIndex(squareIndex);
                int rankIndex = Square.RankIndex(squareIndex);

                ulong knightAttackBitboard = 0UL;
                for (int offsetIndex = 0; offsetIndex < 8; offsetIndex++)
                {
                    int targetFile = fileIndex + knightFileRankOffsets[offsetIndex, 0];
                    int targetRank = rankIndex + knightFileRankOffsets[offsetIndex, 1];
                    if (targetFile >= 0 && targetFile < 8 && targetRank >= 0 && targetRank < 8)
                        knightAttackBitboard |= 1UL << Square.FromFileAndRank(targetFile, targetRank);
                }
                KnightAttacks[squareIndex] = knightAttackBitboard;

                ulong kingAttackBitboard = 0UL;
                for (int fileDelta = -1; fileDelta <= 1; fileDelta++)
                    for (int rankDelta = -1; rankDelta <= 1; rankDelta++)
                    {
                        if (fileDelta == 0 && rankDelta == 0) continue;
                        int targetFile = fileIndex + fileDelta;
                        int targetRank = rankIndex + rankDelta;
                        if (targetFile >= 0 && targetFile < 8 && targetRank >= 0 && targetRank < 8)
                            kingAttackBitboard |= 1UL << Square.FromFileAndRank(targetFile, targetRank);
                    }
                KingAttacks[squareIndex] = kingAttackBitboard;

                ulong whitePawnAttackBitboard = 0UL;
                if (fileIndex - 1 >= 0 && rankIndex + 1 < 8)
                    whitePawnAttackBitboard |= 1UL << Square.FromFileAndRank(fileIndex - 1, rankIndex + 1);
                if (fileIndex + 1 < 8 && rankIndex + 1 < 8)
                    whitePawnAttackBitboard |= 1UL << Square.FromFileAndRank(fileIndex + 1, rankIndex + 1);
                PawnAttacks[(int)PieceColor.White, squareIndex] = whitePawnAttackBitboard;

                ulong blackPawnAttackBitboard = 0UL;
                if (fileIndex - 1 >= 0 && rankIndex - 1 >= 0)
                    blackPawnAttackBitboard |= 1UL << Square.FromFileAndRank(fileIndex - 1, rankIndex - 1);
                if (fileIndex + 1 < 8 && rankIndex - 1 >= 0)
                    blackPawnAttackBitboard |= 1UL << Square.FromFileAndRank(fileIndex + 1, rankIndex - 1);
                PawnAttacks[(int)PieceColor.Black, squareIndex] = blackPawnAttackBitboard;
            }
        }
    }
}
