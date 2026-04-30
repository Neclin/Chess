using System.Collections.Generic;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Rules
{
    public enum GameResult
    {
        Ongoing,
        WhiteWinsByCheckmate,
        BlackWinsByCheckmate,
        DrawByStalemate,
        DrawByInsufficientMaterial,
        DrawByFiftyMoveRule,
        DrawByThreefoldRepetition
    }

    public static class GameStateChecker
    {
        public static GameResult Evaluate(BoardState board)
        {
            if (board.HalfmoveClock >= 100) return GameResult.DrawByFiftyMoveRule;
            if (IsInsufficientMaterial(board)) return GameResult.DrawByInsufficientMaterial;

            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            if (legalMoves.Count > 0) return GameResult.Ongoing;

            int kingSquareIndex = Bitboard.LowestSetBitIndex(board.BitboardFor(PieceType.King, board.SideToMove));
            PieceColor opponentColor = board.SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
            bool sideToMoveInCheck = MoveGenerator.IsSquareAttacked(board, kingSquareIndex, opponentColor);
            if (!sideToMoveInCheck) return GameResult.DrawByStalemate;
            return board.SideToMove == PieceColor.White
                ? GameResult.BlackWinsByCheckmate
                : GameResult.WhiteWinsByCheckmate;
        }

        public static bool InCheck(BoardState board)
        {
            int kingSquareIndex = Bitboard.LowestSetBitIndex(board.BitboardFor(PieceType.King, board.SideToMove));
            PieceColor opponentColor = board.SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
            return MoveGenerator.IsSquareAttacked(board, kingSquareIndex, opponentColor);
        }

        private static bool IsInsufficientMaterial(BoardState board)
        {
            for (int colorIndex = 0; colorIndex < 2; colorIndex++)
            {
                PieceColor pieceColor = (PieceColor)colorIndex;
                if (board.BitboardFor(PieceType.Pawn, pieceColor) != 0) return false;
                if (board.BitboardFor(PieceType.Rook, pieceColor) != 0) return false;
                if (board.BitboardFor(PieceType.Queen, pieceColor) != 0) return false;
            }
            int whiteMinorPieceCount = Bitboard.CountSetBits(board.BitboardFor(PieceType.Bishop, PieceColor.White))
                                     + Bitboard.CountSetBits(board.BitboardFor(PieceType.Knight, PieceColor.White));
            int blackMinorPieceCount = Bitboard.CountSetBits(board.BitboardFor(PieceType.Bishop, PieceColor.Black))
                                     + Bitboard.CountSetBits(board.BitboardFor(PieceType.Knight, PieceColor.Black));
            return whiteMinorPieceCount <= 1 && blackMinorPieceCount <= 1;
        }
    }
}
