using System.Collections.Generic;
using Chess.Core.Board;

namespace Chess.Core.Moves
{
    public static class MoveGenerator
    {
        public static void GeneratePseudoLegal(BoardState board, List<Move> moves)
        {
            PieceColor sideToMove = board.SideToMove;
            ulong ownOccupancy = sideToMove == PieceColor.White ? board.WhiteOccupancy : board.BlackOccupancy;
            ulong opponentOccupancy = sideToMove == PieceColor.White ? board.BlackOccupancy : board.WhiteOccupancy;
            ulong allOccupancy = board.AllOccupancy;

            GeneratePawnMoves(board, moves, sideToMove, opponentOccupancy, allOccupancy);
            GenerateKnightMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy);
            GenerateBishopMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy, allOccupancy);
            GenerateRookMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy, allOccupancy);
            GenerateQueenMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy, allOccupancy);
            GenerateKingMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy);
            GenerateCastlingMoves(board, moves, sideToMove, allOccupancy);
        }

        public static void GenerateLegal(BoardState board, List<Move> moves)
        {
            var pseudoLegalMoves = new List<Move>(64);
            GeneratePseudoLegal(board, pseudoLegalMoves);
            PieceColor sideToMove = board.SideToMove;
            PieceColor opponentColor = sideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
            foreach (var move in pseudoLegalMoves)
            {
                var undoInfo = MoveExecutor.MakeMove(board, move);
                ulong kingBitboard = board.BitboardFor(PieceType.King, sideToMove);
                bool isLegal = kingBitboard != 0 && !IsSquareAttacked(board, Bitboard.LowestSetBitIndex(kingBitboard), opponentColor);
                MoveExecutor.UnmakeMove(board, move, undoInfo);
                if (isLegal) moves.Add(move);
            }
        }

        private static void GenerateCastlingMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong allOccupancy)
        {
            PieceColor opponentColor = sideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
            if (sideToMove == PieceColor.White)
            {
                const int kingHomeE1 = 4;
                const int squareF1 = 5;
                const int squareG1 = 6;
                const int squareD1 = 3;
                const int squareC1 = 2;
                const int squareB1 = 1;
                if ((board.Castling & CastlingRights.WhiteKingside) != 0
                    && (allOccupancy & ((1UL << squareF1) | (1UL << squareG1))) == 0
                    && !IsSquareAttacked(board, kingHomeE1, opponentColor)
                    && !IsSquareAttacked(board, squareF1, opponentColor)
                    && !IsSquareAttacked(board, squareG1, opponentColor))
                    moves.Add(new Move(kingHomeE1, squareG1, MoveFlags.CastleKingside));
                if ((board.Castling & CastlingRights.WhiteQueenside) != 0
                    && (allOccupancy & ((1UL << squareB1) | (1UL << squareC1) | (1UL << squareD1))) == 0
                    && !IsSquareAttacked(board, kingHomeE1, opponentColor)
                    && !IsSquareAttacked(board, squareD1, opponentColor)
                    && !IsSquareAttacked(board, squareC1, opponentColor))
                    moves.Add(new Move(kingHomeE1, squareC1, MoveFlags.CastleQueenside));
            }
            else
            {
                const int kingHomeE8 = 60;
                const int squareF8 = 61;
                const int squareG8 = 62;
                const int squareD8 = 59;
                const int squareC8 = 58;
                const int squareB8 = 57;
                if ((board.Castling & CastlingRights.BlackKingside) != 0
                    && (allOccupancy & ((1UL << squareF8) | (1UL << squareG8))) == 0
                    && !IsSquareAttacked(board, kingHomeE8, opponentColor)
                    && !IsSquareAttacked(board, squareF8, opponentColor)
                    && !IsSquareAttacked(board, squareG8, opponentColor))
                    moves.Add(new Move(kingHomeE8, squareG8, MoveFlags.CastleKingside));
                if ((board.Castling & CastlingRights.BlackQueenside) != 0
                    && (allOccupancy & ((1UL << squareB8) | (1UL << squareC8) | (1UL << squareD8))) == 0
                    && !IsSquareAttacked(board, kingHomeE8, opponentColor)
                    && !IsSquareAttacked(board, squareD8, opponentColor)
                    && !IsSquareAttacked(board, squareC8, opponentColor))
                    moves.Add(new Move(kingHomeE8, squareC8, MoveFlags.CastleQueenside));
            }
        }

        public static bool IsSquareAttacked(BoardState board, int squareIndex, PieceColor byColor)
        {
            ulong allOccupancy = board.AllOccupancy;
            PieceColor opposingPawnLookupColor = byColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            if ((PrecomputedAttacks.PawnAttacks[(int)opposingPawnLookupColor, squareIndex] & board.BitboardFor(PieceType.Pawn, byColor)) != 0) return true;
            if ((PrecomputedAttacks.KnightAttacks[squareIndex] & board.BitboardFor(PieceType.Knight, byColor)) != 0) return true;
            if ((PrecomputedAttacks.KingAttacks[squareIndex] & board.BitboardFor(PieceType.King, byColor)) != 0) return true;
            ulong bishopsAndQueens = board.BitboardFor(PieceType.Bishop, byColor) | board.BitboardFor(PieceType.Queen, byColor);
            if ((SlidingAttacks.Bishop(squareIndex, allOccupancy) & bishopsAndQueens) != 0) return true;
            ulong rooksAndQueens = board.BitboardFor(PieceType.Rook, byColor) | board.BitboardFor(PieceType.Queen, byColor);
            if ((SlidingAttacks.Rook(squareIndex, allOccupancy) & rooksAndQueens) != 0) return true;
            return false;
        }

        private static void GenerateKnightMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy)
        {
            ulong knightBitboard = board.BitboardFor(PieceType.Knight, sideToMove);
            while (knightBitboard != 0)
            {
                int fromSquare = Bitboard.PopLowestSetBitIndex(ref knightBitboard);
                ulong targetBitboard = PrecomputedAttacks.KnightAttacks[fromSquare] & ~ownOccupancy;
                EmitTargets(moves, fromSquare, targetBitboard, opponentOccupancy);
            }
        }

        private static void GenerateKingMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy)
        {
            ulong kingBitboard = board.BitboardFor(PieceType.King, sideToMove);
            if (kingBitboard == 0) return;
            int fromSquare = Bitboard.LowestSetBitIndex(kingBitboard);
            ulong targetBitboard = PrecomputedAttacks.KingAttacks[fromSquare] & ~ownOccupancy;
            EmitTargets(moves, fromSquare, targetBitboard, opponentOccupancy);
        }

        private static void GenerateBishopMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy, ulong allOccupancy)
            => GenerateSlidingMoves(board.BitboardFor(PieceType.Bishop, sideToMove), moves, ownOccupancy, opponentOccupancy, allOccupancy, includeBishopRays: true, includeRookRays: false);

        private static void GenerateRookMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy, ulong allOccupancy)
            => GenerateSlidingMoves(board.BitboardFor(PieceType.Rook, sideToMove), moves, ownOccupancy, opponentOccupancy, allOccupancy, includeBishopRays: false, includeRookRays: true);

        private static void GenerateQueenMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy, ulong allOccupancy)
            => GenerateSlidingMoves(board.BitboardFor(PieceType.Queen, sideToMove), moves, ownOccupancy, opponentOccupancy, allOccupancy, includeBishopRays: true, includeRookRays: true);

        private static void GenerateSlidingMoves(ulong pieceBitboard, List<Move> moves, ulong ownOccupancy, ulong opponentOccupancy, ulong allOccupancy, bool includeBishopRays, bool includeRookRays)
        {
            while (pieceBitboard != 0)
            {
                int fromSquare = Bitboard.PopLowestSetBitIndex(ref pieceBitboard);
                ulong attackBitboard = 0UL;
                if (includeBishopRays) attackBitboard |= SlidingAttacks.Bishop(fromSquare, allOccupancy);
                if (includeRookRays) attackBitboard |= SlidingAttacks.Rook(fromSquare, allOccupancy);
                attackBitboard &= ~ownOccupancy;
                EmitTargets(moves, fromSquare, attackBitboard, opponentOccupancy);
            }
        }

        private static void EmitTargets(List<Move> moves, int fromSquare, ulong targetBitboard, ulong opponentOccupancy)
        {
            while (targetBitboard != 0)
            {
                int toSquare = Bitboard.PopLowestSetBitIndex(ref targetBitboard);
                MoveFlags flags = (opponentOccupancy & (1UL << toSquare)) != 0 ? MoveFlags.Capture : MoveFlags.None;
                moves.Add(new Move(fromSquare, toSquare, flags));
            }
        }

        private static void GeneratePawnMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong opponentOccupancy, ulong allOccupancy)
        {
            ulong pawnBitboard = board.BitboardFor(PieceType.Pawn, sideToMove);
            int forwardOffset = sideToMove == PieceColor.White ? 8 : -8;
            int startRank = sideToMove == PieceColor.White ? 1 : 6;
            int promotionRank = sideToMove == PieceColor.White ? 7 : 0;

            while (pawnBitboard != 0)
            {
                int fromSquare = Bitboard.PopLowestSetBitIndex(ref pawnBitboard);
                int oneStepSquare = fromSquare + forwardOffset;
                if (oneStepSquare >= 0 && oneStepSquare < 64 && (allOccupancy & (1UL << oneStepSquare)) == 0)
                {
                    if (Square.RankIndex(oneStepSquare) == promotionRank)
                        AddPromotions(moves, fromSquare, oneStepSquare, MoveFlags.None);
                    else
                        moves.Add(new Move(fromSquare, oneStepSquare, MoveFlags.None));

                    if (Square.RankIndex(fromSquare) == startRank)
                    {
                        int twoStepSquare = oneStepSquare + forwardOffset;
                        if ((allOccupancy & (1UL << twoStepSquare)) == 0)
                            moves.Add(new Move(fromSquare, twoStepSquare, MoveFlags.DoublePawnPush));
                    }
                }

                ulong captureBitboard = PrecomputedAttacks.PawnAttacks[(int)sideToMove, fromSquare] & opponentOccupancy;
                while (captureBitboard != 0)
                {
                    int toSquare = Bitboard.PopLowestSetBitIndex(ref captureBitboard);
                    if (Square.RankIndex(toSquare) == promotionRank)
                        AddPromotions(moves, fromSquare, toSquare, MoveFlags.Capture);
                    else
                        moves.Add(new Move(fromSquare, toSquare, MoveFlags.Capture));
                }

                if (board.EnPassantSquare >= 0)
                {
                    ulong enPassantTargetBit = 1UL << board.EnPassantSquare;
                    if ((PrecomputedAttacks.PawnAttacks[(int)sideToMove, fromSquare] & enPassantTargetBit) != 0)
                        moves.Add(new Move(fromSquare, board.EnPassantSquare, MoveFlags.Capture | MoveFlags.EnPassant));
                }
            }
        }

        private static void AddPromotions(List<Move> moves, int fromSquare, int toSquare, MoveFlags baseFlags)
        {
            moves.Add(new Move(fromSquare, toSquare, baseFlags | MoveFlags.Promotion, PieceType.Queen));
            moves.Add(new Move(fromSquare, toSquare, baseFlags | MoveFlags.Promotion, PieceType.Rook));
            moves.Add(new Move(fromSquare, toSquare, baseFlags | MoveFlags.Promotion, PieceType.Bishop));
            moves.Add(new Move(fromSquare, toSquare, baseFlags | MoveFlags.Promotion, PieceType.Knight));
        }
    }
}
