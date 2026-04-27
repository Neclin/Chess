namespace Chess.Core.Board
{
    public enum PieceColor : byte { White = 0, Black = 1 }

    public enum PieceType : byte { None = 0, Pawn, Knight, Bishop, Rook, Queen, King }

    [System.Flags]
    public enum CastlingRights : byte
    {
        None = 0,
        WhiteKingside = 1,
        WhiteQueenside = 2,
        BlackKingside = 4,
        BlackQueenside = 8,
        All = 15
    }
}
