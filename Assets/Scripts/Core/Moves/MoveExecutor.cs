using Chess.Core.Board;

namespace Chess.Core.Moves
{
    public readonly struct UndoInfo
    {
        public readonly PieceType MovedPiece;
        public readonly PieceType CapturedPiece;
        public readonly PieceColor CapturedColor;
        public readonly int CapturedSquare;
        public readonly CastlingRights PreviousCastlingRights;
        public readonly int PreviousEnPassantSquare;
        public readonly int PreviousHalfmoveClock;
        public readonly ulong PreviousZobristKey;

        public UndoInfo(
            PieceType movedPiece,
            PieceType capturedPiece,
            PieceColor capturedColor,
            int capturedSquare,
            CastlingRights previousCastlingRights,
            int previousEnPassantSquare,
            int previousHalfmoveClock,
            ulong previousZobristKey)
        {
            MovedPiece = movedPiece;
            CapturedPiece = capturedPiece;
            CapturedColor = capturedColor;
            CapturedSquare = capturedSquare;
            PreviousCastlingRights = previousCastlingRights;
            PreviousEnPassantSquare = previousEnPassantSquare;
            PreviousHalfmoveClock = previousHalfmoveClock;
            PreviousZobristKey = previousZobristKey;
        }
    }

    public static class MoveExecutor
    {
        public static UndoInfo MakeMove(BoardState board, Move move)
        {
            PieceColor sideToMove = board.SideToMove;
            PieceColor opponentColor = sideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
            PieceType movedPiece = board.PieceAt(move.FromSquare, out _);

            PieceType capturedPiece = PieceType.None;
            int capturedSquare = move.ToSquare;
            if ((move.Flags & MoveFlags.EnPassant) != 0)
            {
                capturedSquare = sideToMove == PieceColor.White ? move.ToSquare - 8 : move.ToSquare + 8;
                capturedPiece = PieceType.Pawn;
            }
            else if ((move.Flags & MoveFlags.Capture) != 0)
            {
                capturedPiece = board.PieceAt(move.ToSquare, out _);
            }

            var undoInfo = new UndoInfo(
                movedPiece,
                capturedPiece,
                opponentColor,
                capturedSquare,
                board.Castling,
                board.EnPassantSquare,
                board.HalfmoveClock,
                board.ZobristKey);

            if (capturedPiece != PieceType.None)
                board.RemovePiece(capturedPiece, opponentColor, capturedSquare);
            board.RemovePiece(movedPiece, sideToMove, move.FromSquare);

            if (move.IsPromotion)
                board.PlacePiece(move.PromotionPiece, sideToMove, move.ToSquare);
            else
                board.PlacePiece(movedPiece, sideToMove, move.ToSquare);

            if ((move.Flags & MoveFlags.CastleKingside) != 0)
            {
                int rookFromSquare = sideToMove == PieceColor.White ? 7 : 63;
                int rookToSquare = sideToMove == PieceColor.White ? 5 : 61;
                board.RemovePiece(PieceType.Rook, sideToMove, rookFromSquare);
                board.PlacePiece(PieceType.Rook, sideToMove, rookToSquare);
            }
            else if ((move.Flags & MoveFlags.CastleQueenside) != 0)
            {
                int rookFromSquare = sideToMove == PieceColor.White ? 0 : 56;
                int rookToSquare = sideToMove == PieceColor.White ? 3 : 59;
                board.RemovePiece(PieceType.Rook, sideToMove, rookFromSquare);
                board.PlacePiece(PieceType.Rook, sideToMove, rookToSquare);
            }

            UpdateCastlingRights(board, movedPiece, sideToMove, move.FromSquare, move.ToSquare, capturedPiece);

            board.EnPassantSquare = (move.Flags & MoveFlags.DoublePawnPush) != 0
                ? (sideToMove == PieceColor.White ? move.FromSquare + 8 : move.FromSquare - 8)
                : -1;

            board.HalfmoveClock = (movedPiece == PieceType.Pawn || capturedPiece != PieceType.None)
                ? 0
                : board.HalfmoveClock + 1;
            if (sideToMove == PieceColor.Black) board.FullmoveNumber++;
            board.SideToMove = opponentColor;

            return undoInfo;
        }

        public static void UnmakeMove(BoardState board, Move move, UndoInfo undoInfo)
        {
            PieceColor sideThatMoved = board.SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
            board.SideToMove = sideThatMoved;
            if (sideThatMoved == PieceColor.Black) board.FullmoveNumber--;

            if (move.IsPromotion)
                board.RemovePiece(move.PromotionPiece, sideThatMoved, move.ToSquare);
            else
                board.RemovePiece(undoInfo.MovedPiece, sideThatMoved, move.ToSquare);
            board.PlacePiece(undoInfo.MovedPiece, sideThatMoved, move.FromSquare);

            if (undoInfo.CapturedPiece != PieceType.None)
                board.PlacePiece(undoInfo.CapturedPiece, undoInfo.CapturedColor, undoInfo.CapturedSquare);

            if ((move.Flags & MoveFlags.CastleKingside) != 0)
            {
                int rookFromSquare = sideThatMoved == PieceColor.White ? 7 : 63;
                int rookToSquare = sideThatMoved == PieceColor.White ? 5 : 61;
                board.RemovePiece(PieceType.Rook, sideThatMoved, rookToSquare);
                board.PlacePiece(PieceType.Rook, sideThatMoved, rookFromSquare);
            }
            else if ((move.Flags & MoveFlags.CastleQueenside) != 0)
            {
                int rookFromSquare = sideThatMoved == PieceColor.White ? 0 : 56;
                int rookToSquare = sideThatMoved == PieceColor.White ? 3 : 59;
                board.RemovePiece(PieceType.Rook, sideThatMoved, rookToSquare);
                board.PlacePiece(PieceType.Rook, sideThatMoved, rookFromSquare);
            }

            board.Castling = undoInfo.PreviousCastlingRights;
            board.EnPassantSquare = undoInfo.PreviousEnPassantSquare;
            board.HalfmoveClock = undoInfo.PreviousHalfmoveClock;
            board.ZobristKey = undoInfo.PreviousZobristKey;
        }

        private static void UpdateCastlingRights(
            BoardState board,
            PieceType movedPiece,
            PieceColor sideToMove,
            int fromSquare,
            int toSquare,
            PieceType capturedPiece)
        {
            if (movedPiece == PieceType.King)
                board.Castling &= sideToMove == PieceColor.White
                    ? ~(CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside)
                    : ~(CastlingRights.BlackKingside | CastlingRights.BlackQueenside);
            if (movedPiece == PieceType.Rook)
            {
                if (sideToMove == PieceColor.White && fromSquare == 0) board.Castling &= ~CastlingRights.WhiteQueenside;
                if (sideToMove == PieceColor.White && fromSquare == 7) board.Castling &= ~CastlingRights.WhiteKingside;
                if (sideToMove == PieceColor.Black && fromSquare == 56) board.Castling &= ~CastlingRights.BlackQueenside;
                if (sideToMove == PieceColor.Black && fromSquare == 63) board.Castling &= ~CastlingRights.BlackKingside;
            }
            if (capturedPiece == PieceType.Rook)
            {
                if (toSquare == 0) board.Castling &= ~CastlingRights.WhiteQueenside;
                if (toSquare == 7) board.Castling &= ~CastlingRights.WhiteKingside;
                if (toSquare == 56) board.Castling &= ~CastlingRights.BlackQueenside;
                if (toSquare == 63) board.Castling &= ~CastlingRights.BlackKingside;
            }
        }
    }
}
