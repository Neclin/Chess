using Chess.Core.Board;

namespace Chess.Core.Search
{
    public static class Zobrist
    {
        public static readonly ulong[,] PieceSquareKeys = new ulong[12, 64];
        public static readonly ulong[] CastlingRightsKeys = new ulong[16];
        public static readonly ulong[] EnPassantFileKeys = new ulong[8];
        public static readonly ulong SideToMoveKey;

        static Zobrist()
        {
            var random = new System.Random(0x5C0FE5);
            byte[] randomBuffer = new byte[8];
            ulong NextRandomKey()
            {
                random.NextBytes(randomBuffer);
                return System.BitConverter.ToUInt64(randomBuffer, 0);
            }

            for (int pieceIndex = 0; pieceIndex < 12; pieceIndex++)
                for (int squareIndex = 0; squareIndex < 64; squareIndex++)
                    PieceSquareKeys[pieceIndex, squareIndex] = NextRandomKey();

            for (int castlingMask = 0; castlingMask < 16; castlingMask++)
                CastlingRightsKeys[castlingMask] = NextRandomKey();

            for (int fileIndex = 0; fileIndex < 8; fileIndex++)
                EnPassantFileKeys[fileIndex] = NextRandomKey();

            SideToMoveKey = NextRandomKey();
        }

        public static ulong PieceSquareKey(PieceType pieceType, PieceColor pieceColor, int squareIndex)
        {
            int pieceIndex = (int)pieceColor * 6 + ((int)pieceType - 1);
            return PieceSquareKeys[pieceIndex, squareIndex];
        }

        public static ulong ComputeFromScratch(BoardState board)
        {
            ulong key = 0;
            for (int colorIndex = 0; colorIndex < 2; colorIndex++)
            {
                var pieceColor = (PieceColor)colorIndex;
                for (int pieceTypeIndex = 1; pieceTypeIndex <= 6; pieceTypeIndex++)
                {
                    var pieceType = (PieceType)pieceTypeIndex;
                    ulong pieceBitboard = board.BitboardFor(pieceType, pieceColor);
                    while (pieceBitboard != 0)
                    {
                        int squareIndex = Bitboard.PopLowestSetBitIndex(ref pieceBitboard);
                        key ^= PieceSquareKey(pieceType, pieceColor, squareIndex);
                    }
                }
            }

            key ^= CastlingRightsKeys[(int)board.Castling];

            if (board.EnPassantSquare >= 0)
                key ^= EnPassantFileKeys[Square.FileIndex(board.EnPassantSquare)];

            if (board.SideToMove == PieceColor.Black)
                key ^= SideToMoveKey;

            return key;
        }
    }
}
