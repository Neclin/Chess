using System.Collections.Generic;
using System.Text;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Notation
{
    public static class San
    {
        public static string ToSan(BoardState boardBeforeMove, Move move)
        {
            if ((move.Flags & MoveFlags.CastleKingside) != 0)
                return "O-O" + CheckOrMateSuffix(boardBeforeMove, move);
            if ((move.Flags & MoveFlags.CastleQueenside) != 0)
                return "O-O-O" + CheckOrMateSuffix(boardBeforeMove, move);

            var movingPieceType = boardBeforeMove.PieceAt(move.FromSquare, out var movingPieceColor);
            var sanBuilder = new StringBuilder();

            if (movingPieceType == PieceType.Pawn)
            {
                if (move.IsCapture)
                {
                    sanBuilder.Append((char)('a' + Square.FileIndex(move.FromSquare)));
                    sanBuilder.Append('x');
                }
                sanBuilder.Append(Square.ToAlgebraic(move.ToSquare));
                if (move.IsPromotion)
                    sanBuilder.Append('=').Append(PieceLetter(move.PromotionPiece));
            }
            else
            {
                sanBuilder.Append(PieceLetter(movingPieceType));
                AppendDisambiguation(sanBuilder, boardBeforeMove, move, movingPieceType, movingPieceColor);
                if (move.IsCapture) sanBuilder.Append('x');
                sanBuilder.Append(Square.ToAlgebraic(move.ToSquare));
            }

            sanBuilder.Append(CheckOrMateSuffix(boardBeforeMove, move));
            return sanBuilder.ToString();
        }

        private static char PieceLetter(PieceType pieceType) => pieceType switch
        {
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook => 'R',
            PieceType.Queen => 'Q',
            PieceType.King => 'K',
            _ => '?'
        };

        private static void AppendDisambiguation(
            StringBuilder sanBuilder,
            BoardState boardBeforeMove,
            Move move,
            PieceType movingPieceType,
            PieceColor movingPieceColor)
        {
            var allLegalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(boardBeforeMove, allLegalMoves);
            int otherSamePieceReachingDestinationCount = 0;
            int otherSamePieceOnSameFileCount = 0;
            int otherSamePieceOnSameRankCount = 0;
            int fromFileIndex = Square.FileIndex(move.FromSquare);
            int fromRankIndex = Square.RankIndex(move.FromSquare);

            foreach (var candidateMove in allLegalMoves)
            {
                if (candidateMove.FromSquare == move.FromSquare) continue;
                if (candidateMove.ToSquare != move.ToSquare) continue;
                var candidatePieceType = boardBeforeMove.PieceAt(candidateMove.FromSquare, out var candidatePieceColor);
                if (candidatePieceType != movingPieceType) continue;
                if (candidatePieceColor != movingPieceColor) continue;
                otherSamePieceReachingDestinationCount++;
                if (Square.FileIndex(candidateMove.FromSquare) == fromFileIndex) otherSamePieceOnSameFileCount++;
                if (Square.RankIndex(candidateMove.FromSquare) == fromRankIndex) otherSamePieceOnSameRankCount++;
            }

            if (otherSamePieceReachingDestinationCount == 0) return;

            if (otherSamePieceOnSameFileCount == 0)
                sanBuilder.Append((char)('a' + fromFileIndex));
            else if (otherSamePieceOnSameRankCount == 0)
                sanBuilder.Append((char)('1' + fromRankIndex));
            else
            {
                sanBuilder.Append((char)('a' + fromFileIndex));
                sanBuilder.Append((char)('1' + fromRankIndex));
            }
        }

        private static string CheckOrMateSuffix(BoardState boardBeforeMove, Move move)
        {
            var boardAfterMove = boardBeforeMove.Clone();
            MoveExecutor.MakeMove(boardAfterMove, move);
            ulong defenderKingBitboard = boardAfterMove.BitboardFor(PieceType.King, boardAfterMove.SideToMove);
            if (defenderKingBitboard == 0) return "";
            int defenderKingSquare = Bitboard.LowestSetBitIndex(defenderKingBitboard);
            var attackerColor = boardAfterMove.SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
            bool defenderInCheck = MoveGenerator.IsSquareAttacked(boardAfterMove, defenderKingSquare, attackerColor);
            if (!defenderInCheck) return "";
            var legalMovesAfter = new List<Move>(64);
            MoveGenerator.GenerateLegal(boardAfterMove, legalMovesAfter);
            return legalMovesAfter.Count == 0 ? "#" : "+";
        }
    }
}
