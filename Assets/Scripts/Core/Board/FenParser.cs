using System;
using System.Text;

namespace Chess.Core.Board
{
    public static class FenParser
    {
        public static BoardState Parse(string fen)
        {
            var fenFields = fen.Split(' ');
            if (fenFields.Length < 4) throw new ArgumentException("FEN: need at least 4 fields");

            var board = new BoardState();
            int rankIndex = 7, fileIndex = 0;
            foreach (char fenChar in fenFields[0])
            {
                if (fenChar == '/') { rankIndex--; fileIndex = 0; }
                else if (char.IsDigit(fenChar)) fileIndex += fenChar - '0';
                else
                {
                    PieceColor pieceColor = char.IsUpper(fenChar) ? PieceColor.White : PieceColor.Black;
                    PieceType pieceType = char.ToLower(fenChar) switch
                    {
                        'p' => PieceType.Pawn,
                        'n' => PieceType.Knight,
                        'b' => PieceType.Bishop,
                        'r' => PieceType.Rook,
                        'q' => PieceType.Queen,
                        'k' => PieceType.King,
                        _ => throw new ArgumentException($"FEN: unknown piece '{fenChar}'")
                    };
                    board.PlacePiece(pieceType, pieceColor, Square.FromFileAndRank(fileIndex, rankIndex));
                    fileIndex++;
                }
            }

            board.SideToMove = fenFields[1] == "w" ? PieceColor.White : PieceColor.Black;

            board.Castling = CastlingRights.None;
            if (fenFields[2] != "-")
                foreach (char castlingChar in fenFields[2])
                    board.Castling |= castlingChar switch
                    {
                        'K' => CastlingRights.WhiteKingside,
                        'Q' => CastlingRights.WhiteQueenside,
                        'k' => CastlingRights.BlackKingside,
                        'q' => CastlingRights.BlackQueenside,
                        _ => CastlingRights.None
                    };

            board.EnPassantSquare = fenFields[3] == "-" ? -1 : Square.FromAlgebraic(fenFields[3]);
            board.HalfmoveClock = fenFields.Length > 4 ? int.Parse(fenFields[4]) : 0;
            board.FullmoveNumber = fenFields.Length > 5 ? int.Parse(fenFields[5]) : 1;
            return board;
        }

        public static string ToFen(BoardState board)
        {
            var fenBuilder = new StringBuilder();
            for (int rankIndex = 7; rankIndex >= 0; rankIndex--)
            {
                int emptyFileCount = 0;
                for (int fileIndex = 0; fileIndex < 8; fileIndex++)
                {
                    int squareIndex = Square.FromFileAndRank(fileIndex, rankIndex);
                    var pieceType = board.PieceAt(squareIndex, out var pieceColor);
                    if (pieceType == PieceType.None) { emptyFileCount++; continue; }
                    if (emptyFileCount > 0) { fenBuilder.Append(emptyFileCount); emptyFileCount = 0; }
                    char pieceChar = pieceType switch
                    {
                        PieceType.Pawn => 'p',
                        PieceType.Knight => 'n',
                        PieceType.Bishop => 'b',
                        PieceType.Rook => 'r',
                        PieceType.Queen => 'q',
                        PieceType.King => 'k',
                        _ => '?'
                    };
                    fenBuilder.Append(pieceColor == PieceColor.White ? char.ToUpper(pieceChar) : pieceChar);
                }
                if (emptyFileCount > 0) fenBuilder.Append(emptyFileCount);
                if (rankIndex > 0) fenBuilder.Append('/');
            }

            fenBuilder.Append(' ').Append(board.SideToMove == PieceColor.White ? 'w' : 'b');

            fenBuilder.Append(' ');
            if (board.Castling == CastlingRights.None) fenBuilder.Append('-');
            else
            {
                if ((board.Castling & CastlingRights.WhiteKingside) != 0) fenBuilder.Append('K');
                if ((board.Castling & CastlingRights.WhiteQueenside) != 0) fenBuilder.Append('Q');
                if ((board.Castling & CastlingRights.BlackKingside) != 0) fenBuilder.Append('k');
                if ((board.Castling & CastlingRights.BlackQueenside) != 0) fenBuilder.Append('q');
            }

            fenBuilder.Append(' ').Append(board.EnPassantSquare < 0 ? "-" : Square.ToAlgebraic(board.EnPassantSquare));
            fenBuilder.Append(' ').Append(board.HalfmoveClock);
            fenBuilder.Append(' ').Append(board.FullmoveNumber);
            return fenBuilder.ToString();
        }
    }
}
