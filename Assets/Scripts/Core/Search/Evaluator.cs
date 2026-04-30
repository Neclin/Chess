using Chess.Core.Board;

namespace Chess.Core.Search
{
    public static class Evaluator
    {
        public static readonly int[] MaterialValueByPieceType =
        {
            0,
            100,
            320,
            330,
            500,
            900,
            20000
        };

        public static readonly int[] PawnPieceSquareTable =
        {
              0,  0,  0,  0,  0,  0,  0,  0,
              5, 10, 10,-20,-20, 10, 10,  5,
              5, -5,-10,  0,  0,-10, -5,  5,
              0,  0,  0, 20, 20,  0,  0,  0,
              5,  5, 10, 25, 25, 10,  5,  5,
             10, 10, 20, 30, 30, 20, 10, 10,
             50, 50, 50, 50, 50, 50, 50, 50,
              0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] KnightPieceSquareTable =
        {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        };

        public static readonly int[] BishopPieceSquareTable =
        {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };

        public static readonly int[] RookPieceSquareTable =
        {
              0,  0,  5, 10, 10,  5,  0,  0,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
              5, 10, 10, 10, 10, 10, 10,  5,
              0,  0,  0,  0,  0,  0,  0,  0
        };

        public static readonly int[] QueenPieceSquareTable =
        {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -10,  5,  5,  5,  5,  5,  0,-10,
              0,  0,  5,  5,  5,  5,  0, -5,
             -5,  0,  5,  5,  5,  5,  0, -5,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };

        public static readonly int[] KingPieceSquareTable =
        {
             20, 30, 10,  0,  0, 10, 30, 20,
             20, 20,  0,  0,  0,  0, 20, 20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30
        };

        public static int Evaluate(BoardState board)
        {
            int whiteScore = ScoreSide(board, PieceColor.White);
            int blackScore = ScoreSide(board, PieceColor.Black);
            int scoreFromWhitePerspective = whiteScore - blackScore;
            return board.SideToMove == PieceColor.White ? scoreFromWhitePerspective : -scoreFromWhitePerspective;
        }

        private static int ScoreSide(BoardState board, PieceColor pieceColor)
        {
            int sideScore = 0;
            sideScore += ScorePieces(board.BitboardFor(PieceType.Pawn,   pieceColor), pieceColor, PieceType.Pawn,   PawnPieceSquareTable);
            sideScore += ScorePieces(board.BitboardFor(PieceType.Knight, pieceColor), pieceColor, PieceType.Knight, KnightPieceSquareTable);
            sideScore += ScorePieces(board.BitboardFor(PieceType.Bishop, pieceColor), pieceColor, PieceType.Bishop, BishopPieceSquareTable);
            sideScore += ScorePieces(board.BitboardFor(PieceType.Rook,   pieceColor), pieceColor, PieceType.Rook,   RookPieceSquareTable);
            sideScore += ScorePieces(board.BitboardFor(PieceType.Queen,  pieceColor), pieceColor, PieceType.Queen,  QueenPieceSquareTable);
            sideScore += ScorePieces(board.BitboardFor(PieceType.King,   pieceColor), pieceColor, PieceType.King,   KingPieceSquareTable);
            return sideScore;
        }

        private static int ScorePieces(ulong pieceBitboard, PieceColor pieceColor, PieceType pieceType, int[] pieceSquareTable)
        {
            int materialValue = MaterialValueByPieceType[(int)pieceType];
            int pieceScore = 0;
            while (pieceBitboard != 0)
            {
                int squareIndex = Bitboard.PopLowestSetBitIndex(ref pieceBitboard);
                int tableIndex = pieceColor == PieceColor.White ? squareIndex : squareIndex ^ 56;
                pieceScore += materialValue + pieceSquareTable[tableIndex];
            }
            return pieceScore;
        }
    }
}
