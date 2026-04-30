using Chess.Core.Board;

namespace Chess.Core.Moves
{
    public static class SlidingAttacks
    {
        private static readonly int[,] RookDirections =
        {
            { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }
        };

        private static readonly int[,] BishopDirections =
        {
            { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 }
        };

        public static ulong Rook(int squareIndex, ulong occupancyBitboard) => RayAttacks(squareIndex, occupancyBitboard, RookDirections);
        public static ulong Bishop(int squareIndex, ulong occupancyBitboard) => RayAttacks(squareIndex, occupancyBitboard, BishopDirections);
        public static ulong Queen(int squareIndex, ulong occupancyBitboard) => Rook(squareIndex, occupancyBitboard) | Bishop(squareIndex, occupancyBitboard);

        private static ulong RayAttacks(int squareIndex, ulong occupancyBitboard, int[,] directions)
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
