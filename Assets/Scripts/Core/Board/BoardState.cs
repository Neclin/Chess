namespace Chess.Core.Board
{
    public sealed class BoardState
    {
        private readonly ulong[] _pieceBitboards = new ulong[12];

        public PieceColor SideToMove;
        public CastlingRights Castling;
        public int EnPassantSquare = -1;
        public int HalfmoveClock;
        public int FullmoveNumber = 1;
        public ulong ZobristKey;

        public ulong BitboardFor(PieceType pieceType, PieceColor pieceColor) => _pieceBitboards[BitboardIndexFor(pieceType, pieceColor)];
        public ulong WhiteOccupancy => _pieceBitboards[0] | _pieceBitboards[1] | _pieceBitboards[2] | _pieceBitboards[3] | _pieceBitboards[4] | _pieceBitboards[5];
        public ulong BlackOccupancy => _pieceBitboards[6] | _pieceBitboards[7] | _pieceBitboards[8] | _pieceBitboards[9] | _pieceBitboards[10] | _pieceBitboards[11];
        public ulong AllOccupancy => WhiteOccupancy | BlackOccupancy;

        public void PlacePiece(PieceType pieceType, PieceColor pieceColor, int squareIndex) => _pieceBitboards[BitboardIndexFor(pieceType, pieceColor)] |= 1UL << squareIndex;
        public void RemovePiece(PieceType pieceType, PieceColor pieceColor, int squareIndex) => _pieceBitboards[BitboardIndexFor(pieceType, pieceColor)] &= ~(1UL << squareIndex);

        public PieceType PieceAt(int squareIndex, out PieceColor pieceColor)
        {
            ulong squareMask = 1UL << squareIndex;
            for (int bitboardIndex = 0; bitboardIndex < 12; bitboardIndex++)
            {
                if ((_pieceBitboards[bitboardIndex] & squareMask) != 0)
                {
                    pieceColor = (PieceColor)(bitboardIndex / 6);
                    return (PieceType)((bitboardIndex % 6) + 1);
                }
            }
            pieceColor = PieceColor.White;
            return PieceType.None;
        }

        public BoardState Clone()
        {
            var clonedBoard = new BoardState
            {
                SideToMove = SideToMove,
                Castling = Castling,
                EnPassantSquare = EnPassantSquare,
                HalfmoveClock = HalfmoveClock,
                FullmoveNumber = FullmoveNumber,
                ZobristKey = ZobristKey
            };
            System.Array.Copy(_pieceBitboards, clonedBoard._pieceBitboards, 12);
            return clonedBoard;
        }

        private static int BitboardIndexFor(PieceType pieceType, PieceColor pieceColor) => (int)pieceColor * 6 + ((int)pieceType - 1);
    }
}
