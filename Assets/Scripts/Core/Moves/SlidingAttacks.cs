using Chess.Core.Board;

namespace Chess.Core.Moves
{
    public static class SlidingAttacks
    {
        public static bool UseMagicBitboards = true;

        private static readonly int[,] RookRayDeltas =
        {
            { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }
        };

        private static readonly int[,] BishopRayDeltas =
        {
            { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 }
        };

        public static ulong Rook(int squareIndex, ulong occupancyBitboard)
            => UseMagicBitboards
                ? MagicBitboards.RookAttacks(squareIndex, occupancyBitboard)
                : RookSlow(squareIndex, occupancyBitboard);

        public static ulong Bishop(int squareIndex, ulong occupancyBitboard)
            => UseMagicBitboards
                ? MagicBitboards.BishopAttacks(squareIndex, occupancyBitboard)
                : BishopSlow(squareIndex, occupancyBitboard);

        public static ulong Queen(int squareIndex, ulong occupancyBitboard) => Rook(squareIndex, occupancyBitboard) | Bishop(squareIndex, occupancyBitboard);

        public static ulong RookSlow(int squareIndex, ulong occupancyBitboard) => RaySlideAttacks(squareIndex, occupancyBitboard, RookRayDeltas);
        public static ulong BishopSlow(int squareIndex, ulong occupancyBitboard) => RaySlideAttacks(squareIndex, occupancyBitboard, BishopRayDeltas);
        public static ulong QueenSlow(int squareIndex, ulong occupancyBitboard) => RookSlow(squareIndex, occupancyBitboard) | BishopSlow(squareIndex, occupancyBitboard);

        private static ulong RaySlideAttacks(int squareIndex, ulong occupancyBitboard, int[,] directions)
        {
            ulong attackBitboard = 0UL;
            int fromFile = Square.FileIndex(squareIndex);
            int fromRank = Square.RankIndex(squareIndex);
            for (int directionIndex = 0; directionIndex < 4; directionIndex++)
            {
                int fileDelta = directions[directionIndex, 0];
                int rankDelta = directions[directionIndex, 1];
                int targetFile = fromFile + fileDelta;
                int targetRank = fromRank + rankDelta;
                while (targetFile >= 0 && targetFile < 8 && targetRank >= 0 && targetRank < 8)
                {
                    int targetSquare = Square.FromFileAndRank(targetFile, targetRank);
                    attackBitboard |= 1UL << targetSquare;
                    if ((occupancyBitboard & (1UL << targetSquare)) != 0) break;
                    targetFile += fileDelta;
                    targetRank += rankDelta;
                }
            }
            return attackBitboard;
        }
    }
}
