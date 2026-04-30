using Chess.Core.Board;

namespace Chess.Core.Moves
{
    [System.Flags]
    public enum MoveFlags : byte
    {
        None = 0,
        Capture = 1,
        EnPassant = 2,
        DoublePawnPush = 4,
        CastleKingside = 8,
        CastleQueenside = 16,
        Promotion = 32
    }

    public readonly struct Move
    {
        public readonly byte FromSquare;
        public readonly byte ToSquare;
        public readonly MoveFlags Flags;
        public readonly PieceType PromotionPiece;

        public Move(int fromSquare, int toSquare, MoveFlags flags = MoveFlags.None, PieceType promotionPiece = PieceType.None)
        {
            FromSquare = (byte)fromSquare;
            ToSquare = (byte)toSquare;
            Flags = flags;
            PromotionPiece = promotionPiece;
        }

        public bool IsCapture => (Flags & MoveFlags.Capture) != 0;
        public bool IsPromotion => (Flags & MoveFlags.Promotion) != 0;
        public bool IsCastle => (Flags & (MoveFlags.CastleKingside | MoveFlags.CastleQueenside)) != 0;

        public override string ToString() => $"{Square.ToAlgebraic(FromSquare)}{Square.ToAlgebraic(ToSquare)}{PromotionSuffix()}";

        private string PromotionSuffix() => PromotionPiece switch
        {
            PieceType.Queen => "q",
            PieceType.Rook => "r",
            PieceType.Bishop => "b",
            PieceType.Knight => "n",
            _ => ""
        };
    }
}
