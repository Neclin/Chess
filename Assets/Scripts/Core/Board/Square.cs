namespace Chess.Core.Board
{
    public static class Square
    {
        public static int FromAlgebraic(string algebraic)
        {
            int fileIndex = algebraic[0] - 'a';
            int rankIndex = algebraic[1] - '1';
            return rankIndex * 8 + fileIndex;
        }

        public static string ToAlgebraic(int squareIndex) => $"{(char)('a' + (squareIndex & 7))}{(char)('1' + (squareIndex >> 3))}";

        public static int FileIndex(int squareIndex) => squareIndex & 7;
        public static int RankIndex(int squareIndex) => squareIndex >> 3;
        public static int FromFileAndRank(int fileIndex, int rankIndex) => rankIndex * 8 + fileIndex;
    }
}
